using System;
using System.Collections.Generic;
using System.Linq;
using PK.Models;
using Realms;

namespace PK.ViewModels
{
   public interface IManualCalibrationViewModel
   {
      void DistanceSelectedAt( int index );
      void DistanceCalibratedAt( int index, double percent );
      void CalibrationFinished( );
      void NotifyCalibrationIncomplete( );
   }

   public class ManualCalibrationViewModel
   {
      private readonly IManualCalibrationViewModel viewModel;
      private CalibrationData calibrationData;
      private int selectedDistance;

      public string Title => "Manual Calibration";
      public string SubTitle => "Please stand at your selected distance away from your vehicle before pressing 'calibrate':";
      public int TotalDistances => Distances.Length;

      public string[ ] Distances
      {
         get
         {
            var distances = new List<string>( );

            foreach( var distance in calibrationData.LocationsData.First( ).DistancesData )
            {
               distances.Add( $"{distance.Distance}m" );
            }

            return distances.ToArray( );
         }
      }

      public ManualCalibrationViewModel( IManualCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;

         SetupCalibrationData( );
      }

      private void SetupCalibrationData( )
      {
         var realm = Realm.GetInstance( PKRealm.Configuration );
         calibrationData = realm.All<CalibrationData>( ).FirstOrDefault( );

         if( calibrationData == null )
            calibrationData = PKRealm.CreateDefaultCalibrationData( );
      }

      public void SelectDistance( int index )
      {
         selectedDistance = index;
         viewModel.DistanceSelectedAt( selectedDistance );
      }

      public void ActionCalibrate( )
      {
         /*
          * 1. Notify to turn advertisements ON.
          * 2. Feed advertisements through view model.
          * 3. Notify percentage has been calibrated => update UI.
          * 4. This method should be quite complex.         
          */


         viewModel.DistanceCalibratedAt( selectedDistance, 1.0 );
      }

      public void ActionDone( )
      {
         var passangerDoorLocation = calibrationData.LocationsData.First( l => l.LocationID == AnchorLocation.PassangerDoor );

         if( passangerDoorLocation.Completed )
         {
            viewModel.CalibrationFinished( );
         }
         else
         {
            viewModel.NotifyCalibrationIncomplete( );
         }
      }
   }
}
