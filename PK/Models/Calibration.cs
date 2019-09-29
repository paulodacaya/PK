using Realms;

namespace PK.Models
{
   public class Calibration : RealmObject
   {
      public const string KEY_DEVICE_MODEL = nameof( DeviceModel );
      public const string KEY_RSSI_ONE_METRE = nameof( Rssi_One_Metre );

      [PrimaryKey]
      public string DeviceModel { get; set; }
      public int Rssi_One_Metre { get; set; }

      public bool IsCalibrated => Rssi_One_Metre != default;
   }
}
