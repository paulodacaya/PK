using System;
using System.Collections.Generic;
using System.Linq;
using Realms;

namespace PK.Models
{
   public static class AnchorLocation
   {
      public static int Master = 1;
      public static int DriverDoor = 2;
      public static int PassangerDoor = 3;
   }

   public class CalibrationData : RealmObject
   {
      [PrimaryKey]
      public int CalibrationID { get; set; }
      public IList<CalibrationLocationData> LocationsData { get; }

      public bool Completed 
      {
         get
         {
            foreach( var locationData in LocationsData )
            {
               if( !locationData.Completed )
                  return false;
            }

            return true;
         } 
      }
   }

   public class CalibrationLocationData : RealmObject
   {
      [PrimaryKey]
      public int LocationID { get; set; }
      public IList<CalibrationDistanceData> DistancesData { get; }

      public bool Completed
      {
         get
         {
            foreach( var distanceData in DistancesData )
            {
               if( !distanceData.Completed )
                  return false;
            }

            return true;
         }
      }
   }

   public class CalibrationDistanceData : RealmObject
   {
      public int Distance { get; set; }
      public int MinRSSI { get; set; }
      public int MaxRSSI { get; set; }
      public bool Completed { get; set; }
   }
}
