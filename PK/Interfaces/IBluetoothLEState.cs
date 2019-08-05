using System;

namespace PK.Interfaces
{
   public interface IBluetoothLEState
   {
      void NotifyBluetoothNotSupported( );
      void NotifyBluetoothIsOff( );
      void NotifyBluetoothIsOn( );

      bool VerifyLocationPermission( );
   }
}
