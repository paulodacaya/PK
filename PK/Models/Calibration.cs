using Realms;

namespace PK.Models
{
   public class Calibration : RealmObject
   {
      // There can only be a single calibration table with single row per device. Use the ID of 0.
      public const int PrimaryKey = 0;

      [PrimaryKey]
      public int ID { get; set; }
      public double MinKConstant { get; set; }
      public double MaxKConstant { get; set; }
      public int MinRSSI { get; set; }
      public int MaxRSSI { get; set; }

      public bool IsCalibrated => MinKConstant > default( double ) && MaxKConstant > default( double );
   }


   /// <summary>
   /// When mutliple achors are introduced. We will need 
   /// </summary>
   public class Node : RealmObject
   {
      [PrimaryKey]
      public int ID { get; set; }
   }
}
