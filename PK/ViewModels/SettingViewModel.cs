using System;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace PK.ViewModels
{
   public interface ISettingViewModel
   {

   }

   public class SettingViewModel
   {
      public string VersionAndBuild => $"Version: {VersionTracking.CurrentVersion} (Build: {VersionTracking.CurrentBuild})";

      public List<ListItemViewModel> ListItems { get; private set; }

      private readonly ISettingViewModel viewModel;

      public SettingViewModel( ISettingViewModel viewModel )
      {
         this.viewModel = viewModel;

         SetupSettingListItems( );
      }

      private void SetupSettingListItems( )
      {
         ListItems = new List<ListItemViewModel> {
            new ListItemViewModel {
               ListType = ListItemViewModel.Type.CalibrateDevice,
               Title = "Calibrate Device",
               SubTitle = "Re-calibrate this device with your vehicle."
            },
            new ListItemViewModel {
               ListType = ListItemViewModel.Type.ConfigureZones,
               Title = "Configure Vehicle Zones"
            },
            new ListItemViewModel {
               ListType = ListItemViewModel.Type.Notification,
               Title = "Notifications"
            }
         };

         foreach( var item in ListItems )
         {
            item.LeftImageHidden = true;
            item.ItemSelected += HandleListItemSelected;
         }
      }

      private void HandleListItemSelected( object sender, EventArgs e )
      {
         
      }
   }
}
