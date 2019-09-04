using System;
using System.Collections.Generic;
using System.Linq;
using PK.Models;
using Realms;

namespace PK.ViewModels
{
   public interface ICameraCalibrationViewModel
   {
      void UpdateCalibrationProgress( float percent );
      void StopAdvertisingAndReset( );
      void ShowCalibrationCompleted( );
      void NavigateToConfigureZones( );
      void NavigateToHome( );
   }

   public class CameraCalibrationViewModel
   {
      // Only calibrate (RSSI to distance mapping) when distance is 0.5 metres
      public readonly double RSSICapturableDistance = 0.5;

      private readonly ICameraCalibrationViewModel viewModel;

      private bool calibrationCompleted;
      private float rssiCount;
      private readonly float completionCount = 80f;

      private readonly HashSet<int> RSSISet;

      public CameraCalibrationViewModel( ICameraCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;
         RSSISet = new HashSet<int>( );
      }

      public void calibrateRSSI( int RSSI )
      {
         // Prevent this method from being called after calibration is completed.
         if( calibrationCompleted )
            return;

         RSSISet.Add( RSSI );

         rssiCount++;

         var progressPercent = rssiCount / completionCount;
         Console.WriteLine( $"PK - RSSI receieved: {RSSI}. Progress: {progressPercent * 100}%" );

         viewModel.UpdateCalibrationProgress( progressPercent );

         if( Math.Abs( rssiCount - completionCount ) < double.Epsilon )
         {
            calibrationCompleted = true;
            Console.WriteLine( $"PK - Calibration completed" );

            // iOS and Android should STOP advertising and do any clean up.
            viewModel.StopAdvertisingAndReset( );

            var minRSSI = RSSISet.Min( );
            Console.WriteLine( $"PK - Min RSSI at {RSSICapturableDistance}m: {minRSSI}" );

            var maxRSSI = RSSISet.Max( );
            Console.WriteLine( $"PK - Max RSSI at {RSSICapturableDistance}m: {maxRSSI}" );

            var minKConstant = CalculateKConstant( minRSSI, RSSICapturableDistance );
            Console.WriteLine( $"PK - Min K Constant calculated: at {RSSICapturableDistance}m: {minKConstant}" );

            var maxKConstant = CalculateKConstant( maxRSSI, RSSICapturableDistance );
            Console.WriteLine( $"PK - Max K Constant calculated: at {RSSICapturableDistance}m: {maxKConstant}" );

            Console.WriteLine( "PK - Storing calibration data to realm commenced" );

            try
            {
               var realm = Realm.GetInstance( PKRealm.Configuration );
               var calibration = realm.Find<Calibration>( Calibration.PrimaryKey );

               if( calibration == null )
                  throw new KeyNotFoundException( message: "Calibration object could not be found in realm. Object is NULL." );

               realm.Write( ( ) => {
                  calibration.MinRSSI = minRSSI;
                  calibration.MaxRSSI = maxRSSI;
                  calibration.MinKConstant = minKConstant;
                  calibration.MaxKConstant = maxKConstant;
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

      public double CalculateKConstant( int RSSI, double distance )
      {
         // Received power is inversly proportional to the square
         // of the distance between transmitter and recevier.
         // RSSI is proportional to K 1/d^2. Therefore, K = RSSI x d^2.

         return RSSI * Math.Pow( distance, 2.0 );
      }

      public void ActionSelected( )
      {
         viewModel.NavigateToConfigureZones( );
      }

      public void UseDefaultZonesSelected( )
      {
         viewModel.NavigateToHome( );
      }
   }
}
