using System;
using Realms;

namespace PK.Models
{
   public static class PKRealm
   {
      private static RealmConfiguration configuration;
      public static RealmConfiguration Configuration
      {
         get
         {
            if( configuration == null )
            {
               configuration = new RealmConfiguration( optionalPath: "PKRealm.realm" ) {
                  SchemaVersion = 1,
#if DEBUG
                  ShouldDeleteIfMigrationNeeded = true,
#endif
               };
            }

            return configuration;
         }
      }

      public static string Path => Configuration.DatabasePath;

      public static CalibrationData CreateDefaultCalibrationData( )
      {
         var realm = Realm.GetInstance( Configuration );
         CalibrationData calibrationData = null;

         try
         {
            using( var transaction = realm.BeginWrite( ) )
            {
               calibrationData = new CalibrationData {
                  CalibrationID = 0,
               };

               var locationData = new CalibrationLocationData {
                  LocationID = AnchorLocation.PassangerDoor
               };

               calibrationData.LocationsData.Add( locationData );

               for( var i = 0; i < 8; i++ )
               {
                  var distanceData = new CalibrationDistanceData {
                     Distance = i + 1
                  };

                  locationData.DistancesData.Add( distanceData );
               }

               realm.Add( calibrationData );
               transaction.Commit( );
            }
         }
         catch( Exception ex )
         {
            throw new Exception( $"Creating calibration data failed. Exception: {ex.Message}" );
         }

         return calibrationData;
      }
   }
}