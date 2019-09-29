using PK.Cloud;
using Xamarin.Essentials;
using System;
using Firebase.CloudFirestore;
using Foundation;
using PK.Models;
using Realms;
using System.Threading.Tasks;

namespace PK.ViewModels
{
   public interface IOnBoardingViewModel
   {
      void PresentLoading( );
      void DismissLoading( );
      void PresentCalibrationDataFound( string title, string message );
      void PresentCameraCalibration( );
      void NavigateToHome( );
   }

   public class CalibrateOnBoardingViewModel
   {
      private IOnBoardingViewModel viewModel;

      public readonly string Title;
      public readonly string SubTitle;
      public readonly string Message;
      public readonly string SectionOneTitle;
      public readonly string SectionOneMessage;
      public readonly string SectionTwoTitle;
      public readonly string SectionTwoMessage;

      public CalibrateOnBoardingViewModel( IOnBoardingViewModel viewModel )
      {
         this.viewModel = viewModel;

         Title = "Welcome to the Perfectly Keyless App!";
         SubTitle = "Your new digital key on a mobile phone. Before we get started, let's calibrate your device.";
         Message = "This calibration ensures your mobiles phone's position is accurate with your vehicle.";

         SectionOneTitle = "Get your Calibration Card";
         SectionOneMessage = "Place your Calibration Card on the door handle, have the image facing you.";

         SectionTwoTitle = "Follow On-Screen Instructions";
         SectionTwoMessage = "Find the image to track using the camera and hold the device at specific distance until completed.";
      }

      private static Calibration cloudCalibrationData;

      public void ActionSelected( )
      {
         // Check if the calibration has been performed previously

         viewModel.PresentLoading( );

         Task.Delay( 500 ).ContinueWith( task => {

            CloudManager.GetCalibrationData( DeviceInfo.Model, completion: ( DocumentSnapshot snapshot, NSError error ) => {

               viewModel.DismissLoading( );

               Task.Delay( 500 ).ContinueWith( anotherTask => {

                  if( error != null )
                  {
                     Console.WriteLine( "PK - Error in retrieve calibration data from Cloud service." );

                     viewModel.PresentCameraCalibration( );
                  }

                  if( snapshot.Exists )
                  {
                     // Hold cloud data in field variable

                     cloudCalibrationData = new Calibration {
                        DeviceModel = snapshot.Data[ Calibration.KEY_DEVICE_MODEL ].ToString( ),
                        Rssi_One_Metre = ( snapshot.Data[ Calibration.KEY_RSSI_ONE_METRE ] as NSNumber ).Int32Value,
                     };

                     Console.WriteLine( $"PK - Existing calibration data found for {DeviceInfo.Model}." );

                     viewModel.PresentCalibrationDataFound(
                        "Calibration Data Found in the Cloud!",
                        $"We have found existing calibration data for {DeviceInfo.Model}. Do you want to use this data or perform your own calibration?"
                     );
                  }
                  else
                  {
                     Console.WriteLine( $"PK - No existing calibration data found for your {DeviceInfo.Model}." );

                     viewModel.PresentCameraCalibration( );
                  }

               } );

            } );

         } );
      }

      public void ActionPerformCaliberationMySelf( )
      {
         viewModel.PresentCameraCalibration( );
      }

      public void ActionUseExistingCalibrationData( )
      {
         var realm = Realm.GetInstance( PKRealm.Configuration );

         realm.Write( ( ) => realm.Add( cloudCalibrationData, update: true ) );

         cloudCalibrationData = null;

         viewModel.NavigateToHome( );
      }
   }
}
