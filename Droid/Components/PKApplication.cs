using System;
using Android;
using Android.App;

namespace PK.Droid.Components
{
   public class PKApplication
   {
      public const int REQUESTCODE_LOCATION_ID = 0;
      public const int REQUESTCODE_CAMERA_ID = 1;

      public static void RequestLocationPermission( Activity activity ) => activity.RequestPermissions( permissions: new string[ ] { Manifest.Permission.AccessFineLocation }, requestCode: REQUESTCODE_LOCATION_ID );

      public static void RequestCameraPermission( Activity activity ) => activity.RequestPermissions( permissions: new string[ ] { Manifest.Permission.Camera }, requestCode: REQUESTCODE_CAMERA_ID );
   }
}
