using System;
using PK.Common;
using PK.Interfaces;
using PK.iOS.Bluetooth;
using PK.iOS.Helpers;
using UIKit;

namespace PK.iOS.Controllers
{
   public class LightNavigationController : UINavigationController, IBluetoothLEState
   {
      private SnackBarController bluetoothSnackBar;

      public LightNavigationController( UIViewController rootViewController ) : base( rootViewController )
      {
         // IOSBluetoothLE.Instance.StateDelegate = this;
      }

      public override UIStatusBarStyle PreferredStatusBarStyle( ) => TopViewController?.PreferredStatusBarStyle( ) ?? UIStatusBarStyle.LightContent;

      void IBluetoothLEState.NotifyBluetoothNotSupported( )
      {
         var alertController = UIAlertController.Create( Strings.UhOh, Strings.NoBluetoothSuportText, UIAlertControllerStyle.Alert );

         TopViewController.PresentViewController( alertController, animated: true, completionHandler: null );
      }

      void IBluetoothLEState.NotifyBluetoothIsOff( )
      {
         bluetoothSnackBar = new SnackBarController {
            IconImage = Images.AlertCircleOutline,
            MessageText = Strings.BluetoothTurnOnText,
            ActionText = string.Empty,
         };

         TopViewController.PresentViewController( viewControllerToPresent: bluetoothSnackBar, animated: true, completionHandler: null );
      }

      void IBluetoothLEState.NotifyBluetoothIsOn( )
      {
         bluetoothSnackBar?.DismissViewController( animated: true, completionHandler: null );
      }

      bool IBluetoothLEState.VerifyLocationPermission( ) => throw new NotImplementedException( );
   }
}
