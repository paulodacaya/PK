﻿using System;
using System.Text;
using CoreBluetooth;
using Foundation;
using PK.Interfaces;
using PK.Helpers;

namespace PK.iOS.Bluetooth
{
   public sealed class IOSBluetoothLE : CBCentralManagerDelegate
   {
      private static IOSBluetoothLE instance;
      private static readonly object padlock = new object( );

      public static IOSBluetoothLE Instance
      {
         get
         {
            lock( padlock )
            {
               if( instance == null )
                  instance = new IOSBluetoothLE( );

               return instance;
            }
         }
      }

      private const string CENTRAL_RESTORE_ID = "PKCBRestoreID";
      private readonly PeripheralScanningOptions scanningOptions;

      public IBluetoothLEState StateDelegate { get; set; }
      public IBluetoothLEAdvertisement AdvertisementDelegate { get; set; }

      private bool startScanWhenPoweredOn;

      public CBCentralManager CentralManager { get; private set; }

      public IOSBluetoothLE( )
      {
         var centralInitOptions = new CBCentralInitOptions {
            ShowPowerAlert = false,
            // RestoreIdentifier = CENTRAL_RESTORE_ID // TODO Uncomment when background processing.
         };

         scanningOptions = new PeripheralScanningOptions {
            AllowDuplicatesKey = true,
         };

         CentralManager = new CBCentralManager( centralDelegate: this, queue: null, options: centralInitOptions );
      }

      public void ScanForAdvertisements( )
      {
         if( CentralManager.IsScanning )
            return;

         startScanWhenPoweredOn = true;

         if( ( CBManagerState )CentralManager.State == CBManagerState.PoweredOn )
         {
            CentralManager.ScanForPeripherals( peripheralUuids: null, options: scanningOptions );
            Console.WriteLine( "iOS - STARTED scanning for advertiselments" );
         }
         else
            Console.WriteLine( "iOS - Scanning will commence once Bluetooth is PoweredOn" );
      }

      public void StopScanningForAdvertisements( )
      {
         if( !CentralManager.IsScanning )
            return;

         startScanWhenPoweredOn = false;

         CentralManager.StopScan( );
         Console.WriteLine( "iOS - STOPPED scanning for advertisements" );
      }

      #region CBCentralManagerDelegate
      public override void DiscoveredPeripheral( CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber RSSI )
      {
         // Filter by location name
         var localName = advertisementData[ CBAdvertisement.DataLocalNameKey ] as NSString;

         if( localName == null )
            return;

         var localNameString = localName.ToString( );

         if( AnchorHelper.IsPKAnchor( localNameString ) )
         {
#if DEBUG
            //LogAdvertisementData( advertisementData, RSSI );
#endif
            AdvertisementDelegate?.ReceivedPKAdvertisement( AnchorHelper.GetAnchorType( localNameString ), RSSI.Int32Value );
         }
      }

      public override void UpdatedState( CBCentralManager central )
      {
         Console.WriteLine( $"iOS - CBCentralManager State Updated: {( CBManagerState )CentralManager.State}" );

         switch( ( CBManagerState )CentralManager.State )
         {
            case CBManagerState.PoweredOff:
               StateDelegate?.NotifyBluetoothIsOff( );
               StopScanningForAdvertisements( );
               break;

            case CBManagerState.Unsupported:
               StateDelegate?.NotifyBluetoothNotSupported( "Oh no...", "Your device does not support Bluetooth. This application can no longer be used on your device." );
               break;

            case CBManagerState.PoweredOn:
               StateDelegate?.NotifyBluetoothIsOn( );

               if( startScanWhenPoweredOn )
               {
                  Console.WriteLine( "iOS - STARTED scanning for advertiselments" );
                  CentralManager.ScanForPeripherals( peripheralUuids: null, options: scanningOptions );
               }
               break;

            case CBManagerState.Resetting:
               break;

            case CBManagerState.Unauthorized:
               break;

            case CBManagerState.Unknown:
               break;
         }
      }
      #endregion

      private void LogAdvertisementData( NSDictionary advertisementData, NSNumber RSSI )
      {
         var stringBuilder = new StringBuilder( );

         stringBuilder.AppendLine( );

         stringBuilder.Append( $"----------------------------------------------------------------" ).AppendLine( );

         var localName = advertisementData[ CBAdvertisement.DataLocalNameKey ] as NSString;
         stringBuilder.Append( $"Local Name: {localName}" ).AppendLine( );

         var rssi = RSSI;
         stringBuilder.Append( $"RSSI: {rssi}" ).AppendLine( );

         var txPowerLevel = advertisementData[ CBAdvertisement.DataTxPowerLevelKey ] as NSNumber;
         if( txPowerLevel != null )
            stringBuilder.Append( $"Tx Power Level: {txPowerLevel}" ).AppendLine( ).AppendLine( );

         var isConnectable = advertisementData[ CBAdvertisement.IsConnectable ] as NSNumber;
         if( isConnectable != null )
            stringBuilder.Append( $"Is Connectable: {isConnectable}" ).AppendLine( );

         stringBuilder.Append( $"----------------------------------------------------------------" ).AppendLine( );

         Console.WriteLine( stringBuilder );

         // EXTRA DETAILS IF NEEDED

         //var manufacturerData = advertisementData[ CBAdvertisement.DataManufacturerDataKey ] as NSData;
         //stringBuilder.Append( $"Manufacturer Data: {manufacturerData?.Description}" ).AppendLine( ).AppendLine( );

         //if( advertisementData[ CBAdvertisement.DataServiceDataKey ] is NSDictionary serviceData )
         //{
         //   stringBuilder.Append( $"Service Data:" ).AppendLine( );
         //   foreach( var pair in serviceData )
         //   {
         //      stringBuilder.Append( $"{pair.Key}: {pair.Value}" ).AppendLine( );
         //   }
         //   stringBuilder.AppendLine( );
         //}

         //if( advertisementData[ CBAdvertisement.DataServiceUUIDsKey ] is NSArray serviceUUIDs )
         //{
         //   stringBuilder.Append( $"Service UUIDs:" ).AppendLine( );
         //   for( nuint i = 0; i < serviceUUIDs.Count; i++ )
         //   {
         //      stringBuilder.Append( $"   {serviceUUIDs.ValueAt( i )}" ).AppendLine( );
         //   }
         //   stringBuilder.AppendLine( );
         //}

         //if( advertisementData[ CBAdvertisement.DataOverflowServiceUUIDsKey ] is NSArray overflowServiceUUIDs )
         //{
         //   stringBuilder.Append( $"Overflow service UUIDs:" ).AppendLine( );
         //   for( nuint i = 0; i < overflowServiceUUIDs.Count; i++ )
         //   {
         //      var cbuuid = overflowServiceUUIDs.GetItem<CBUUID>( i );
         //      stringBuilder.Append( $"   {cbuuid.Uuid}" ).AppendLine( );
         //   }
         //   stringBuilder.AppendLine( );
         //}

         //if( advertisementData[ CBAdvertisement.DataSolicitedServiceUUIDsKey ] is NSArray solicitedServiceUUIDs )
         //{
         //   stringBuilder.Append( $"Solicited service UUIDs:" ).AppendLine( );
         //   for( nuint i = 0; i < solicitedServiceUUIDs.Count; i++ )
         //   {
         //      var cbuuid = solicitedServiceUUIDs.GetItem<CBUUID>( i );
         //      stringBuilder.Append( $"   {cbuuid.Uuid}" ).AppendLine( );
         //   }
         //   stringBuilder.AppendLine( );
         //}
      }
   }
}
