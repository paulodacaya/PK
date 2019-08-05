using System;
using System.Text;
using CoreBluetooth;
using Foundation;
using PK.Interfaces;
using PK.Models;

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
         CentralManager.ScanForPeripherals( peripheralUuids: null, options: scanningOptions );
      }

      public void StopScanningForAdvertisements( )
      {
         Console.WriteLine( "iOS - Stop Scanning for advertisements." );
         CentralManager.StopScan( );
      }

      #region CBCentralManagerDelegate
      public override void DiscoveredPeripheral( CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber RSSI )
      {
#if DEBUG
         LogAdvertisementData( advertisementData, RSSI );
#endif

         AdvertisementDelegate?.ReceivedAdvertisement( anchorLocationID: AnchorLocation.PassangerDoor, RSSI.Int32Value );
      }

      public override void UpdatedState( CBCentralManager central )
      {
         Console.WriteLine( $"CBCentralManager State Updated: {( CBManagerState )CentralManager.State}" );

         switch( ( CBManagerState )CentralManager.State )
         {
            case CBManagerState.PoweredOff:
               StateDelegate?.NotifyBluetoothIsOff( );

               if( CentralManager.IsScanning )
                  StopScanningForAdvertisements( );
               break;

            case CBManagerState.Unsupported:
               StateDelegate?.NotifyBluetoothNotSupported( );
               break;

            case CBManagerState.PoweredOn:
               StateDelegate?.NotifyBluetoothIsOn( );
               //ScanForAdvertisements( );
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
         Console.WriteLine( advertisementData );

         return;
         var stringBuilder = new StringBuilder( );
         var rssi = RSSI;
         stringBuilder.Append( $"RSSI: {rssi}" ).AppendLine( ).AppendLine( );

         var localName = advertisementData[ CBAdvertisement.DataLocalNameKey ] as NSString;
         stringBuilder.Append( $"Local Name: {localName}" ).AppendLine( ).AppendLine( );

         var manufacturerData = advertisementData[ CBAdvertisement.DataManufacturerDataKey ] as NSData;
         stringBuilder.Append( $"Manufacturer Data: {manufacturerData?.Description}" ).AppendLine( ).AppendLine( );

         if( advertisementData[ CBAdvertisement.DataServiceDataKey ] is NSDictionary serviceData )
         {
            stringBuilder.Append( $"Service Data:" ).AppendLine( );
            foreach( var pair in serviceData )
            {
               stringBuilder.Append( $"{pair.Key}: {pair.Value}" ).AppendLine( );
            }
            stringBuilder.AppendLine( );
         }

         if( advertisementData[ CBAdvertisement.DataServiceUUIDsKey ] is NSArray serviceUUIDs )
         {
            stringBuilder.Append( $"Service UUIDs:" ).AppendLine( );
            for( nuint i = 0; i < serviceUUIDs.Count; i++ )
            {
               stringBuilder.Append( $"   {serviceUUIDs.ValueAt( i )}" ).AppendLine( );
            }
            stringBuilder.AppendLine( );
         }

         if( advertisementData[ CBAdvertisement.DataOverflowServiceUUIDsKey ] is NSArray overflowServiceUUIDs )
         {
            stringBuilder.Append( $"Overflow service UUIDs:" ).AppendLine( );
            for( nuint i = 0; i < overflowServiceUUIDs.Count; i++ )
            {
               var cbuuid = overflowServiceUUIDs.GetItem<CBUUID>( i );
               stringBuilder.Append( $"   {cbuuid.Uuid}" ).AppendLine( );
            }
            stringBuilder.AppendLine( );
         }

         var txPowerLevel = advertisementData[ CBAdvertisement.DataTxPowerLevelKey ] as NSNumber;
         stringBuilder.Append( $"Tx Power Level: {txPowerLevel}" ).AppendLine( ).AppendLine( );

         var isConnectable = advertisementData[ CBAdvertisement.DataTxPowerLevelKey ] as NSNumber;
         stringBuilder.Append( $"Is Connectable: {isConnectable}" ).AppendLine( ).AppendLine( );

         if( advertisementData[ CBAdvertisement.DataSolicitedServiceUUIDsKey ] is NSArray solicitedServiceUUIDs )
         {
            stringBuilder.Append( $"Solicited service UUIDs:" ).AppendLine( );
            for( nuint i = 0; i < solicitedServiceUUIDs.Count; i++ )
            {
               var cbuuid = solicitedServiceUUIDs.GetItem<CBUUID>( i );
               stringBuilder.Append( $"   {cbuuid.Uuid}" ).AppendLine( );
            }
            stringBuilder.AppendLine( );
         }

         stringBuilder.Append( $"----------------------------------------------------------------" ).AppendLine( );

         Console.WriteLine( stringBuilder );
      }
   }
}
