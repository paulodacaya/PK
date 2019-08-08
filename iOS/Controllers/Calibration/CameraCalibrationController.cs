using System;
using System.Text;
using System.Threading.Tasks;
using ARKit;
using CoreGraphics;
using Foundation;
using PK.iOS.Helpers;
using SceneKit;
using UIKit;
using Xamarin.Essentials;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class CameraCalibrationController : UIViewController, IARSCNViewDelegate, IARSessionDelegate
   {
      private bool isRestartAvailable = true; // Prevents restarting the session while a restart is in progress.

      private ARSCNView sceneView;
      private ControlAndStatusController controlAndStatusController;

      public CameraCalibrationController( )
      {
      }

      public override bool PrefersStatusBarHidden( )
      {
         return true;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupViews( );
      }

      public override void ViewWillAppear( bool animated )
      {
         base.ViewWillAppear( animated );

         OrienationHelper.LockOrientation( UIInterfaceOrientationMask.All );
      }

      public override void ViewDidAppear( bool animated )
      {
         base.ViewDidAppear( animated );

         // Prevent the screen from dimming to avoid interuppting the AR experience.
         UIApplication.SharedApplication.IdleTimerDisabled = true;

         // Start AR caibration experience.
         ResetTracking( );

         // Start guidance animation prompt.
         controlAndStatusController.StartGuidancePrompt( );
      }

      public override void ViewDidDisappear( bool animated )
      {
         base.ViewDidDisappear( animated );

         sceneView.Session.Pause( );
      }

      private void SetupViews( )
      {
         sceneView = new ARSCNView( );

         sceneView.Delegate = this;
         sceneView.Session.Delegate = this;

         controlAndStatusController = new ControlAndStatusController( );
         controlAndStatusController.OnBackButtonTouchUpInside += HandleBackButtonTouchUpInside;
         controlAndStatusController.OnRestartButtonTouchUpInside += HandleRestartButtonTouchUpInside;

         View.AddSubviews( sceneView, controlAndStatusController.View );

         AddChildViewController( controlAndStatusController );
         controlAndStatusController.DidMoveToParentViewController( this );

         sceneView.FillSuperview( );

         controlAndStatusController.View.Anchor( leading: View.LeadingAnchor, top: View.LayoutMarginsGuide.TopAnchor,
            trailing: View.TrailingAnchor, bottom: View.LayoutMarginsGuide.BottomAnchor );
      }

      private void ResetTracking( )
      {
         // Load reference images
         var trackedImages = ARReferenceImage.GetReferenceImagesInGroup( name: "AR Resources", bundle: NSBundle.MainBundle );

         if( trackedImages == null )
            throw new Exception( "iOS - Reference images could not be loaded from Assets.xcassts." );

         var configuration = new ARImageTrackingConfiguration {
            TrackingImages = trackedImages,
            MaximumNumberOfTrackedImages = 1
         };

         sceneView.Session.Run( configuration, options: ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors );

         controlAndStatusController.ShowGuidancePrompt( );
      }

      public void RestartARExperience( )
      {
         if( !isRestartAvailable )
         {
            Console.WriteLine( "iOS - A restart is in progress." );
            return;
         }

         Console.WriteLine( "iOS - Restarting AR Experience." );
         isRestartAvailable = false;

         ResetTracking( );

         // Allow for sceneView to restart before setting isRestartAvailable flag to true
         Task.Delay( 5000 ).ContinueWith( task => {
            InvokeOnMainThread( ( ) => {
               Console.WriteLine( "iOS - Restarting AR Experience completed." );
               isRestartAvailable = true;
            } );
         } );
      }

      #region Event Handlers
      private void HandleBackButtonTouchUpInside( object sender, EventArgs e )
      {
         // Lock orientation back to default (portrait) and force rotate it.
         OrienationHelper.LockOrientation( UIInterfaceOrientationMask.Portrait, rotateToOrientation: UIInterfaceOrientationMask.Portrait );

         DismissViewController( animated: true, completionHandler: null );
      }

      private void HandleRestartButtonTouchUpInside( object sender, EventArgs e ) => RestartARExperience( );
      #endregion

      #region IARSCNViewDelegate
      [Export( "renderer:didAddNode:forAnchor:" )]
      public void DidAddNode( ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor )
      {
         Console.WriteLine( "iOS - DidAddNode" );

         var imageAnchor = anchor as ARImageAnchor;

         var plane = new SCNPlane {
            Width = imageAnchor.ReferenceImage.PhysicalSize.Width,
            Height = imageAnchor.ReferenceImage.PhysicalSize.Height,
         };

         var planeNode = new SCNNode {
            Geometry = plane,
            Opacity = 0.6f,
         };

         /*
          * 'SCNPlane' is vertically oriented in its local coordinate space, but
          * 'ARImageAnchor' assumes the image is horizontal in its local space, so
          * rotate the plane to match.
          */
         var eulersAngles = planeNode.EulerAngles;
         eulersAngles.X = ( float )-Math.PI / 2f;
         planeNode.EulerAngles = eulersAngles;

         // Add the plane visualization to the scene.
         node.AddChildNode( planeNode );
      }

      [Export( "renderer:didUpdateNode:forAnchor:" )]
      public void DidUpdateNode( ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor )
      {
         // Using Anchors to determine distance
         using( var cameraFrame = sceneView.Session.CurrentFrame )
         {
            var imageAnchor = anchor as ARImageAnchor;
            var cameraAnchor = cameraFrame.Camera;

            var imageAnchorPosition = imageAnchor.Transform.Column3;
            var cameraAnchorPosition = cameraAnchor.Transform.Column3;

            var cameraToImagePosition = cameraAnchorPosition - imageAnchorPosition;

            controlAndStatusController.ShowDistanceMessage( Math.Round( cameraToImagePosition.Length, 2 ).ToString( ) );
         }
      }
      #endregion

      #region IARSessionDelegate
      [Export( "session:didUpdateAnchors:" )]
      public void DidUpdateAnchors( ARSession session, ARAnchor[ ] anchors )
      {
         var imageAnchor = anchors[ 0 ] as ARImageAnchor;

         if( imageAnchor.IsTracked )
            controlAndStatusController.HideGuidancePrompt( );
         else
         {
            controlAndStatusController.ShowGuidancePrompt( );
            controlAndStatusController.HideDistanceMessage( );
         }
      }
      #endregion

      #region IARSessionObserver
      [ Export( "session:cameraDidChangeTrackingState:" ) ]
      public void CameraDidChangeTrackingState( ARSession session, ARCamera camera )
      {
         controlAndStatusController.ShowTrackingQualityInfo( camera.TrackingState, camera.TrackingStateReason );
      }

      [Export( "session:didFailWithError:" )]
      public void DidFail( ARSession session, NSError error )
      {
         var stringBuilder = new StringBuilder( );

         stringBuilder.Append( error.LocalizedDescription );
         stringBuilder.AppendLine( ).AppendLine( );
         stringBuilder.Append( $"Failure Reason: {error.LocalizedFailureReason}" );
         stringBuilder.AppendLine( ).AppendLine( );
         stringBuilder.Append( $"Recovery Suggestion: {error.LocalizedRecoverySuggestion}" );

         ShowErrorMessageDialog( stringBuilder.ToString( ) );
      }

      [Export( "sessionWasInterrupted:" )]
      public void WasInterrupted( ARSession session )
      {
         Console.WriteLine( "iOS - Session was interrupted. Please wait for interuption to end." );
      }

      [Export( "sessionInterruptionEnded:" )]
      public void InterruptionEnded( ARSession session )
      {
         Console.WriteLine( "iOS - Session interuption has ended." );
      }

      [Export( "sessionShouldAttemptRelocalization:" )]
      public bool ShouldAttemptRelocalization( ARSession session ) => true;
      #endregion

      public void ShowErrorMessageDialog( string message )
      {
         var errorDialogController = new DialogController {
            TitleText = "Opps, an error has occured!",
            MessageText = message,
            NegativeButtonHidden = true,
            PositiveText = "Try Again",
         };

         errorDialogController.OnPositiveButtonTouchUpInside += ( sender, e ) => {
            RestartARExperience( );
         };

         PresentViewController( errorDialogController, animated: true, completionHandler: null );
      }

      public void ShowMessage( string message )
      {
         // TODO Implementation
      }
   }

   public class ControlAndStatusController : UIViewController
   {
      public event EventHandler OnBackButtonTouchUpInside;
      public event EventHandler OnRestartButtonTouchUpInside;

      private UIToolbar topToolbar;

      private UIView trackingStateContainerView;
      private UILabel trackingStateLabel;

      private UIStackView guidanceStackView;
      private UILabel guidanceMessageLabel;

      private UIView distanceContainerView;
      private UILabel distanceLabel;

      private UIImageView flashlightImageView;
      private bool IsFlashlightOn;

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupTopToolBar( );
         SetupButtomToolBar( );
         SetupInfoLabels( );
         SetupGuidancePrompt( );
      }

      private void SetupTopToolBar( )
      {
         var backButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
            TintColor = Colors.White,
         };
         backButton.SetImage( Images.ChevronLeft, UIControlState.Normal );
         backButton.WithSquareSize( 22 );
         backButton.TouchUpInside += ( sender, e ) => OnBackButtonTouchUpInside?.Invoke( sender, e );

         var restartButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
            TintColor = Colors.White,
         };
         restartButton.SetImage( Images.Restart, UIControlState.Normal );
         restartButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
         restartButton.WithSquareSize( 22 );
         restartButton.TouchUpInside += ( sender, e ) => OnRestartButtonTouchUpInside?.Invoke( sender, e );

         topToolbar = new UIToolbar {
            Items = new UIBarButtonItem[ ] {
               new UIBarButtonItem( customView: backButton ),
               new UIBarButtonItem( UIBarButtonSystemItem.FlexibleSpace ),
               new UIBarButtonItem( customView: restartButton )
            },
            BackgroundColor = Colors.Clear,
            ClipsToBounds = true,
         };
         topToolbar.SetBackgroundImage( new UIImage( ), UIToolbarPosition.Any, UIBarMetrics.Default );

         View.AddSubview( topToolbar );

         topToolbar.Anchor( leading: View.LeadingAnchor, top: View.LayoutMarginsGuide.TopAnchor, trailing: View.TrailingAnchor );
      }

      private void SetupButtomToolBar( )
      {
         flashlightImageView = new UIImageView {
            UserInteractionEnabled = true,
            Image = Images.FlashlightOff,
            ContentMode = UIViewContentMode.ScaleAspectFit,
            TintColor = Colors.White,
         };
         flashlightImageView.WithSquareSize( 24 );

         var tapGesture = new UITapGestureRecognizer( HandleFlashlightTap );
         flashlightImageView.AddGestureRecognizer( tapGesture );

         var bottomToolbar = new UIToolbar {
            Items = new UIBarButtonItem[ ] {
               new UIBarButtonItem( UIBarButtonSystemItem.FlexibleSpace ),
               new UIBarButtonItem( customView: flashlightImageView )
            },
            BackgroundColor = Colors.Clear,
            ClipsToBounds = true,
         };
         bottomToolbar.SetBackgroundImage( new UIImage( ), UIToolbarPosition.Any, UIBarMetrics.Default );

         View.AddSubview( bottomToolbar );

         bottomToolbar.Anchor( leading: View.LeadingAnchor, bottom: View.LayoutMarginsGuide.BottomAnchor, trailing: View.TrailingAnchor );
      }

      private void SetupInfoLabels( )
      {
         trackingStateContainerView = new UIView {
            BackgroundColor = Colors.OuterSpaceLight,
            ClipsToBounds = true,
            Hidden = true
         };
         trackingStateContainerView.WithCornerRadius( Values.CornerRadius );

         trackingStateLabel = new UILabel {
            Font = Fonts.Medium.WithSize( 16 ),
            TextColor = Colors.White,
            TextAlignment = UITextAlignment.Center
         };

         trackingStateContainerView.AddSubviews( trackingStateLabel );

         trackingStateLabel.FillSuperview( padding: new UIEdgeInsets( 8, 12, 8, 12 ) );

         distanceContainerView = new UIView {
            BackgroundColor = Colors.OuterSpaceLight,
            ClipsToBounds = true,
            Hidden = true
         };
         distanceContainerView.WithCornerRadius( Values.CornerRadius );

         distanceLabel = new UILabel {
            Font = Fonts.Medium.WithSize( 16 ),
            Text = "Awesomeness",
            TextColor = Colors.White,
            TextAlignment = UITextAlignment.Center
         };

         distanceContainerView.AddSubviews( distanceLabel );

         distanceLabel.FillSuperview( padding: new UIEdgeInsets( 8, 12, 8, 12 ) );

         var stackView = VStack(
            trackingStateContainerView,
            distanceContainerView
         ).With( spacing: 12, alignment: UIStackViewAlignment.Center );

         View.AddSubview( stackView );

         stackView.Anchor( centerX: View.CenterXAnchor, top: topToolbar.BottomAnchor );
      }

      private void SetupGuidancePrompt( )
      {
         guidanceMessageLabel = new UILabel {
            Text = "Find the marker",
            TextColor = Colors.White.ColorWithAlpha( 0.8f ),
            Font = Fonts.Medium.WithSize( 26 )
         };

         guidanceStackView = VStack(
            guidanceMessageLabel
         ).With( spacing: 18, alignment: UIStackViewAlignment.Center );
         guidanceStackView.Hidden = true;

         View.AddSubview( guidanceStackView );

         guidanceStackView.CenterInSuperView( );
      }

      private void HandleFlashlightTap( )
      {
         try
         {
            if( IsFlashlightOn )
            {
               Flashlight.TurnOffAsync( );
               flashlightImageView.Image = Images.FlashlightOff;
               flashlightImageView.TintColor = Colors.White;
            }
            else
            {
               Flashlight.TurnOnAsync( );
               flashlightImageView.Image = Images.Flashlight;
               flashlightImageView.TintColor = Colors.OuterSpace;
            }

            IsFlashlightOn = !IsFlashlightOn;
         }
         catch( FeatureNotSupportedException )
         {
            ShowInfoMessage( "Flashlight not supported on your device." );
         }
         catch( PermissionException )
         {
            ShowInfoMessage( "No permissions." );
         }
         catch( Exception )
         {
            ShowInfoMessage( "Unable to turn on/off flashlight." );
         }
      }

      public void StartGuidancePrompt( )
      {
         UIView.AnimateNotify( duration: 1, delay: 0, options: UIViewAnimationOptions.Autoreverse | UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveEaseInOut, animation: ( ) => {
            guidanceMessageLabel.Alpha = 0;
         }, completion: null );
      }

      public void ShowGuidancePrompt( )
      {
         if( !guidanceStackView.Hidden )
            return;

         InvokeOnMainThread( ( ) => {
            guidanceStackView.Hidden = false;
         } );
      }

      public void HideGuidancePrompt( )
      {
         if( guidanceStackView.Hidden )
            return;

         InvokeOnMainThread( ( ) => {
            guidanceStackView.Hidden = true;
         } );
      }

      public void ShowTrackingQualityInfo( ARTrackingState trackingState, ARTrackingStateReason trackingStateReason )
      {
         ShowInfoMessage( ARCameraHelper.PresentationStringForState( trackingState, trackingStateReason ) );
      }

      public void ShowInfoMessage( string message )
      {
         trackingStateLabel.Text = message;

         trackingStateContainerView.Hidden = false;
         trackingStateContainerView.Alpha = 1;

         // Hide Info Message message after delay
         DelayAnimate( animation: ( ) => {
            trackingStateContainerView.Alpha = 0;
         }, completion: finsihed => {
            trackingStateContainerView.Hidden = true;
         } );
      }

      public void ShowDistanceMessage( string distance )
      {
         InvokeOnMainThread( ( ) => {
            distanceLabel.Text = $"{distance}m";

            distanceContainerView.Hidden = false;
            distanceContainerView.Alpha = 1;
         } );
      }

      public void HideDistanceMessage( )
      {
         DelayAnimate( animation: ( ) => {
            distanceContainerView.Alpha = 0;
         }, completion: finished => {
            distanceContainerView.Hidden = true;
         } );
      }

      private void DelayAnimate( Action animation, UICompletionHandler completion )
      {
         Task.Delay( 6000 ).ContinueWith( task => {
            InvokeOnMainThread( ( ) => {
               UIView.AnimateNotify( duration: 0.2, delay: 0, options: UIViewAnimationOptions.CurveEaseInOut, animation, completion );
            } );
         } );
      }
   }

   public static class ARCameraHelper
   {
      public static string PresentationStringForState( ARTrackingState trackingState, ARTrackingStateReason trackingStateReason )
      {
         var message = string.Empty;

         switch( trackingState )
         {
            case ARTrackingState.NotAvailable:
               message = "Tracking Unavailable";
               break;
            case ARTrackingState.Limited:
               message = "Tracking Limited";
               break;
            case ARTrackingState.Normal:
               message = "Tracking Normal";
               break;
         }

         switch( trackingStateReason )
         {
            case ARTrackingStateReason.ExcessiveMotion:
               message += "\nExcessive motion: Try slow down movement.";
               break;
            case ARTrackingStateReason.Initializing:
               message += "\nInitializing";
               break;
            case ARTrackingStateReason.InsufficientFeatures:
               message += "\nLow Detail: Try clearer lighting.";
               break;
            case ARTrackingStateReason.None:
               break;
            case ARTrackingStateReason.Relocalizing:
               message += "\nRecovering: Try returning to previous location.";
               break;
         }

         Console.WriteLine( $"Tracking state changed: {message}" );

         return message;
      }
   }

}
