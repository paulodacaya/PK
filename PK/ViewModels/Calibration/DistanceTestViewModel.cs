using System;

namespace PK.ViewModels
{
   public interface IDistanceTestViewModel
   {
      void DistanceChanged( string distance );
   }

   public class DistanceTestViewModel
   {
      private readonly IDistanceTestViewModel viewModel;

      public string MessageText => "Distance from your vehicle:";

      public DistanceTestViewModel( IDistanceTestViewModel viewModel )
      {
         this.viewModel = viewModel;
      }

      public void SetRSSI( int anchorLocationID, int RSSI )
      {
         var distance = CalculateDistance( RSSI );

         viewModel.DistanceChanged( $"1.2 m" );
      }

      private double CalculateDistance( int RSSI )
      {
         double distance = Math.Sqrt( 1.0 / Math.Abs( RSSI ) );
         distance = Math.Round( distance, 2, MidpointRounding.ToEven );

         // Calibration occurs here...
         //    1. Map Distance to RSSI
         //    2. Theoretically only needs to occur once

         return distance;
      }
   }
}
