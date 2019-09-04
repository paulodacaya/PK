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
      private UIImageView vehicleImageView;
      private RadialGradientLayer radialGradientLayer;

      public RootNavigationController( UIViewController rootViewController ) : base( rootViewController )
      {
         //IOSBluetoothLE.Instance.StateDelegate = this;

         // TODO I think scanning should immediatly occur if AppDelegate device is calibrated.
         // IOSBluetoothLE.Instance.ScanForAdvertisements( );
      }

      public override UIStatusBarStyle PreferredStatusBarStyle( ) => UIStatusBarStyle.LightContent;

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupVehicleImage( );
         SetupGradientLayer( );
      }

      private void SetupVehicleImage( )
      {
         vehicleContainerView = new UIView( );

         vehicleImageView = new UIImageView {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            Image = Images.Mazda3AngleView,
         };

         View.InsertSubview( vehicleContainerView, 0 );

         vehicleContainerView.Anchor( leading: View.LeadingAnchor, top: View.LayoutMarginsGuide.TopAnchor,
            trailing: View.TrailingAnchor, size: new CGSize( 0, UIScreen.MainScreen.Bounds.Height * 0.425 ),
            padding: new UIEdgeInsets( NavigationBar.Frame.Size.Height, 0, 0, 0 ) );

         vehicleContainerView.AddSubview( vehicleImageView );

         vehicleImageView.Anchor( centerX: vehicleContainerView.CenterXAnchor, centerY: vehicleContainerView.CenterYAnchor );
            
         // Allow views to layout to resolve frame values
         View.LayoutIfNeeded( );

         vehicleImageView.ConstraintWidthEqualTo( vehicleContainerView.Frame.Width * 0.85f );
         vehicleImageView.ConstraintHeightEqualTo( vehicleContainerView.Frame.Height * 0.55f );
      }

      public void SetupGradientLayer( )
      {
         var center = vehicleContainerView.Center;
         center.Y -= 20;

         var colors = new CGColor[ ] { Colors.OuterSpaceRadial.CGColor, Colors.OuterSpace.CGColor };

         radialGradientLayer = new RadialGradientLayer( center, colors ) {
            Frame = View.Bounds,
         };

         View.Layer.InsertSublayer( radialGradientLayer, 0 );
      }

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

      public void OverrideTransitionDelegate( ) => Delegate = this;
   }
}
