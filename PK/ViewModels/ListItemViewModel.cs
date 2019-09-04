using System;

namespace PK.ViewModels
{
   public class ListItemViewModel
   {
      public static class Type
      {
         public const int PhoneKey = 1;
         public const int Account = 2;
         public const int Location = 3;
         public const int ShareKey = 4;
         public const int CalibrateDevice = 5;
         public const int ConfigureZones = 6;
         public static int Notification = 7;
      }

      public enum Operation { Navigate, Inform };

      public event EventHandler ItemSelected;

      public int ListType { get; set; }
      public string Title { get; set; }
      public string SubTitle { get; set; }
      public bool LeftImageHidden { get; set; }
      public bool RightImageHidden { get; set; }

      public void Selected( ) => ItemSelected?.Invoke( this, new EventArgs( ) );
   }
}
