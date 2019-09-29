using System;

namespace PK.Interfaces
{
   public interface IBluetoothLEState
   {
      void NotifyBluetoothNotSupported( string title, string message );
      void NotifyBluetoothIsOff( );
      void NotifyBluetoothIsOn( );

      bool VerifyLocationPermission( );
   }
}
