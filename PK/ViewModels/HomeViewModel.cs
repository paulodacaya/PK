using System;
using System.Collections.Generic;

namespace PK.ViewModels
{
   public interface IHomeViewModel
   {
      void NotifyLockChanged( );
   }

   public class HomeViewModel
   {
      public string Title { get; private set; }
      public bool IsLocked { get; private set; }

      private readonly IHomeViewModel viewModel;

      public List<ListItemViewModel> ListItems { get; private set; }

      public HomeViewModel( IHomeViewModel viewModel )
      {
         this.viewModel = viewModel;

         SetupListItemViewModels( );

         Title = "My Mazda 3";
      }

      private void SetupListItemViewModels( )
      {
         var phoneKey = new ListItemViewModel {
            ListType = ListItemViewModel.Type.PhoneKey,
            Title = "DIGITAL PHONE KEY",
            SubTitle = "Connected"
         };
         phoneKey.ItemSelected += HandleListItemSelected;

         var control = new ListItemViewModel {
            ListType = ListItemViewModel.Type.Account,
            Title = "ACCOUNT",
            SubTitle = "Mr. Paulo Dacaya"
         };
         control.ItemSelected += HandleListItemSelected;

         var location = new ListItemViewModel {
            ListType = ListItemViewModel.Type.Location,
            Title = "LOCATION",
         };
         location.ItemSelected += HandleListItemSelected;

         var shareKey = new ListItemViewModel {
            ListType = ListItemViewModel.Type.ShareKey,
            Title = "SHARE KEY",
         };
         shareKey.ItemSelected += HandleListItemSelected;

         ListItems = new List<ListItemViewModel> {
            phoneKey,
            control,
            location,
            shareKey
         };
      }

      public void LockSelected( )
      {
         IsLocked = !IsLocked;
         viewModel.NotifyLockChanged( );
      }

      private void HandleListItemSelected( object sender, EventArgs e )
      {
      }
   }
}
