﻿using System;
using System.Collections.Generic;
using System.Timers;
using PK.Helpers;
using PK.Interfaces;
using PK.iOS.Bluetooth;
using PK.Models;
using Realms;
using Xamarin.Essentials;

namespace PK.ViewModels
{
   public interface IHomeViewModel
   {
      void ReloadRowAt( int row );
      void ShowRecalibratePrompt( string title, string message, string cancelText, string actionText );
      void ShowDeleteDataPrompt( string title, string message, string cancelText, string actionText );
      void PresentCameraRecalibration( );
      void NotifyDeviceInZoneStateChanged( bool isInZone );
      void UpdateRssi( int rssi );
      void NavigateToOnBoarding( );
   }

   public class HomeViewModel : IBluetoothLEAdvertisement
   {
      private static Timer timer;
      private static bool isTimerRunning;

      static HomeViewModel( )
      {
         timer = new Timer( interval: 2000 );
         timer.AutoReset = false;
         timer.Elapsed += ( sender, elapsedEventArgs ) => stopTimer( );
      }

      static void startTimer( )
      {
         isTimerRunning = true;
         timer.Start( );
         Console.WriteLine( "PK - Timer stared" );
      }

      static void stopTimer( )
      {
         isTimerRunning = false;
         timer.Stop( );
         Console.WriteLine( "PK - Timer stopped" );
      }

      public readonly string VehicleMessage;

      public string GreetingMessage
      {
         get
         {
            if( DateTime.Now.Hour >= 5 && DateTime.Now.Hour < 12 )
               return "Good morning";

            if( DateTime.Now.Hour >= 12 )
               return "Good afternoon";

            if( DateTime.Now.Hour >= 16 )
               return "Good evening";

            return "Good night";
         }
      }

      public List<CardItemViewModel> CardItemViewModels { get; }

      private readonly IHomeViewModel viewModel;
      private readonly int rssi_one_meter;
      private bool isInZone;

      public HomeViewModel( IHomeViewModel viewModel )
      {
         this.viewModel = viewModel;

         IOSBluetoothLE.Instance.AdvertisementDelegate = this;
         IOSBluetoothLE.Instance.ScanForAdvertisements( );

         rssi_one_meter = Realm.GetInstance( PKRealm.Configuration )
            .Find<Calibration>( DeviceInfo.Model ).Rssi_One_Metre;

         VehicleMessage = "Current Vehicle: Mazda 3 (ABE 674)";

         CardItemViewModels = new List<CardItemViewModel>( );

         var calibrateCard = new CardItemViewModel {
            Id = CardItemViewModel.c_id_calibrate,
            Title = "Re-calibrate your device",
            Message = "A simple AR (Augmeneted Reality) calibration experience to ensure your mobiles proximity is accurate with your vehicle."
         };
         calibrateCard.Selected += HandleCalibrationCardSelected;
         CardItemViewModels.Add( calibrateCard );

         var vehicleZoneVisualCard = new CardItemViewModel {
            Id = CardItemViewModel.c_id_vehicleZoneVisual,
            Type = CardItemViewModel.c_type_expandable,
            Title = "See if your device is within your vehicle's zone",
            Message = "Live image update if your device is within vehicle zone or not."
         };
         vehicleZoneVisualCard.Selected += HandleVehicleZoneVisualCardSelected;
         CardItemViewModels.Add( vehicleZoneVisualCard );

         var deleteDataCard = new CardItemViewModel {
            Id = CardItemViewModel.c_id_deleteData,
            Title = "Delete calibration data",
            Message = "Delete all calibration data in this device."
         };
         deleteDataCard.Selected += HandleDeleteDataCardSelected;
         CardItemViewModels.Add( deleteDataCard );
      }

      void IBluetoothLEAdvertisement.ReceivedPKAdvertisement( Anchor anchor, int RSSI )
      {
         viewModel.UpdateRssi( RSSI );

         var localVariableIsInZone = RSSI >= rssi_one_meter;
         // Prevent from notifying UI unless the state has changed
         if( localVariableIsInZone != isInZone && !isTimerRunning )
         {
            isInZone = localVariableIsInZone;
            Console.WriteLine( $"PK - Device in zone state change to: {isInZone}" );
            viewModel.NotifyDeviceInZoneStateChanged( isInZone );
            startTimer( );
         }
      }

      public void ActionRecalibrateSelected( ) => viewModel.PresentCameraRecalibration( );

      public void ActionDeleteDatabase( )
      {
         var realm = Realm.GetInstance( PKRealm.Configuration );

         realm.Write( ( ) => {
            realm.RemoveAll<Calibration>( );
         } );

         viewModel.NavigateToOnBoarding( );
      }

      private void HandleCalibrationCardSelected( object sender, EventArgs eventArgs )
      {
         viewModel.ShowRecalibratePrompt(
            title: "Device Already Calibrated",
            message: "You don't need to calibrate your device again, are you sure you want to calibrate your device?",
            cancelText: "Cancel",
            actionText: "Recalibrate"
         );
      }

      private void HandleVehicleZoneVisualCardSelected( object sender, EventArgs eventArgs )
      {
         if( sender is CardItemViewModel cardItemViewModel )
         {
            cardItemViewModel.IsExpanded = !cardItemViewModel.IsExpanded;
            viewModel.ReloadRowAt( CardItemViewModels.IndexOf( cardItemViewModel ) );
         }
      }

      private void HandleDeleteDataCardSelected( object sender, EventArgs e )
      {
         viewModel.ShowDeleteDataPrompt(
            title: "Are you sure?",
            message: "Deleting calibration will reset the application.",
            cancelText: "Cancel",
            actionText: "Delete data"
         );
      }
   }

   public class CardItemViewModel
   {
      public event EventHandler Selected;

      public const string c_id_calibrate = "id_calibrate";
      public const string c_id_vehicleZoneVisual = "id_vehicleZoneVisual";
      public const string c_id_deleteData = "id_deleteData";
      public const string c_type_expandable = "type_expandable";

      public string Id { get; set; } = string.Empty;
      public string Type { get; set; } = string.Empty;
      public string Title { get; set; } = string.Empty;
      public string Message { get; set; } = string.Empty;

      public bool IsExpanded { get; set; }

      public void ActionSelected( ) => Selected?.Invoke( this, new EventArgs( ) );
   }
}
