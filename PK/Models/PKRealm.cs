using System;
using Realms;
using Xamarin.Essentials;

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
   }
}
