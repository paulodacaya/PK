using System;

namespace PK.Common
{
   public static class Strings
   {
      public static string BluetoothTurnedOnText = "Bluetooth turned on";
      public static string BluetoothRationaleText = "This application needs Bluetooth to operate.";
      public static string BluetoothTurnOnText => "Please enable Bluetooth on your device before preceding.";

      public static string NoBluetoothSuportText => "Your device does not support Bluetooth. This application can no longer be used on your device.";
      public static string NoBluetoothScanningSupportText => "Your device does not support Bluetooth scanning.";

      public static string Understood => "UNDERSTOOD";
      public static string GotIt => "GOT IT";
      public static string UhOh => "UH OH!";
   }
}
