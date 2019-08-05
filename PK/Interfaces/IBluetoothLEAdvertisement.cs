using System;

namespace PK.Interfaces
{
   public interface IBluetoothLEAdvertisement
   {
      void ReceivedAdvertisement( int anchorLocationID, int RSSI );
   }
}
