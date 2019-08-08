using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PK.ViewModels;

namespace PK.Droid.Adapters
{
   public class OptionListRecyclerViewAdapter : RecyclerView.Adapter
   {
      private readonly OptionListViewModel[ ] optionList;

      public OptionListRecyclerViewAdapter( OptionListViewModel[ ] optionList )
      {
         this.optionList = optionList;
      }

      public override int ItemCount => optionList.Length;

      public override RecyclerView.ViewHolder OnCreateViewHolder( ViewGroup parent, int viewType )
      {
         var layoutItemView = LayoutInflater.From( context: parent.Context ).Inflate( Resource.Layout.Item_OptionList, root: parent, attachToRoot: false );
         return new OptionListViewHolder( layoutItemView );
      }

      public override void OnBindViewHolder( RecyclerView.ViewHolder holder, int position )
      {
         var optionListViewHolder = holder as OptionListViewHolder;
         optionListViewHolder.OptionListViewModel = optionList[ position ];
         optionListViewHolder.ItemView.Click += ( sender, e ) => {
            optionList[ position ].Selected( );
         };
      }
   }

   public class OptionListViewHolder : RecyclerView.ViewHolder
   {
      private OptionListViewModel optionListViewModel;
      public OptionListViewModel OptionListViewModel
      {
         get => optionListViewModel;
         set
         {
            optionListViewModel = value;

            switch( optionListViewModel.Option )
            {
               case OptionListViewModel.Camera:
                  iconImageView.SetImageResource( Resource.Drawable.AugmentedReality );
                  break;
               case OptionListViewModel.Manual:
                  iconImageView.SetImageResource( Resource.Drawable.ShoePrint );
                  break;
            }

            titleTextView.Text = optionListViewModel.Text;
         }
      }

      private readonly ImageView iconImageView;
      private readonly TextView titleTextView;

      public OptionListViewHolder( View itemView ) : base( itemView )
      {
         iconImageView = itemView.FindViewById<ImageView>( Resource.Id.item_option_list_imageview_icon );
         titleTextView = itemView.FindViewById<TextView>( Resource.Id.item_option_list_textview_title );
      }
   }
}
