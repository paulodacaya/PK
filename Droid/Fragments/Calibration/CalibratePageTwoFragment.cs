using System;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PK.Droid.Adapters;
using PK.ViewModels;

namespace PK.Droid.Fragments
{
   public class CalibratePageTwoFragment : Fragment
   {
      private readonly CalibrateViewModel viewModel;

      public CalibratePageTwoFragment( CalibrateViewModel viewModel )
      {
         this.viewModel = viewModel;
      }

      public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
      {
         var pageTwoLayout = inflater.Inflate( Resource.Layout.Fragment_Calibrate_Page_Two, root: container, attachToRoot: false );

         var titleTextView = pageTwoLayout.FindViewById<TextView>( Resource.Id.calibrate_pagetwo_textview_title );
         titleTextView.Text = viewModel.PageTwoTitleText;

         var recyclerView = pageTwoLayout.FindViewById<RecyclerView>( Resource.Id.calibrate_pagetwo_recyclerview );

         var linearLayoutManager = new LinearLayoutManager( context: Context );
         recyclerView.SetLayoutManager( linearLayoutManager );

         var dividerItemDecoration = new DividerItemDecoration( context: Context, DividerItemDecoration.Vertical );
         dividerItemDecoration.SetDrawable( Context.GetDrawable( Resource.Drawable.Shape_Rectangle_Divider ) );
         recyclerView.AddItemDecoration( dividerItemDecoration );

         var recyclerViewAdapter = new OptionListRecyclerViewAdapter( viewModel.OptionList );
         recyclerView.SetAdapter( recyclerViewAdapter );

         return pageTwoLayout;
      }      
   }
}
