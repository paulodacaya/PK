using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using PK.Common;
using PK.Interfaces;
using PK.iOS.Bluetooth;
using PK.iOS.Helpers;
using UIKit;

namespace PK.iOS.Controllers
{
   public class RootNavigationController : UINavigationController, IBluetoothLEState, IUINavigationControllerDelegate
   {
      public bool ReverseTransitioning { get; set; }

      public UIViewController TopMostController
      {
         get
         {
            var topController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            while( topController.PresentedViewController != null )
            {
               topController = topController.PresentedViewController;
            }

            return topController;
         }
      }

      // UIElements
      private SnackBarController bluetoothSnackBar;
      private UIView vehicleContainerView;
      private RadialGradientLayer radialGradientLayer;
      private EllipseView ellipseView;
      private UIImageView vehicleImageView;
      private UIImageView personImageView;
      private AnchoredConstraints personImageViewAnchoredConstraints;
      private AnchoredConstraints ellipseViewAnchoredConstraints;

      public RootNavigationController( ) : base( rootViewController: new HomeController( ) )
      {
         Delegate = this;

         IOSBluetoothLE.Instance.StateDelegate = this;
      }

      public override UIStatusBarStyle PreferredStatusBarStyle( )=> UIStatusBarStyle.LightContent;

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNotificationObserver( );
         SetupViews( );
         SetupGradientLayer( );
      }

      private void SetupNotificationObserver( )
      {
         UIApplication.Notifications.ObserveWillEnterForeground( HandleAppWillEnterForeground );
         UIApplication.Notifications.ObserveDidEnterBackground( HandleAppDidEnterBackground );
      }

      private void SetupViews( )
      {
         View.BackgroundColor = Colors.OuterSpace;

         vehicleContainerView = new UIView( );

         ellipseView = new EllipseView {
            Alpha = 0,
         };

         vehicleImageView = new UIImageView {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            Image = Images.Mazda3AngleView,
         };

         personImageView = new UIImageView {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            Image = Images.IsometricMaleBackwards,
            Alpha = 0,
         };

         View.InsertSubview( vehicleContainerView, 0 );

         vehicleContainerView.Anchor( leading: View.LeadingAnchor, top: View.LayoutMarginsGuide.TopAnchor,
            trailing: View.TrailingAnchor, size: new CGSize( 0, UIScreen.MainScreen.Bounds.Height * 0.425 ),
            padding: new UIEdgeInsets( NavigationBar.Frame.Size.Height, 0, 0, 0 ) );

         vehicleContainerView.AddSubviews( ellipseView, vehicleImageView, personImageView );

         vehicleImageView.Anchor( centerX: vehicleContainerView.CenterXAnchor, centerY: vehicleContainerView.CenterYAnchor );

         ellipseViewAnchoredConstraints = ellipseView.Anchor( leading: vehicleImageView.LeadingAnchor, trailing: vehicleImageView.TrailingAnchor, 
            top: vehicleImageView.TopAnchor, bottom: vehicleImageView.BottomAnchor, padding: new UIEdgeInsets( 0, 0, 8, 0 ) );

         personImageViewAnchoredConstraints = personImageView.Anchor( bottom: vehicleContainerView.BottomAnchor, 
            trailing: vehicleContainerView.TrailingAnchor, padding: new UIEdgeInsets( 0, 0, 0, View.Frame.Width / 2 ) );
            
         // Allow views to layout to resolve frame values
         View.LayoutIfNeeded( );

         personImageView.ConstraintHeightEqualTo( vehicleContainerView.Frame.Height * 0.45f );
         personImageViewAnchoredConstraints.Bottom.Constant = -vehicleContainerView.Frame.Height * 0.3f;
         personImageViewAnchoredConstraints.Trailing.Constant = -vehicleContainerView.Frame.Width * 0.11f;

         vehicleImageView.ConstraintWidthEqualTo( vehicleContainerView.Frame.Width * 0.85f );
         vehicleImageView.ConstraintHeightEqualTo( vehicleContainerView.Frame.Height * 0.55f );

         ellipseViewAnchoredConstraints.Top.Constant = vehicleContainerView.Frame.Height * 0.2f;
      }

      public void SetupGradientLayer( )
      {
         var center = vehicleContainerView.Center;
         center.Y -= 20;

         var colors = new CGColor[ ] { Colors.OuterSpaceLight.CGColor, Colors.OuterSpace.CGColor };

         radialGradientLayer = new RadialGradientLayer( center, colors ) {
            Frame = View.Bounds,
         };

         View.Layer.InsertSublayer( radialGradientLayer, 0 );
      }

      #region IUINavigationControllerDelegate
      [Export( "navigationController:animationControllerForOperation:fromViewController:toViewController:" )]
      public IUIViewControllerAnimatedTransitioning GetAnimationControllerForOperation( UINavigationController navigationController, UINavigationControllerOperation operation, UIViewController fromViewController, UIViewController toViewController )
      {
         switch( operation )
         {
            case UINavigationControllerOperation.Push:
               return new CustomAnimatedTransitioning( isPresenting: true, ReverseTransitioning );

            case UINavigationControllerOperation.Pop:
               return new CustomAnimatedTransitioning( isPresenting: false, ReverseTransitioning );

            default:
               return null;
         }
      }
      #endregion

      void IBluetoothLEState.NotifyBluetoothNotSupported( )
      {
         var dialog = new DialogController {
            TitleText = Strings.UhOh,
            MessageText = Strings.NoBluetoothSuportText,
            NegativeText = string.Empty,
            PositiveText = Strings.Understood,
         };

         TopMostController.PresentViewController( viewControllerToPresent: dialog, animated: true, completionHandler: null );
      }

      void IBluetoothLEState.NotifyBluetoothIsOff( )
      {
         bluetoothSnackBar = new SnackBarController {
            IconImage = Images.AlertCircleOutline,
            MessageText = Strings.BluetoothTurnOnText,
            ActionText = string.Empty,
         };

         TopMostController.PresentViewController( viewControllerToPresent: bluetoothSnackBar, animated: true, completionHandler: null );
      }

      void IBluetoothLEState.NotifyBluetoothIsOn( )
      {
         bluetoothSnackBar?.DismissViewController( animated: true, completionHandler: null );
      }

      bool IBluetoothLEState.VerifyLocationPermission( ) => throw new NotImplementedException( );

      #region Event Handlers
      private void HandleAppWillEnterForeground( object sender, NSNotificationEventArgs e ) => ellipseView.BeginPulsating( );

      private void HandleAppDidEnterBackground( object sender, NSNotificationEventArgs e ) => ellipseView.EndPulsating( );
      #endregion

      public void EnterCalibrateAnimation( )
      {
         ellipseView.BeginPulsating( );

         UIView.AnimateNotify( duration: 0.5, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {

            var transform = CGAffineTransform.MakeTranslation( tx: 0, ty: -vehicleContainerView.Frame.Height * 0.05f );
            transform.Scale( sx: 0.85f, sy: 0.85f, MatrixOrder.Append );

            vehicleImageView.Transform = transform;
            vehicleImageView.Alpha = 1;

            ellipseView.Transform = transform;
            ellipseView.Alpha = 1;

            personImageView.Alpha = 1;

         }, completion: null );
      }

      public void ExitCalibrateAnimation( )
      {
         ellipseView.EndPulsating( );

         UIView.AnimateNotify( duration: 0.5, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {
         
               vehicleImageView.Transform = CGAffineTransform.MakeIdentity( );
               ellipseView.Transform = CGAffineTransform.MakeIdentity( );
               personImageView.Alpha = 0;
               ellipseView.Alpha = 0;

            }, completion: null );
      }

      public void HideVehicle( )
      {
         UIView.AnimateNotify( duration: 0.5, animation: ( ) => {

            radialGradientLayer.Hidden = true;
            vehicleImageView.Alpha = 0;

         }, completion: null );
      }

      internal void ShowVehicle( )
      {
         UIView.AnimateNotify( duration: 0.5, animation: ( ) => {

            radialGradientLayer.Hidden = false;
            vehicleImageView.Alpha = 1;

         }, completion: null );
      }
   }
}
