using Realms;

namespace PK.Models
{
   public class Zone : RealmObject
   {
      public const string Location_Passenger = "Passager";
      public const string Location_Driver = "Driver";
      public const string Location_Boot = "Boot";
      public const string Location_Welcome = "Welcome";

      [PrimaryKey]
      public string ZoneLocation { get; set; }
      public double Radius { get; set; }
   }
}
