using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using PK.Interfaces;
using PK.Common;

namespace PK.Droid.Bluetooth
{
   public class AndroidBluetoothLE : ScanCallback
   {
      private static AndroidBluetoothLE instance;
      private static readonly object padlock = new object( );

      public static AndroidBluetoothLE Instance
      {
         get
         {
            lock( padlock )
            {
               if( instance == null )
                  instance = new AndroidBluetoothLE( );

               return instance;
            }
         }
      }

      public IBluetoothLEState StateDelegate { get; set; }
      public IBluetoothLEAdvertisement AdvertisementDelegate { get; set; }

      private readonly BluetoothManager bluetoothManager;
      private readonly BluetoothAdapter bluetoothAdapter; // Single BluetoothAdapter for entire system
      private readonly BluetoothLeScanner bluetoothLeScanner;
      private readonly List<ScanFilter> bluetoothScanFilters;
      private readonly ScanSettings bluetoothScanSettings;

      public AndroidBluetoothLE( )
      {
         bluetoothManager = Application.Context.GetSystemService( Context.BluetoothService ) as BluetoothManager;
         bluetoothAdapter = bluetoothManager.Adapter;

         bluetoothLeScanner = bluetoothAdapter.BluetoothLeScanner;

         bluetoothScanSettings = new ScanSettings.Builder( )
            .SetScanMode( Android.Bluetooth.LE.ScanMode.LowLatency )
            .SetLegacy( true )
            .SetMatchMode( BluetoothScanMatchMode.Aggressive )
            .SetReportDelay( 0 )
            .Build( );

         bluetoothScanFilters = new List<ScanFilter>( ); // TODO Set scanning filters after calibration to known nodes?
      }

      public void EnableBluetooth( )
      {
         bluetoothAdapter.Enable( );

         // Request to turn bluetooth on with Intent
         //var enableBluetoothIntent = new Intent( action: BluetoothAdapter.ActionRequestEnable );
         //zoningActivity.StartActivityForResult( enableBluetoothIntent, 100 );
      }

      public void ScanForAdvertisements(  )
      {
         if( !Application.Context.PackageManager.HasSystemFeature( PackageManager.FeatureBluetoothLe ) )
         {
            Console.WriteLine( "Android - Bluetooth not supported." );
            StateDelegate.NotifyBluetoothNotSupported( );
            return;
         }

         if( !bluetoothAdapter.IsEnabled )
         {
            Console.WriteLine( "Android - Bluetooth is turned off." );
            StateDelegate.NotifyBluetoothIsOff( );
            bluetoothAdapter.Enable( );
            StateDelegate.NotifyBluetoothIsOn( );
            Console.WriteLine( "Android - Bluetooth is turned on." );
         }


         Console.WriteLine( "Android - Verifying if location permission is granted." );
         if( StateDelegate.VerifyLocationPermission( ) )
         {
            Console.WriteLine( "Android - Location permission is granted." );
            Console.WriteLine( "Android - Scanning for advertisements now." );
            bluetoothLeScanner.StartScan( filters: bluetoothScanFilters, settings: bluetoothScanSettings, callback: this );
         }
      }

      public void StopScanningForAdvertisements( )
      {
         Console.WriteLine( "Android - Stopped scanning for advertisements." );
         bluetoothLeScanner.StopScan( this );
      }

      #region ScanCallback
      public override void OnScanResult( [GeneratedEnum] ScanCallbackType callbackType, ScanResult result )
      {
#if DEBUG
         LogAdvertisementData( result );
#endif
      }

      public override void OnScanFailed( [GeneratedEnum] ScanFailure errorCode )
      {
         if( errorCode == ScanFailure.AlreadyStarted )
         {
            Console.WriteLine( "Android - Scanning has already started." );
            return;
         }
            
         if( errorCode == ScanFailure.FeatureUnsupported )
         {
            Console.WriteLine( "Android - Scanning failed: Feature not supported." );
            return;
         }

         Console.WriteLine( "Android - Scanning failed: Attempting to start scan again." );
         bluetoothLeScanner.StartScan( filters: bluetoothScanFilters, settings: bluetoothScanSettings, callback: this );
      }
      #endregion

      private void LogAdvertisementData( ScanResult result )
      {
         var stringBuilder = new StringBuilder( );

         var rssi = result.Rssi;
         stringBuilder.Append( $"RSSI: {rssi}" ).AppendLine( ).AppendLine( );

         var txPower = result.TxPower;
         stringBuilder.Append( $"Tx Power: {txPower}" ).AppendLine( ).AppendLine( );

         var advertisingSid = result.AdvertisingSid;
         stringBuilder.Append( $"Advertising SID: {advertisingSid}" ).AppendLine( ).AppendLine( );

         var dataStatus = result.DataStatus == DataStatus.Complete ? "Completed" : "Truncated";
         stringBuilder.Append( $"Data Status: {dataStatus}" ).AppendLine( ).AppendLine( );

         var device = result.Device;
         stringBuilder.Append( "Device:" ).AppendLine( );
         stringBuilder.Append( $"   Address: {device.Address}" ).AppendLine( );
         stringBuilder.Append( $"   Name: {device.Name}" ).AppendLine( ).AppendLine( );

         var periodicAdvertisingInterval = result.PeriodicAdvertisingInterval;
         stringBuilder.Append( $"Periodic Advertising Interval: {periodicAdvertisingInterval}" ).AppendLine( ).AppendLine( );

         var primaryPhysicalLayer = result.PrimaryPhy;
         stringBuilder.Append( $"Primary Physical Layer: {primaryPhysicalLayer}" ).AppendLine( ).AppendLine( );

         var secondaryPhysicalLayer = result.SecondaryPhy;
         stringBuilder.Append( $"Secondary Physical Layer: {secondaryPhysicalLayer}" ).AppendLine( ).AppendLine( );

         var timeStampNano = result.TimestampNanos;
         stringBuilder.Append( $"Time Stamp (nanoseconds): {timeStampNano}" ).AppendLine( ).AppendLine( );

         var scanRecord = result.ScanRecord;
         stringBuilder.Append( "Scan Record:" ).AppendLine( );
         stringBuilder.Append( $"   Advertise flags: {scanRecord.AdvertiseFlags}" ).AppendLine( );
         stringBuilder.Append( $"   Device name: {scanRecord.DeviceName}" ).AppendLine( );
         stringBuilder.Append( $"   Raw bytes of scan record: { scanRecord.GetBytes( ).ToReadableByteArray( ) }" ).AppendLine( );

         if( scanRecord.ServiceData != null )
         {
            stringBuilder.Append( $"   Service data:" ).AppendLine( );
            foreach( var dataItem in scanRecord.ServiceData )
            {
               stringBuilder.Append( $"      {dataItem.Key.ToString( )}: {dataItem.Value.ToReadableByteArray( )}" ).AppendLine( );
            }
         }

         if( scanRecord.ServiceUuids != null )
         {
            stringBuilder.Append( $"   Service UUIDs:" ).AppendLine( );
            foreach( var dataItem in scanRecord.ServiceUuids )
            {
               stringBuilder.Append( $"      {dataItem.ToString( )}" ).AppendLine( );
            }
         }

         stringBuilder.Append( $"   Tx Power Level of Packet: {scanRecord.TxPowerLevel} dBm" ).AppendLine( ).AppendLine( );

         stringBuilder.Append( $"----------------------------------------------------------------" ).AppendLine( );

         Console.WriteLine( stringBuilder );
      }
   }
}
