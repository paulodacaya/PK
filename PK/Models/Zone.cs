using Realms;

namespace PK.Models
{
   public class Zone : RealmObject
   {
      [PrimaryKey]
      public int ZoneID { get; set; }
      public double Distance { get; set; }
   }
}
