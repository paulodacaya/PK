using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARKit;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using PK.Helpers;
using PK.Interfaces;
using PK.iOS.Bluetooth;
using PK.iOS.Helpers;
using PK.ViewModels;
using SceneKit;
using UIKit;
using Xamarin.Essentials;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class CameraCalibrationController : UIViewController, IARSCNViewDelegate, IARSessionDelegate, IBluetoothLEAdvertisement, ICameraCalibrationViewModel
   {
      private const string ARResourceImageGroup = "AR Resources";

      private bool isRestartAvailable = true; // Prevents restarting the session while a restart is in progress.
      private readonly bool isRecalibrating;
      private bool canCaptureRSSI;

      private readonly CameraCalibrationViewModel viewModel;

      private ARSCNView sceneView;
      private CameraCalibrationStatusController cameraCalibrationStatusController;

      public CameraCalibrationController( bool isRecalibrating = false )
      {
         viewModel = new CameraCalibrationViewModel( this );

         this.isRecalibrating = isRecalibrating;

         // Start scanning for BLE advertisements
         IOSBluetoothLE.Instance.AdvertisementDelegate = this;
         IOSBluetoothLE.Instance.ScanForAdvertisements( );
      }

      public override UIStatusBarStyle PreferredStatusBarStyle( ) => UIStatusBarStyle.LightContent;

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupSceneViewAndController( );
      }

      public override void ViewDidAppear( bool animated )
      {
         base.ViewDidAppear( animated );

         //// Prevent the screen from dimming to avoid interupting the AR experience.
         //UIApplication.SharedApplication.IdleTimerDisabled = true;

         //// Start AR caibration experience.
         //ResetTracking( );
      }

      public override void ViewDidDisappear( bool animated )
      {
         base.ViewDidDisappear( animated );

         sceneView.Session.Pause( );
      }

      private void SetupSceneViewAndController( )
      {
         sceneView = new ARSCNView( );

         sceneView.Delegate = this;
         sceneView.Session.Delegate = this;

         cameraCalibrationStatusController = new CameraCalibrationStatusController( viewModel );
         cameraCalibrationStatusController.View.PreservesSuperviewLayoutMargins = true;

         AddChildViewController( cameraCalibrationStatusController );
         View.AddSubviews( sceneView, cameraCalibrationStatusController.View );
         cameraCalibrationStatusController.DidMoveToParentViewController( this );

         sceneView.FillSuperview( );

         cameraCalibrationStatusController.View.FillSuperview( );
      }

      private void ResetTracking( )
      {
         // Load reference images
         var trackedImages = ARReferenceImage.GetReferenceImagesInGroup( name: ARResourceImageGroup, bundle: NSBundle.MainBundle );

         if( trackedImages == null )
            throw new Exception( "iOS - Reference images could not be loaded from Assets.xcassets." );

         using( var configuration = new ARImageTrackingConfiguration { TrackingImages = trackedImages, MaximumNumberOfTrackedImages = 1 } )
         {
            Console.WriteLine( $"iOS - Reseting AR tracking with trackable images. Maximum Number Of Tracked Images: {configuration.MaximumNumberOfTrackedImages}" );

            sceneView.Session.Run( configuration, options: ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors );
         }
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
         NavigationController.PopViewController( animated: true );
      }

      private void HandleRestartButtonTouchUpInside( object sender, EventArgs e )
      {
         RestartARExperience( );
      }
      #endregion

      #region IARSCNViewDelegate
      [Export( "renderer:didAddNode:forAnchor:" )]
      public void DidAddNode( ISCNSceneRenderer renderer, SCNNode node, ARAnchor anchor )
      {
         Console.WriteLine( "iOS - Image found and tracked." );

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
         // Using Anchors to determine distance.
         using( var cameraFrame = sceneView.Session.CurrentFrame )
         {
            var imageAnchor = anchor as ARImageAnchor;
            var cameraAnchor = cameraFrame.Camera;

            var imageAnchorPosition = imageAnchor.Transform.Column3;
            var cameraAnchorPosition = cameraAnchor.Transform.Column3;

            var cameraToImageDistance = cameraAnchorPosition - imageAnchorPosition;

            var cameraToImageDistanceRounded = Math.Round( cameraToImageDistance.Length, 2 );

            // Allow to capture RSSI from BLE advertisements if distance is 0.5m. EPSILON is used to compare double values.
            canCaptureRSSI = Math.Abs( cameraToImageDistanceRounded - viewModel.RSSICapturableDistance ) < double.Epsilon;

            cameraCalibrationStatusController.UpdateDistanceMessage( $"{cameraToImageDistanceRounded}m" );
            cameraCalibrationStatusController.NotifyUiState( canCaptureRSSI );
         }
      }
      #endregion

      #region IARSessionDelegate
      [Export( "session:didUpdateAnchors:" )]
      public void DidUpdateAnchors( ARSession session, ARAnchor[ ] anchors )
      {
         var imageAnchor = anchors[ 0 ] as ARImageAnchor;

         if( !imageAnchor.IsTracked )
         {
            if( canCaptureRSSI )
            {
               canCaptureRSSI = false;
               cameraCalibrationStatusController.UpdateDistanceMessage( $"Find the Tracker" );
               cameraCalibrationStatusController.NotifyUiState( canCaptureRSSI );
            }
         }  
      }
      #endregion

      #region IARSessionObserver
      [Export( "session:cameraDidChangeTrackingState:" )]
      public void CameraDidChangeTrackingState( ARSession session, ARCamera camera )
      {
         cameraCalibrationStatusController.ShowTrackingQualityInfo( camera.TrackingState, camera.TrackingStateReason );
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
         var errorAlertController = UIAlertController.Create( "Opps, an error has occured!", message, UIAlertControllerStyle.Alert );
         errorAlertController.AddAction( UIAlertAction.Create( "Try Again", UIAlertActionStyle.Default, alertAction => {
            RestartARExperience( );
         } ) );

         PresentViewController( errorAlertController, animated: true, completionHandler: null );
      }

      void IBluetoothLEAdvertisement.ReceivedPKAdvertisement( Anchor anchor, int RSSI )
      {
         if( canCaptureRSSI )
         {
            viewModel.calibrateRSSI( RSSI );
         }
      }

      void ICameraCalibrationViewModel.UpdateCalibrationProgress( float percent )
      {
         cameraCalibrationStatusController.ProgressView.SetProgress( percent, animated: false );
      }

      void ICameraCalibrationViewModel.StopAdvertisingAndReset( )
      {
         canCaptureRSSI = false;
         IOSBluetoothLE.Instance.StopScanningForAdvertisements( );

         using( var configuration = new ARImageTrackingConfiguration( ) )
         {
            Console.WriteLine( "iOS - Reseting AR tracking with NO trackable images" );

            sceneView.Session.Run( configuration, options: ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors );
         }
      }

      void ICameraCalibrationViewModel.ShowCalibrationCompleted( string calibrationData )
      {
         cameraCalibrationStatusController.NotifyCalibrationCompleted( calibrationData );
      }

      void ICameraCalibrationViewModel.PresentLoading( )
      {
         PresentViewController( new SimpleLoadingController( ), animated: true, completionHandler: null );
      }

      void ICameraCalibrationViewModel.DismissLoading( )
      {
         InvokeOnMainThread( ( ) => {
            if( PresentedViewController is SimpleLoadingController loadingController )
               loadingController.DismissViewController( animated: true, completionHandler: null );
         } );
      }

      void ICameraCalibrationViewModel.NavigateToHome( )
      {
         InvokeOnMainThread( ( ) => {
            if( PresentingViewController is UINavigationController navigationController && !isRecalibrating )
            {
               navigationController.PushViewController( new HomeController( ), animated: false );
            }

            DismissViewController( animated: true, completionHandler: null );
         } );
      }
   }

   public class CameraCalibrationStatusController : UIViewController
   {
      private readonly CameraCalibrationViewModel viewModel;

      // UI Elements
      public UIProgressView ProgressView;

      private AnchoredConstraints topContainerViewAnchoredConstraints;
      private UILabel titleLabel;
      private UILabel subTitleLabel;
      private UILabel rssi_1_0_m_Label;
      private UIButton closeButton;
      private List<UIView> calibrationCompletedViews;

      private UIView frameView;
      private CAShapeLayer dashLineShapeLayer;
      private UILabel infoLabel;
      private UILabel distanceLabel;
      private UIActivityIndicatorView activityIndicatorView;
      private UIButton flashLightButton;

      public CameraCalibrationStatusController( CameraCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;

         calibrationCompletedViews = new List<UIView>( );
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupTopCalibrationView( );
         SetupCentreCalibrationView( );
         SetupFlashlight( );
      }

      public override void ViewDidAppear( bool animated )
      {
         base.ViewDidAppear( animated );

         // Testing purposes only.
         Task.Delay( 2000 ).ContinueWith( task => {
            NotifyCalibrationCompleted( "NaN" );
         } );
      }

      private void SetupTopCalibrationView( )
      {
         var containerView = new UIView {
            PreservesSuperviewLayoutMargins = true,
            BackgroundColor = Colors.White
         };

         View.AddSubview( containerView );

         topContainerViewAnchoredConstraints = containerView.FillSuperview( padding: new UIEdgeInsets( 0, 0, View.Frame.Height * 0.83f, 0 ) );

         var statusBarImageView = new UIImageView {
            Image = Images.BoschSuperGraphic,
            ContentMode = UIViewContentMode.ScaleAspectFill,
            ClipsToBounds = true
         };

         containerView.AddSubview( statusBarImageView );

         statusBarImageView.Anchor( leading: containerView.LeadingAnchor, top: containerView.TopAnchor, trailing: containerView.TrailingAnchor,
            size: new CGSize( 0, UIApplication.SharedApplication.StatusBarFrame.Height ) );

         titleLabel = Components.UILabel( "Calibration Progress", Colors.BoschBlue, Fonts.BoschLight.WithSize( 18 ) );
         subTitleLabel = Components.UILabel( "Track the image and hold at 1 metre.", Colors.BoschBlack, Fonts.BoschLight.WithSize( 15 ) );

         var messagesLabel = Components.UILabel( viewModel.Message, Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ) );
         messagesLabel.Hidden = true;

         var pkImageView = Components.UIImageView( Images.PK3, Colors.BoschBlue );
         pkImageView.Hidden = true;

         rssi_1_0_m_Label = Components.UILabel( "NaN", Colors.BoschBlack, Fonts.BoschLight.WithSize( 15 ), textAlignment: UITextAlignment.Right );

         var rssi_1_0_m_StackView = HStack(
            Components.UILabel( "1m:", Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ) ),
            new UIView( ),
            rssi_1_0_m_Label
         );
         rssi_1_0_m_StackView.Hidden = true;

         closeButton = Components.UIButton( Images.Close, Colors.BoschBlue );
         closeButton.TouchUpInside += ( sender, eventArgs ) => {
            DismissViewController( animated: true, completionHandler: null );
         };

         var finishedButton = Components.UIButton( "Finished", Colors.BoschBlue, Fonts.BoschLight.WithSize( 16 ), new UIEdgeInsets( 12, 0, 12, 0 ) );
         finishedButton.WithBorder( 1, Colors.BoschBlue );
         finishedButton.WithCornerRadius( Values.CornerRadius );
         finishedButton.Hidden = true;
         finishedButton.TouchUpInside += ( sender, eventArgs ) => {
            viewModel.ActionFinished( );
         };

         calibrationCompletedViews.Add( messagesLabel );
         calibrationCompletedViews.Add( pkImageView );
         calibrationCompletedViews.Add( rssi_1_0_m_StackView );
         calibrationCompletedViews.Add( finishedButton );

         var stackView = VStack(
            HStack(
               titleLabel,
               closeButton.WithSquareSize( 18 )
            ).With( alignment: UIStackViewAlignment.Center ),
            subTitleLabel,
            messagesLabel,
            pkImageView.WithHeight( 150 ),
            rssi_1_0_m_StackView,
            new UIView( ),
            finishedButton
         ).With( spacing: 12 ).WithPadding( new UIEdgeInsets( 12, 0, 12, 0 ) );

         containerView.AddSubview( stackView );

         stackView.Anchor( leading: containerView.LayoutMarginsGuide.LeadingAnchor, top: statusBarImageView.BottomAnchor,
            trailing: containerView.LayoutMarginsGuide.TrailingAnchor, bottom: containerView.BottomAnchor );

         ProgressView = new UIProgressView( UIProgressViewStyle.Bar ) {
            ProgressTintColor = Colors.BoschLightPurple,
            TrackTintColor = Colors.BoschGray.ColorWithAlpha( 0.5f ),
            ClipsToBounds = true,
         };

         containerView.AddSubview( ProgressView );

         ProgressView.Anchor( leading: containerView.LeadingAnchor, trailing: containerView.TrailingAnchor,
            bottom: containerView.BottomAnchor, size: new CGSize( 0, 4 ) );
      }

      private void SetupCentreCalibrationView( )
      {
         frameView = new UIView( );

         infoLabel = Components.UILabel( "Tracking Normal", Colors.White, Fonts.BoschBold.WithSize( 15 ) );
         infoLabel.Alpha = 0;
         distanceLabel = Components.UILabel( "Find the Tracker", Colors.White, Fonts.BoschBold.WithSize( 15 ) );
         activityIndicatorView = new UIActivityIndicatorView( UIActivityIndicatorViewStyle.White );
         activityIndicatorView.HidesWhenStopped = true;

         View.AddSubview( frameView );

         frameView.CenterInSuperView( size: new CGSize( View.Frame.Width - 40, 250 ) );

         frameView.AddSubviews( infoLabel, distanceLabel, activityIndicatorView );

         infoLabel.Anchor( leading: frameView.LeadingAnchor, bottom: frameView.BottomAnchor,
            trailing: frameView.TrailingAnchor, padding: new UIEdgeInsets( 0, 12, 8, 12 ) );

         distanceLabel.Anchor( centerX: frameView.CenterXAnchor, top: frameView.TopAnchor, padding: new UIEdgeInsets( 8, 0, 0, 0 ) );

         activityIndicatorView.Anchor( bottom: frameView.BottomAnchor, trailing: frameView.TrailingAnchor,padding: new UIEdgeInsets( 0, 0, 12, 12 ) );

         // Add dashed line shape layer to frameView

         View.LayoutIfNeeded( );

         dashLineShapeLayer = new CAShapeLayer {
            StrokeColor = Colors.White.CGColor,
            LineDashPattern = new NSNumber[ ] { 6, 3 },
            LineWidth = 2,
            FillColor = Colors.Clear.CGColor,
            Path = UIBezierPath.FromRoundedRect( frameView.Bounds, cornerRadius: 20 ).CGPath,
         };

         frameView.Layer.AddSublayer( dashLineShapeLayer );
      }

      private void SetupFlashlight( )
      {
         flashLightButton = new UIButton( UIButtonType.System );
         flashLightButton.SetImage( Images.FlashlightOff, UIControlState.Normal );
         flashLightButton.SetImage( Images.Flashlight, UIControlState.Selected );
         flashLightButton.TintColor = Colors.White;
         flashLightButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
         flashLightButton.ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 );
         flashLightButton.TouchUpInside += ( sender, e ) => {
            try
            {
               if( flashLightButton.Selected )
               {
                  Flashlight.TurnOffAsync( );
                  flashLightButton.Selected = false;
                  flashLightButton.ImageView.TintColor = Colors.White;
               }
               else
               {
                  Flashlight.TurnOnAsync( );
                  flashLightButton.Selected = true;
                  flashLightButton.ImageView.TintColor = Colors.BoschBlue;
               }
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
         };

         View.AddSubview( flashLightButton );

         flashLightButton.Anchor( trailing: View.LayoutMarginsGuide.TrailingAnchor, bottom: View.LayoutMarginsGuide.BottomAnchor,
            padding: new UIEdgeInsets( 0, 0, 8, 8 ), size: new CGSize( 30, 30 ) );
      }

      public void ShowTrackingQualityInfo( ARTrackingState trackingState, ARTrackingStateReason trackingStateReason )
      {
         ShowInfoMessage( ARCameraHelper.PresentationStringForState( trackingState, trackingStateReason ) );
      }

      public void ShowInfoMessage( string message )
      {
         InvokeOnMainThread( ( ) => {
            infoLabel.Text = message;
            infoLabel.Alpha = 1;
            DelayAnimate( ( ) => infoLabel.Alpha = 0, null );
         } );
      }

      public void NotifyUiState( bool isCapturing )
      {
         InvokeOnMainThread( ( ) => {
            if( isCapturing )
            {
               distanceLabel.TextColor = Colors.BoschLightGreen;
               dashLineShapeLayer.StrokeColor = Colors.BoschLightGreen.CGColor;
               if( !activityIndicatorView.IsAnimating )
                  activityIndicatorView.StartAnimating( );
            }
            else
            {
               distanceLabel.TextColor = Colors.White;
               dashLineShapeLayer.StrokeColor = Colors.White.CGColor;
               if( activityIndicatorView.IsAnimating )
                  activityIndicatorView.StopAnimating( );
            }
         } );  
      }

      public void UpdateDistanceMessage( string message )
      {
         InvokeOnMainThread( ( ) => {
            distanceLabel.Text = message;
         } );
      }

      public void NotifyCalibrationCompleted( string calibrationData )
      {
         InvokeOnMainThread( ( ) => {
            titleLabel.Text = "Calibration Completed";
            subTitleLabel.Text = "The calibration has been successful.";
            rssi_1_0_m_Label.Text = calibrationData;
            closeButton.Hidden = true;
            ProgressView.Hidden = true;
            frameView.Hidden = true;
            flashLightButton.Hidden = true;

            UIView.AnimateNotify( duration: 0.5, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {
               topContainerViewAnchoredConstraints.Bottom.Constant = -View.Frame.Height * 0.25f;
               View.LayoutIfNeeded( );
            }, completion: finished => {
               foreach( var view in calibrationCompletedViews )
                  view.Hidden = false;
            } );
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
}
