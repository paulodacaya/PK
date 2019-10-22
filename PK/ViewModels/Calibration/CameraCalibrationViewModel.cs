using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using PK.Cloud;
using PK.Models;
using Realms;
using Xamarin.Essentials;

namespace PK.ViewModels
{
   public interface ICameraCalibrationViewModel
   {
      void UpdateCalibrationProgress( float percent );
      void StopAdvertisingAndReset( );
      void ShowCalibrationCompleted( string calibrationData );
      void PresentLoading( );
      void DismissLoading( );
      void NavigateToHome( );
   }

   public class CameraCalibrationViewModel
   {
      // Only calibrate (RSSI to distance mapping) when distance is 1 metre
      public readonly double RSSICapturableDistance = 1;

      private readonly ICameraCalibrationViewModel viewModel;

      private bool calibrationCompleted;
      private readonly float completionCount = 200f;
      private float rssiCount;
      private readonly List<int> RssiList;
      private readonly HashSet<int> RssiHashSet; 

      public readonly string Message;

      public CameraCalibrationViewModel( ICameraCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;

         RssiList = new List<int>( );
         RssiHashSet = new HashSet<int>( );

         Message = "The following information has been obtained from the calibration. You can perform the calibration again in your home screen.";
      }

      public void calibrateRssi( int RSSI )
      {
         // Prevent this method from being called after calibration is completed.
         if( calibrationCompleted )
            return;

         RssiList.Add( RSSI );
         RssiHashSet.Add( RSSI );

         rssiCount++;

         var progress = rssiCount / completionCount;

         Console.WriteLine( $"PK - RSSI receieved: {RSSI}. Progress: {progress * 100}%" );

         viewModel.UpdateCalibrationProgress( progress );

         //  Calibration is completed
         if( Math.Abs( rssiCount - completionCount ) < double.Epsilon )
         {
            calibrationCompleted = true;

            Console.WriteLine( $"PK - Calibration completed" );

            Console.WriteLine( $"PK - Printing out Calibration data" );

            foreach( var rssi in RssiHashSet )
               Console.WriteLine( $"   PK - RSSI {rssi}, count: {RssiList.Count( r => r == rssi )}" );   

            // iOS and Android should STOP advertising and do any clean up.
            viewModel.StopAdvertisingAndReset( );

            // Check for any outliers and remove them.
            var max_Rssi_One_Metre = CheckAndRemoveOutliers( RssiList.Min( ) );

            Console.WriteLine( $"PK - Maximum RSSI at 1 metre is {max_Rssi_One_Metre}" );

            Console.WriteLine( "PK - Storing calibration data to realm commenced" );

            try
            {
               var realm = Realm.GetInstance( PKRealm.Configuration );
               var calibration = realm.Find<Calibration>( DeviceInfo.Model );

               realm.Write( ( ) => {
                  calibration.Rssi_One_Metre = max_Rssi_One_Metre;
               } );
            }
            catch( Exception ex )
            {
               Console.WriteLine( $"PK - Calibration Failed: {ex.Message}" );

               return;
            }

            Console.WriteLine( "PK - Storing calibration data successfully completed" );

            viewModel.ShowCalibrationCompleted( max_Rssi_One_Metre.ToString( ) );
         }
      }

      private int CheckAndRemoveOutliers( int rssi )
      {
         if( RssiList.Count( r => r == rssi ) <= 10 )
         {
            Console.WriteLine( $"PK - Outlier RSSI: {rssi}." );
            return CheckAndRemoveOutliers( ++rssi );
         }

         return rssi;
      }

      public void ActionFinished( )
      {
         viewModel.PresentLoading( );

         Task.Delay( 1000 ).ContinueWith( task => {

            // Attempt to store data in the cloud
            var realm = Realm.GetInstance( PKRealm.Configuration );
            var calibration = realm.Find<Calibration>( DeviceInfo.Model );

            CloudManager.SetCalibrationData( calibration, ( NSError error ) => {

               viewModel.DismissLoading( );

               Task.Delay( 1000 ).ContinueWith( anotherTask => {

                  if( error != null )
                  {
                     Console.WriteLine( "PK - Error saving calibration data to cloud. Did not save." );
                  }

                  viewModel.NavigateToHome( );

               } );
            } );
         } );
      }

      /*
       
      CURRENTLY NOT IN USE

      public double CalculateKConstant( double rssi, double distance )
      {
         // Received power is inversly proportional to the square
         // of the distance between transmitter and recevier.
         // RSSI is proportional to K * 1/d^2. Therefore, K = RSSI x d^2.

         return Math.Round( rssi * Math.Pow( distance, 2.0 ), 2 );
      }

      public int CalculateRssiForDistance( double distance, double kConstant )
      {
         // Received power is inversly proportional to the square
         // of the distance between transmitter and recevier.
         // RSSI is proportional to K * 1/d^2. Therefore, RSSI = k / d^2.

         return ( int )Math.Ceiling( kConstant / Math.Pow( distance, 2.0 ) );
      }

      public void ActionUseStandardZones( )
      {
         // Store 'default' zones.

         var realm = Realm.GetInstance( PKRealm.Configuration );

         realm.Write( ( ) => {
            realm.Add( new Zone { ZoneLocation = Zone.Location_Welcome, Radius = 2.0 } );
            realm.Add( new Zone { ZoneLocation = Zone.Location_Passenger, Radius = 1.0 } );
            realm.Add( new Zone { ZoneLocation = Zone.Location_Driver, Radius = 1.0 } );
            realm.Add( new Zone { ZoneLocation = Zone.Location_Boot, Radius = 1.0 } );
         } );

         viewModel.NavigateToHome( );
      }

      */
   }
}
