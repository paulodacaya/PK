using System;
using System.Collections.Generic;
using System.Linq;
using PK.Models;
using Realms;
using Xamarin.Essentials;

namespace PK.ViewModels
{
   public interface ICameraCalibrationViewModel
   {
      void UpdateCalibrationProgress( float percent );
      void StopAdvertisingAndReset( );
      void ShowCalibrationCompleted( );
      void NavigateToHome( );
   }

   public class CameraCalibrationViewModel
   {
      // Only calibrate (RSSI to distance mapping) when distance is 0.5 metres
      public readonly double RSSICapturableDistance = 0.5;

      private readonly ICameraCalibrationViewModel viewModel;

      private bool calibrationCompleted;
      private readonly float completionCount = 100f;
      private float rssiCount;
      private readonly List<int> RssiList;

      public readonly string Message;

      public Calibration CalibrationData => Realm.GetInstance( PKRealm.Configuration ).Find<Calibration>( DeviceInfo.Model );

      public CameraCalibrationViewModel( ICameraCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;

         RssiList = new List<int>( );

         Message = "The following information has been obtained from the calibration. You can perform the calibration again in Settings.";
      }

      public void calibrateRSSI( int RSSI )
      {
         // Prevent this method from being called after calibration is completed.
         if( calibrationCompleted )
            return;

         RssiList.Add( RSSI );

         rssiCount++;

         var progressPercent = rssiCount / completionCount;

         Console.WriteLine( $"PK - RSSI receieved: {RSSI}. Progress: {progressPercent * 100}%" );

         viewModel.UpdateCalibrationProgress( progressPercent );

         //  Calibration is completed
         if( Math.Abs( rssiCount - completionCount ) < double.Epsilon )
         {
            calibrationCompleted = true;

            Console.WriteLine( $"PK - Calibration completed" );

            // iOS and Android should STOP advertising and do any clean up.
            viewModel.StopAdvertisingAndReset( );

            // Check for any outliers and remove them.
            var maxRssiAt0_5_m = CheckForOutliers( RssiList.Min( ) );

            Console.WriteLine( $"PK - Maximum RSSI at 0.5 metres is {maxRssiAt0_5_m}" );

            var kConstant = CalculateKConstant( maxRssiAt0_5_m, RSSICapturableDistance );

            Console.WriteLine( $"PK - Calculated KConstant is {kConstant}" );

            Console.WriteLine( "PK - Storing calibration data to realm commenced" );

            try
            {
               var realm = Realm.GetInstance( PKRealm.Configuration );
               var calibration = realm.Find<Calibration>( DeviceInfo.Model );

               if( calibration == null )
                  throw new KeyNotFoundException( message: "Calibration object could not be found in realm. Object is NULL." );

               Console.WriteLine( "PK - Storing calibration data to realm commenced" );

               var rssi_1_0_m = CalculateRssiForDistance( 1.0, kConstant );

               Console.WriteLine( $"PK - Calculated RSSI at 1.0m: {rssi_1_0_m}" );

               var rssi_1_5_m = CalculateRssiForDistance( 1.5, kConstant );

               Console.WriteLine( $"PK - Calculated RSSI at 1.5m: {rssi_1_5_m}" );

               var rssi_2_0_m = CalculateRssiForDistance( 2.0, kConstant );

               Console.WriteLine( $"PK - Calculated RSSI at 2.0m: {rssi_2_0_m}" );

               var rssi_2_5_m = CalculateRssiForDistance( 2.5, kConstant );

               Console.WriteLine( $"PK - Calculated RSSI at 2.5m: {rssi_2_5_m}" );

               var rssi_3_0_m = CalculateRssiForDistance( 3.0, kConstant );

               Console.WriteLine( $"PK - Calculated RSSI at 3.0m: {rssi_3_0_m}" );

               realm.Write( ( ) => {
                  calibration.RSSI_0_5_m = maxRssiAt0_5_m;
                  calibration.RSSI_1_0_m = rssi_1_0_m;
                  calibration.RSSI_1_5_m = rssi_1_5_m;
                  calibration.RSSI_2_0_m = rssi_2_0_m;
                  calibration.RSSI_2_5_m = rssi_2_5_m;
                  calibration.RSSI_3_0_m = rssi_3_0_m;
               } );
            }
            catch( Exception ex )
            {
               Console.WriteLine( $"PK - Calibration Failed: {ex.Message}" );

               return;
            }

            Console.WriteLine( "PK - Storing calibration data successfully completed" );

            viewModel.ShowCalibrationCompleted( );
         }
      }

      private int CheckForOutliers( int rssi )
      {
         if( RssiList.Count( r => r == rssi ) <= 2 )
         {
            Console.WriteLine( $"PK - Outlier RSSI: {rssi}." );

            return CheckForOutliers( ++rssi );
         }

         return rssi;
      }

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

      /*

         CURRENTLY NOT IN USE

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

      public void ActionFinished( )
      {
         // TODO Connect to FireStore Database and store calibration informartion for this Device Model.

         // Loading dialog screen during this process.

         viewModel.NavigateToHome( );
      }
   }
}
