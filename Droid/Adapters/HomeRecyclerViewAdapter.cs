using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PK.ViewModels;

namespace PK.Droid.Adapters
{
   public class HomeRecyclerViewAdapter : RecyclerView.Adapter
   {
      private readonly HomeViewModel viewModel;

      public HomeRecyclerViewAdapter( HomeViewModel viewModel )
      {
         this.viewModel = viewModel;
      }

      public override int ItemCount => viewModel.ListItems.Count + 1;

      public override int GetItemViewType( int position ) => position;

      public override RecyclerView.ViewHolder OnCreateViewHolder( ViewGroup parent, int viewType )
      {
         // viewType == position

         if( viewType == 0 )
         {
            var layoutItemView = LayoutInflater.From( context: parent.Context ).Inflate( Resource.Layout.Item_HomeUnlockLockButton, root: parent, attachToRoot: false );
            return new HomeUnlockLockHolder( layoutItemView );
         }
         else
         {
            var layoutItemView = LayoutInflater.From( context: parent.Context ).Inflate( Resource.Layout.Item_HomeList, root: parent, attachToRoot: false );
            return new HomeListViewHolder( layoutItemView );
         }
      }

      public override void OnBindViewHolder( RecyclerView.ViewHolder holder, int position )
      {
         if( position == 0 )
         {
            var homeUnlockLockViewHolder = holder as HomeUnlockLockHolder;
            homeUnlockLockViewHolder.IsLocked = viewModel.IsLocked;
            homeUnlockLockViewHolder.OnLockUnlockViewClick += ( sender, e ) => viewModel.LockSelected( );
         }
         else
         {
            var homeListViewHolder = holder as HomeListViewHolder;
            homeListViewHolder.ListItemViewModel = viewModel.ListItems[ position - 1 ];
         }
      }
   }

   public class HomeUnlockLockHolder : RecyclerView.ViewHolder
   {
      public EventHandler OnLockUnlockViewClick;

      private bool isLocked;
      public bool IsLocked
      {
         get => isLocked;
         set
         {
            isLocked = value;

            UpdateLock( );
         }
      }

      private readonly ImageView lockUnlockIconImageView;
      private readonly TextView lockUnlockTextView;
      private readonly View lockUnlockView;

      public HomeUnlockLockHolder( View itemView ) : base( itemView )
      {
         lockUnlockIconImageView = itemView.FindViewById<ImageView>( Resource.Id.item_home_unlock_lock_imageview_icon );
         lockUnlockTextView = itemView.FindViewById<TextView>( Resource.Id.item_home_unlock_lock_textview );
         lockUnlockView = itemView.FindViewById<View>( Resource.Id.item_home_unlock_lock_view );
         lockUnlockView.Click += LockUnlockViewClick;

         UpdateLock( );
      }

      private void LockUnlockViewClick( object sender, EventArgs e ) => OnLockUnlockViewClick?.Invoke( sender, e );

      private void UpdateLock( )
      {
         if( isLocked )
         {
            lockUnlockIconImageView.SetImageResource( Resource.Drawable.PadlockLock );
            lockUnlockTextView.Text = "Locked";
         }
         else
         {
            lockUnlockIconImageView.SetImageResource( Resource.Drawable.PadlockUnlock );
            lockUnlockTextView.Text = "Unlocked";
         }
      }
   }

   public class HomeListViewHolder : RecyclerView.ViewHolder
   {
      private ListItemViewModel listItemViewModel;
      public ListItemViewModel ListItemViewModel
      {
         get => listItemViewModel;
         set
         {
            listItemViewModel = value;

            switch( listItemViewModel.ListType )
            {
               case ListItemViewModel.Type.PhoneKey:
                  iconImageView.SetImageResource( Resource.Drawable.CellPhoneKey );
                  break;
               case ListItemViewModel.Type.Account:
                  iconImageView.SetImageResource( Resource.Drawable.Account );
                  break;
               case ListItemViewModel.Type.Calibrate:
                  iconImageView.SetImageResource( Resource.Drawable.CellPhoneWireless );
                  break;
               case ListItemViewModel.Type.Location:
                  iconImageView.SetImageResource( Resource.Drawable.Navigation );
                  break;
               case ListItemViewModel.Type.ShareKey:
                  iconImageView.SetImageResource( Resource.Drawable.KeyVarient );
                  break;
            }

            titleTextView.Text = listItemViewModel.Title;

            subTitleTextView.Text = listItemViewModel.SubTitle;
            subTitleTextView.Visibility = string.IsNullOrEmpty( listItemViewModel.SubTitle ) ? ViewStates.Gone : ViewStates.Visible;

            switch( listItemViewModel.ListOperation )
            {
               case ListItemViewModel.Operation.Inform:
                  actionImageView.SetImageResource( Resource.Drawable.InformationOutline );
                  break;
               case ListItemViewModel.Operation.Navigate:
                  actionImageView.SetImageResource( Resource.Drawable.ChevronRight );
                  break;
            }
         }
      }

      private readonly ImageView iconImageView;
      private readonly TextView titleTextView;
      private readonly TextView subTitleTextView;
      private readonly ImageView actionImageView;

      public HomeListViewHolder( View itemView ) : base( itemView )
      {
         iconImageView = itemView.FindViewById<ImageView>( Resource.Id.item_home_list_imageview_icon );
         titleTextView = itemView.FindViewById<TextView>( Resource.Id.item_home_list_textview_title );
         subTitleTextView = itemView.FindViewById<TextView>( Resource.Id.item_home_list_textview_subtitle );
         actionImageView = itemView.FindViewById<ImageView>( Resource.Id.item_home_list_imageview_action );

         itemView.Click += ItemViewClick;
      }

      private void ItemViewClick( object sender, EventArgs e )
      {
         listItemViewModel.Selected( );
      }
   }
}