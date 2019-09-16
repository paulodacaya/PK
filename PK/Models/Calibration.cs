using Realms;

namespace PK.Models
{
   public class Calibration : RealmObject
   {
      [PrimaryKey]
      public string DeviceModel { get; set; }
      public int RSSI_0_5_m { get; set; }
      public int RSSI_1_0_m { get; set; }
      public int RSSI_1_5_m { get; set; }
      public int RSSI_2_0_m { get; set; }
      public int RSSI_2_5_m { get; set; }
      public int RSSI_3_0_m { get; set; }

      public bool IsCalibrated => RSSI_0_5_m < 0 && RSSI_1_0_m < 0 && RSSI_1_5_m < 0
         && RSSI_2_0_m < 0 && RSSI_2_5_m < 0 && RSSI_3_0_m < 0;
   }
}
