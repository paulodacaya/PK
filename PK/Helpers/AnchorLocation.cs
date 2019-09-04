using System;
using System.Linq;

namespace PK.Helpers
{
   public enum Anchor { Central, DriverDoor, PassangerDoor, BootDoor }

   public static class AnchorHelper
   {
      private const string CENTRAL_NAME = "PK CENTRAL NODE";
      private const string DRIVER_NAME = "PK DRIVER NODE";
      private const string PASSANGER_NAME = "PK PASSANGER NODE";
      private const string BOOT_NAME = "PK BOOT NODE";

      private static string[ ] Names = { CENTRAL_NAME, DRIVER_NAME, PASSANGER_NAME, BOOT_NAME };

      public static bool IsPKAnchor( string advertismentName )
      {
         return Names.Any( n => n.Equals( advertismentName ) );
      }

      public static Anchor GetAnchorType( string advertisementName )
      {
         return ( Anchor )Array.IndexOf( Names, advertisementName );
      }

      public static string GetAnchorName( Anchor anchor )
      {
         return Names[ ( int )anchor ];
      }
   }
}
