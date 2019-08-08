using System;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PK.Droid.Activities;
using PK.Droid.Adapters;
using PK.ViewModels;

namespace PK.Droid.Fragments
{
   public class HomeFragment : Fragment, IHomeViewModel
   {
      private readonly HomeViewModel viewModel;

      // UI Elements
      private RootActivity rootActivity;
      private View homeLayout;
      private HomeRecyclerViewAdapter recyclerViewAdapter;

      public HomeFragment( )
      {
         viewModel = new HomeViewModel( viewModel: this );
      }

      public override void OnCreate( Bundle savedInstanceState )
      {
         base.OnCreate( savedInstanceState );

         rootActivity = Activity as RootActivity;
      }

      public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
      {
         rootActivity.toolbarTitle.Text = viewModel.Title;
         rootActivity.ShowToolbarSettings( );

         if( rootActivity.DidPresentCalibrationScreen )
            rootActivity.ExitCalibrationAnimation( );

         homeLayout = inflater.Inflate( Resource.Layout.Fragment_Home, root: container, attachToRoot: false );

         SetupRecyclerView( );

         return homeLayout;
      }

      private void SetupRecyclerView( )
      {
         var homeRecyclerView = homeLayout.FindViewById<RecyclerView>( Resource.Id.home_recyclerview );

         var homeConstraintLayout = homeLayout.FindViewById<ConstraintLayout>( Resource.Id.home_constraintlayout );
         homeConstraintLayout.ViewTreeObserver.GlobalLayout += (object sender, EventArgs e) => {
            homeRecyclerView.SetPadding( 0, homeConstraintLayout.MeasuredHeight / 2, 0, 0 );
         };

         var linearLayoutManager = new LinearLayoutManager( context: Activity );
         homeRecyclerView.SetLayoutManager( linearLayoutManager );

         recyclerViewAdapter = new HomeRecyclerViewAdapter( viewModel );
         homeRecyclerView.SetAdapter( recyclerViewAdapter );
      }

      void IHomeViewModel.NavigateToCalibrateScreen( )
      {
         rootActivity.EnterCalibrationAnimation( );

         var calibrateFragment = new CalibrateFragment( );
         rootActivity.ReplaceFragment( calibrateFragment );
      }

      void IHomeViewModel.NotifyLockChanged( )
      {
         recyclerViewAdapter.NotifyItemChanged( position: 0 );
      }
   }
}
