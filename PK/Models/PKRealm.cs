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

      public static bool DeviceIsCalibrated( )
      {
         var realm = Realm.GetInstance( Configuration );

         var calibration = realm.Find<Calibration>( Calibration.PrimaryKey );

         if( calibration == null )
         {
            realm.Write( ( ) => {
               Console.WriteLine( "PK - Creating new Calibration table." );
               var newCalibration = new Calibration {
                  ID = Calibration.PrimaryKey
               };

               realm.Add( newCalibration );
            } );

            return false;
         }

         return calibration.IsCalibrated;
      }
   }
}
