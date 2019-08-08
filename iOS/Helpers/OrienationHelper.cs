using Foundation;
using UIKit;

namespace PK.iOS.Helpers
{
   public static class OrienationHelper
   {
      public static void LockOrientation( UIInterfaceOrientationMask orientation )
      {
         var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
         appDelegate.OrientationLock = orientation;
      }

      public static void LockOrientation( UIInterfaceOrientationMask orientation, UIInterfaceOrientationMask rotateToOrientation )
      {
         LockOrientation( orientation );

         UIDevice.CurrentDevice.SetValueForKey( new NSNumber( ( int )rotateToOrientation ), new NSString( "orientation" ) );

         UIViewController.AttemptRotationToDeviceOrientation( );
      }
   }
}
