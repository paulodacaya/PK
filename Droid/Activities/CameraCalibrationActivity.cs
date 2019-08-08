using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Google.AR.Core;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Google.AR.Core.Exceptions;
using Android.Hardware.Camera2;

namespace PK.Droid.Activities
{
   [Activity( Label = "CameraCalibrationActivity" )]
   public class CameraCalibrationActivity : AppCompatActivity
   {
      private bool UserRequestedARCoreInstall = true;
      private Session sharedSession;
      private SharedCamera sharedCamera;
      private string cameraID;
      private CameraDeviceStateCallback cameraDeviceStateCallback;
      private Handler backgroundHandler;

      public CameraCalibrationActivity( )
      {
         cameraDeviceStateCallback = new CameraDeviceStateCallback( );
         backgroundHandler = new Handler( );
      }

      protected override void OnCreate( Bundle savedInstanceState )
      {
         base.OnCreate( savedInstanceState );
         SetContentView( Resource.Layout.Activity_Camera_Calibration );
         MaybeEnableAR( );
      }

      protected override void OnResume( )
      {
         base.OnResume( );
         InitialiseSession( );
      }

      private void MaybeEnableAR( )
      {
         // Check ARCore availability
         var availability = ArCoreApk.Instance.CheckAvailability( this );
         if( availability.IsTransient )
         {
            // This state is temporary and the application should check again soon.
            var handler = new Handler( );
            handler.PostDelayed( MaybeEnableAR, 200 );
         }

         if( availability.IsSupported )
         {
            Console.WriteLine( "Android - ARCore (Augmeneted Reality) is supported" );
         }
         else
         {
            Console.WriteLine( "Android - ARCore (Augmeneted Reality) is not supported" );
         }
      }

      private void InitialiseSession( )
      {
         try
         {
            if( sharedSession == null )
            {
               var requestStatus = ArCoreApk.Instance.RequestInstall( this, UserRequestedARCoreInstall );

               if( requestStatus == ArCoreApk.InstallStatus.Installed )
               {
                  sharedSession = new Session( this, new List<Session.Feature> { Session.Feature.SharedCamera } );
                  sharedCamera = sharedSession.SharedCamera;
                  cameraID = sharedSession.CameraConfig.CameraId;

                  var wrappedCallback = sharedCamera.CreateARDeviceStateCallback( cameraDeviceStateCallback, backgroundHandler );
               }
               else
                  UserRequestedARCoreInstall = false;
            }
         }
         catch( UnavailableUserDeclinedInstallationException unavailableDeclinedEx )
         {
            Console.WriteLine( $"Android - Handle Exception: {unavailableDeclinedEx}" );
         }
         catch( Exception ex )
         {
            Console.WriteLine( $"Android - Handle Exception: {ex}" );
         } 
      }
   }

   public class CameraDeviceStateCallback : CameraDevice.StateCallback
   {
      public override void OnDisconnected( CameraDevice camera )
      {
         
      }

      public override void OnError( CameraDevice camera, [GeneratedEnum] CameraError error )
      {
         
      }

      public override void OnOpened( CameraDevice camera )
      {
         
      }
   }
}
