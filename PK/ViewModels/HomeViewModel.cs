using System;
using System.Collections.Generic;

namespace PK.ViewModels
{
   public interface IHomeViewModel
   {
      void NavigateToCalibrateScreen( );
      void NotifyLockChanged( );
   }

   public class HomeViewModel
   {
      public string Title { get; private set; }
      public bool IsLocked { get; private set; }

      private readonly IHomeViewModel viewModel;

      public List<ListItemViewModel> ListItemViewModels { get; private set; }

      public HomeViewModel( IHomeViewModel viewModel )
      {
         this.viewModel = viewModel;

         SetupListItemViewModels( );

         Title = "My Mazda 3";
      }

      private void SetupListItemViewModels( )
      {
         var phoneKey = new ListItemViewModel {
            ListOperation = ListItemViewModel.Operation.Inform,
            ListType = ListItemViewModel.Type.PhoneKey,
            Title = "DIGITAL PHONE KEY",
            SubTitle = "Connected"
         };
         phoneKey.ListItemSelected += HandleListItemSelected;

         var control = new ListItemViewModel {
            ListType = ListItemViewModel.Type.Account,
            Title = "ACCOUNT",
            SubTitle = "Mr. Paulo Dacaya"
         };
         control.ListItemSelected += HandleListItemSelected;

         var calibrate = new ListItemViewModel {
            ListType = ListItemViewModel.Type.Calibrate,
            Title = "CALIBRATE",
         };
         calibrate.ListItemSelected += HandleListItemSelected;

         var location = new ListItemViewModel {
            ListType = ListItemViewModel.Type.Location,
            Title = "LOCATION",
         };
         location.ListItemSelected += HandleListItemSelected;

         var shareKey = new ListItemViewModel {
            ListType = ListItemViewModel.Type.ShareKey,
            Title = "SHARE KEY",
         };
         shareKey.ListItemSelected += HandleListItemSelected;

         ListItemViewModels = new List<ListItemViewModel> {
            phoneKey,
            control,
            calibrate,
            location,
            shareKey
         };
      }

      public void LockSelected( )
      {
         IsLocked = !IsLocked;
         viewModel.NotifyLockChanged( );
      }

      #region Event Handlers
      private void HandleListItemSelected( object sender, EventArgs e )
      {
         var listItemViewModel = sender as ListItemViewModel;

         if( listItemViewModel.ListOperation == ListItemViewModel.Operation.Navigate )
         {
            if( listItemViewModel.ListType == ListItemViewModel.Type.Calibrate )
            {
               viewModel.NavigateToCalibrateScreen( );
            }
         }
      }
      #endregion
   }

   public class ListItemViewModel
   {
      public event EventHandler ListItemSelected;

      public Operation ListOperation { get; set; }
      public int ListType { get; set; }
      public string Title { get; set; }
      public string SubTitle { get; set; }

      public void Selected( )
      {
         ListItemSelected?.Invoke( this, new EventArgs( ) );
      }

      public static class Type
      {
         public const int PhoneKey = 0;
         public const int Account = 2;
         public const int Calibrate = 3;
         public const int Location = 4;
         public const int ShareKey = 5;
      }

      public enum Operation { Navigate, Inform }; 
   }
}
