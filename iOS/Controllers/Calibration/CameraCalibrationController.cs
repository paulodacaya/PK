using System;
using System.Text;
using System.Threading.Tasks;
using ARKit;
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
      private bool captureRSSI;

      private readonly CameraCalibrationViewModel viewModel;

      private ARSCNView sceneView;
      private CameraCalibrationStatusController cameraCalibrationStatusController;

      public CameraCalibrationController( )
      {
         viewModel = new CameraCalibrationViewModel( this );

         IOSBluetoothLE.Instance.AdvertisementDelegate = this;
         IOSBluetoothLE.Instance.ScanForAdvertisements( );
      }

      public override bool PrefersStatusBarHidden( )
      {
         return true;
      }

      public override void ViewWillTransitionToSize( CGSize toSize, IUIViewControllerTransitionCoordinator coordinator )
      {
         base.ViewWillTransitionToSize( toSize, coordinator );

         coordinator.AnimateAlongsideTransition( ( IUIViewControllerTransitionCoordinatorContext context ) => {

            var orientation = UIApplication.SharedApplication.StatusBarOrientation;

            NavigationController.SetNavigationBarHidden( hidden: orientation == UIInterfaceOrientation.LandscapeLeft
               || orientation == UIInterfaceOrientation.LandscapeRight, animated: true );

         }, completion: null );
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNavigation( );
         SetupViews( );
      }

      public override void ViewWillAppear( bool animated )
      {
         base.ViewWillAppear( animated );
         NavigationController.SetNavigationBarHidden( false, animated: true );
      }

      public override void ViewDidAppear( bool animated )
      {
         base.ViewDidAppear( animated );
         // Prevent the screen from dimming to avoid interuppting the AR experience.
         UIApplication.SharedApplication.IdleTimerDisabled = true;

         // Start AR caibration experience.
         ResetTracking( );

         // Start guidance animation prompt.
         cameraCalibrationStatusController.StartGuidancePrompt( );
      }

      public override void ViewWillDisappear( bool animated )
      {
         base.ViewWillDisappear( animated );
         NavigationController.SetNavigationBarHidden( true, animated: true );
      }

      public override void ViewDidDisappear( bool animated )
      {
         base.ViewDidDisappear( animated );
         sceneView.Session.Pause( );
      }

      private void SetupNavigation( )
      {
         var backButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
            TintColor = Colors.White,
         };
         backButton.SetImage( Images.ChevronLeft, UIControlState.Normal );
         backButton.WithSquareSize( 22 );
         backButton.TouchUpInside += HandleBackButtonTouchUpInside;

         var restartButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
            TintColor = Colors.White,
         };
         restartButton.SetImage( Images.Restart, UIControlState.Normal );
         restartButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
         restartButton.WithSquareSize( 22 );
         restartButton.TouchUpInside += HandleRestartButtonTouchUpInside;

         NavigationItem.LeftBarButtonItem = new UIBarButtonItem( customView: backButton );
         NavigationItem.RightBarButtonItem = new UIBarButtonItem( customView: restartButton );
      }

      private void SetupViews( )
      {
         sceneView = new ARSCNView( );

         sceneView.Delegate = this;
         sceneView.Session.Delegate = this;

         cameraCalibrationStatusController = new CameraCalibrationStatusController( viewModel );

         AddChildViewController( cameraCalibrationStatusController );
         View.AddSubviews( sceneView, cameraCalibrationStatusController.View );
         cameraCalibrationStatusController.DidMoveToParentViewController( this );

         sceneView.FillSuperview( );

         cameraCalibrationStatusController.View.Anchor( leading: View.LeadingAnchor, top: View.LayoutMarginsGuide.TopAnchor,
            trailing: View.TrailingAnchor, bottom: View.LayoutMarginsGuide.BottomAnchor );
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

         cameraCalibrationStatusController.ShowGuidancePrompt( );
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
         // TODO Handle orientation lock
         // Lock orientation back to default (portrait) and force rotate it.
         // OrienationHelper.LockOrientation( UIInterfaceOrientationMask.Portrait, rotateToOrientation: UIInterfaceOrientationMask.Portrait );

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
         // Using Anchors to determine distance
         using( var cameraFrame = sceneView.Session.CurrentFrame )
         {
            var imageAnchor = anchor as ARImageAnchor;
            var cameraAnchor = cameraFrame.Camera;

            var imageAnchorPosition = imageAnchor.Transform.Column3;
            var cameraAnchorPosition = cameraAnchor.Transform.Column3;

            var cameraToImageDistance = cameraAnchorPosition - imageAnchorPosition;

            var cameraToImageDistance2d = Math.Round( cameraToImageDistance.Length, 2 );

            // Allow to capture RSSI from BLE advertisements if distance is 0.5m
            // EPSILON is used to compare double values
            captureRSSI = Math.Abs( cameraToImageDistance2d - viewModel.RSSICapturableDistance ) < double.Epsilon;

            cameraCalibrationStatusController.ShowDistanceMessage( cameraToImageDistance2d.ToString( ) );
            cameraCalibrationStatusController.IsCapturingRSSI = captureRSSI;
         }
      }
      #endregion

      #region IARSessionDelegate
      [Export( "session:didUpdateAnchors:" )]
      public void DidUpdateAnchors( ARSession session, ARAnchor[ ] anchors )
      {
         var imageAnchor = anchors[ 0 ] as ARImageAnchor;

         if( imageAnchor.IsTracked )
            cameraCalibrationStatusController.HideGuidancePrompt( );
         else
         {
            captureRSSI = false;
            cameraCalibrationStatusController.IsCapturingRSSI = false;
            cameraCalibrationStatusController.ShowGuidancePrompt( );
            cameraCalibrationStatusController.HideDistanceMessage( );
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

      void IBluetoothLEAdvertisement.ReceivedPKAdvertisement( Anchor anchor, int RSSI )
      {
         if( captureRSSI )
            viewModel.calibrateRSSI( RSSI );
      }

      void ICameraCalibrationViewModel.UpdateCalibrationProgress( float percent )
      {
         cameraCalibrationStatusController.ProgressView.SetProgress( percent, animated: false );
      }

      void ICameraCalibrationViewModel.StopAdvertisingAndReset( )
      {
         captureRSSI = false;
         IOSBluetoothLE.Instance.StopScanningForAdvertisements( );

         using( var configuration = new ARImageTrackingConfiguration( ) )
         {
            Console.WriteLine( "iOS - Reseting AR tracking with NO trackable images" );

            sceneView.Session.Run( configuration, options: ARSessionRunOptions.ResetTracking | ARSessionRunOptions.RemoveExistingAnchors );
         }
      }

      void ICameraCalibrationViewModel.ShowCalibrationCompleted( )
      {
         UIView.AnimateNotify( duration: 0.5, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {

            cameraCalibrationStatusController.View.Alpha = 0;

         }, completion: finished => {

            NavigationController.SetNavigationBarHidden( true, animated: true );

            var cameraCalibrationPopUpController = new CameraCalibrationPopUpController( viewModel );
            PresentViewController( cameraCalibrationPopUpController, animated: false, completionHandler: null );

         } );
      }

      void ICameraCalibrationViewModel.NavigateToHome( )
      {
         var homeController = new HomeController( );
         NavigationController.PushViewController( homeController, animated: true );
      }

      void ICameraCalibrationViewModel.NavigateToConfigureZones( )
      {
      }
   }

   public class CameraCalibrationStatusController : UIViewController
   {
      public bool IsCapturingRSSI
      {
         get => calibrationDistanceContainerView.BackgroundColor == Colors.BoschDarkGreen;
         set
         {
            InvokeOnMainThread( ( ) => {
               calibrationDistanceContainerView.BackgroundColor = value ? Colors.BoschDarkGreen : Colors.White;
            } );
         }
      }

      public UIProgressView ProgressView;

      private UILabel infoLabel;
      private UIView calibrationDistanceInfoLabel;
      private UIView calibrationDistanceContainerView;
      private UILabel calibrationDistanceDistanceLabel;
      private UIStackView guidanceStackView;
      private UILabel guidanceMessageLabel;

      private UIImageView flashlightImageView;
      private bool IsFlashlightOn;

      private readonly CameraCalibrationViewModel viewModel;

      public CameraCalibrationStatusController( CameraCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupButtomToolBar( );
         SetupCalibrationView( );
         SetupGuidancePrompt( );
      }

      private void SetupButtomToolBar( )
      {
         infoLabel = new UILabel {
            Font = Fonts.Medium.WithSize( 17 ),
            TextColor = Colors.White,
         };
         infoLabel.WithWidth( 300 );

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
               new UIBarButtonItem( customView: infoLabel ),
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

      private void SetupCalibrationView( )
      {
         var progressContainerView = new UIView {
            BackgroundColor = Colors.White
         };
         progressContainerView.WithCornerRadius( Values.CornerRadius );
         progressContainerView.WithShadow( opacity: 0.4f, radius: Values.CornerRadius, offset: new CGSize( 0, 2 ), color: Colors.White );

         var progressLabel = new UILabel {
            Text = "Calibration progress:",
            TextColor = Colors.OuterSpace,
            Font = Fonts.Medium.WithSize( 17 )
         };

         ProgressView = new UIProgressView( style: UIProgressViewStyle.Bar ) {
            ProgressTintColor = Colors.BoschDarkGreen,
            TrackTintColor = Colors.LightWhite,
            ClipsToBounds = true,
         };
         ProgressView.WithHeight( 4 );
         ProgressView.WithCornerRadius( Values.CornerRadius );

         progressContainerView.AddSubviews( ProgressView, progressLabel );

         ProgressView.Anchor( leading: progressContainerView.LeadingAnchor, bottom: progressContainerView.BottomAnchor,
            trailing: progressContainerView.TrailingAnchor );

         progressLabel.Anchor( leading: progressContainerView.LeadingAnchor, top: progressContainerView.TopAnchor,
            trailing: progressContainerView.TrailingAnchor, bottom: ProgressView.TopAnchor, padding: new UIEdgeInsets( 8, 8, 8, 8 ) );

         calibrationDistanceInfoLabel = new UILabel {
            Font = Fonts.Medium.WithSize( 17 ),
            Text = "Hold at 0.5 metres.",
            TextColor = Colors.White,
            Alpha = 0
         };

         calibrationDistanceContainerView = new UIView {
            BackgroundColor = Colors.White,
            Alpha = 0
         };
         calibrationDistanceContainerView.WithCornerRadius( Values.CornerRadius );
         calibrationDistanceContainerView.WithShadow( opacity: 0.4f, radius: Values.CornerRadius, offset: new CGSize( 0, 2 ), color: Colors.White );

         calibrationDistanceDistanceLabel = new UILabel {
            Font = Fonts.Medium.WithSize( 17 ),
            TextColor = Colors.OuterSpace,
            TextAlignment = UITextAlignment.Center
         };

         calibrationDistanceContainerView.AddSubviews( calibrationDistanceDistanceLabel );

         calibrationDistanceDistanceLabel.FillSuperview( padding: new UIEdgeInsets( 8, 12, 8, 12 ) );

         var stackView = VStack(
            progressContainerView,
            HStack(
               calibrationDistanceInfoLabel,
               new UIView( ),
               calibrationDistanceContainerView
            ).With( alignment: UIStackViewAlignment.Center )
         ).With( spacing: 8 );

         View.AddSubview( stackView );

         stackView.Anchor( leading: View.LayoutMarginsGuide.LeadingAnchor, top: View.LayoutMarginsGuide.TopAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor, padding: new UIEdgeInsets( 8, 0, 0, 0 ) );
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
         UIView.AnimateNotify( duration: 1, delay: 0, options: UIViewAnimationOptions.Autoreverse |
            UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveEaseInOut, animation: ( ) => {
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
         infoLabel.Text = message;
         infoLabel.Alpha = 1;

         // Hide Info Message message after delay
         DelayAnimate( animation: ( ) => {
            infoLabel.Alpha = 0;
         }, completion: null );
      }

      public void ShowDistanceMessage( string distance )
      {
         InvokeOnMainThread( ( ) => {
            calibrationDistanceInfoLabel.Alpha = 1;
            calibrationDistanceContainerView.Alpha = 1;
            calibrationDistanceDistanceLabel.Text = $"{distance}m";
         } );
      }

      public void HideDistanceMessage( )
      {
         DelayAnimate( animation: ( ) => {
            calibrationDistanceInfoLabel.Alpha = 0;
            calibrationDistanceContainerView.Alpha = 0;
         }, completion: null );
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

   public class CameraCalibrationPopUpController : UIViewController
   {
      private readonly CameraCalibrationViewModel viewModel;

      private UIView containerView;
      private nfloat containerViewHeight;
      private AnchoredConstraints containerAnchoredConstraints;
      private CircularCheckBox statusCheckBox;
      private UIButton actionButton;

      public CameraCalibrationPopUpController( CameraCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupContent( );
         SetupActionButton( );
      }

      public override void ViewDidAppear( bool animated )
      {
         base.ViewDidAppear( animated );

         PresentPopUp( );
      }

      private void SetupContent( )
      {
         containerViewHeight = View.Frame.Height * 0.75f;

         containerView = new UIView {
            BackgroundColor = Colors.OuterSpace,
         };

         var titleLabel = new PKLabel( "Calibration Information", Colors.White, Fonts.Bold.WithSize( 24 ) );
         statusCheckBox = new CircularCheckBox( );
         var statusLabel = new PKLabel( "Status: COMPLETED", Colors.Gray, Fonts.Medium.WithSize( 18 ) );
         var nodeImageView = new PKImageView( Images.CircleSlice, Colors.White );
         var nodeStatusLabel = new PKLabel( "Central Node: -19.6", Colors.Gray, Fonts.Medium.WithSize( 18 ) );
         var calibrationMessage = new PKLabel( "To re-calibrate your device, find the option in settings. You will perform the same operation.", Colors.White, Fonts.Regular.WithSize( 16 ) );
         var divider = new UIView { BackgroundColor = Colors.InnerSpace };
         var customiseZonesLabel = new PKLabel( "Customise Vehicle Zones", Colors.White, Fonts.Bold.WithSize( 20 ) );
         var customiseZonesMessageLabel = new PKLabel( "You can now customise your vehicle zones. This will determine the range in which your mobile is considered within the vehicle's zone. Using default zones will skip to your home screen.", Colors.White, Fonts.Regular.WithSize( 16 ) );

         var defaultZonesButton = new UIButton( UIButtonType.System );
         defaultZonesButton.SetAttributedTitle( new NSAttributedString(
            str: "Use Default Zones",
            font: Fonts.Regular.WithSize( 17 ),
            foregroundColor: Colors.White,
            underlineStyle: NSUnderlineStyle.Single
         ), UIControlState.Normal );
         defaultZonesButton.TouchUpInside += HandleDefaultZonesButtonTouchUpInside;

         var stackView = VStack(
            titleLabel,
            HStack( statusCheckBox.WithSquareSize( 16 ), statusLabel ).With( spacing: 10 ),
            HStack( nodeImageView.WithSquareSize( 16 ), nodeStatusLabel ).With( spacing: 10 ),
            calibrationMessage,
            divider.WithHeight( 0.5f ),
            customiseZonesLabel,
            customiseZonesMessageLabel,
            defaultZonesButton,
            new UIView( )
         ).With( spacing: 20 ).WithPadding( new UIEdgeInsets( 20, 0, 20, 0 ) );

         View.AddSubview( containerView );

         containerAnchoredConstraints = containerView.Anchor( leading: View.LeadingAnchor, bottom: View.BottomAnchor,
            trailing: View.TrailingAnchor, size: new CGSize( 0, containerViewHeight ),
            padding: new UIEdgeInsets( 0, 0, -containerViewHeight, 0 ) );

         View.LayoutIfNeeded( );

         containerView.CornerRadius( UIRectCorner.TopLeft | UIRectCorner.TopRight, Values.DialogCornerRadius );

         containerView.AddSubview( stackView );

         stackView.Anchor( leading: View.LayoutMarginsGuide.LeadingAnchor, trailing: View.LayoutMarginsGuide.TrailingAnchor,
            top: containerView.TopAnchor, bottom: containerView.BottomAnchor );
      }

      private void SetupActionButton( )
      {
         actionButton = new UIButton( UIButtonType.System );
         actionButton.SetImage( Images.ChevronRight, UIControlState.Normal );
         actionButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
         actionButton.ImageView.WithSquareSize( 20 );
         actionButton.TintColor = Colors.OuterSpace;
         actionButton.BackgroundColor = Colors.White;
         actionButton.WithSquareSize( 50 );
         actionButton.WithCornerRadius( 25 );
         actionButton.WithShadow( opacity: 0.5f, radius: 2, color: Colors.White );
         actionButton.Alpha = 0;
         actionButton.TouchUpInside += HandleActionButtonTouchUpInside;

         View.AddSubview( actionButton );

         actionButton.Anchor( bottom: View.LayoutMarginsGuide.BottomAnchor, trailing: View.LayoutMarginsGuide.TrailingAnchor,
            padding: new UIEdgeInsets( 0, 0, 14, 0 ) );
      }

      private void HandleDefaultZonesButtonTouchUpInside( object sender, EventArgs e )
      {
         DismissPopUp( completionHandler: ( ) => {
            viewModel.UseDefaultZonesSelected( );
         } );
      }

      private void HandleActionButtonTouchUpInside( object sender, EventArgs e )
      {
         DismissPopUp( completionHandler: ( ) => {
            viewModel.ActionSelected( );
         } );
      } 

      private void PresentPopUp( )
      {
         UIView.AnimateNotify( duration: 0.4, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {

            View.BackgroundColor = Colors.Black.ColorWithAlpha( 0.2f );
            containerAnchoredConstraints.Bottom.Constant = 0;
            actionButton.Alpha = 1;

            View.LayoutIfNeeded( );

         }, completion: finished => {

            statusCheckBox.SetOn( true, animated: true );

         } );
      }

      private void DismissPopUp( Action completionHandler )
      {
         UIView.AnimateNotify( duration: 0.4, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {

            View.BackgroundColor = Colors.Clear;
            containerAnchoredConstraints.Bottom.Constant = containerViewHeight;
            actionButton.Alpha = 0 ;

            View.LayoutIfNeeded( );

         }, completion: ( bool finished ) => {

            DismissViewController( animated: false, completionHandler: completionHandler );

         } );
      }
   }
}
