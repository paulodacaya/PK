using System;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;

namespace PK.iOS.Controllers
{
   public class ConfigureZonesController : UIViewController, IConfigureZonesViewModel
   {
      public ConfigureZonesController( )
      {
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupView( );
         SetupNavigation( );
      }

      private void SetupView( )
      {
         View.BackgroundColor = Colors.White;
      }

      private void SetupNavigation( )
      {
         
      }
   }
}
