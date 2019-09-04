using PK.Helpers;

namespace PK.Interfaces
{
   public interface IBluetoothLEAdvertisement
   {
      void ReceivedPKAdvertisement( Anchor anchor, int RSSI );
   }
}
