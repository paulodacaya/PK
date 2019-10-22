using System;
using PK.Models;
using Realms;
using Xamarin.Essentials;

namespace PK.Helpers
{
   public static class CalibrationHelper
   {
      public static bool DeviceIsCalibrated( )
      {
         var realm = Realm.GetInstance( PKRealm.Configuration );
         var calibration = realm.Find<Calibration>( DeviceInfo.Model );

         if( calibration == null )
         {
            Console.WriteLine( $"PK - Creating new calibration data for device model: {DeviceInfo.Model}." );

            realm.Write( ( ) => {
               var newCalibration = new Calibration {
                  DeviceModel = DeviceInfo.Model
               };

               realm.Add( newCalibration );
            } );

            return false;
         }

         return calibration.IsCalibrated;
      }
   }
}
