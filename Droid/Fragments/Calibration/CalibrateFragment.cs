using System;
using Android;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using PK.Droid.Activities;
using PK.Droid.Adapters;
using PK.Droid.Components;
using PK.ViewModels;

namespace PK.Droid.Fragments
{
   public class CalibrateFragment : Fragment, ICalibrateViewModel
   {
      public readonly CalibrateViewModel viewModel;

      // UI Elements
      private View calibrateLayout;
      private RootActivity rootActivity;
      private ViewPager viewPager;

      public CalibrateFragment( )
      {
         viewModel = new CalibrateViewModel( this );
      }

      public override void OnCreate( Bundle savedInstanceState )
      {
         base.OnCreate( savedInstanceState );

         rootActivity = Activity as RootActivity;
      }

      public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
      {
         rootActivity.toolbarTitle.Text = viewModel.Title;
         rootActivity.ShowToolbarBack( );

         calibrateLayout = inflater.Inflate( Resource.Layout.Fragment_Calibrate, root: container, attachToRoot: false );

         SetupViewPager( );

         return calibrateLayout;
      }

      private void SetupViewPager( )
      {
         var pageOneFragment = new CalibratePageOneFragment( viewModel );
         var pageTwoFragment = new CalibratePageTwoFragment( viewModel );

         viewPager = calibrateLayout.FindViewById<ViewPager>( Resource.Id.calibrate_viewpager );
         viewPager.Adapter = new CalibrateSlidePagerAdapter( ChildFragmentManager, new Fragment[ ] { pageOneFragment, pageTwoFragment } );

         var tabLayout = calibrateLayout.FindViewById<TabLayout>( Resource.Id.calibrate_tablayout_dots );
         tabLayout.SetupWithViewPager( viewPager );
      }

      bool ICalibrateViewModel.VerifyCameraPermission( )
      {
         // Check camera permissions
         if( Android.App.Application.Context.CheckSelfPermission( Manifest.Permission.Camera ) != Permission.Granted )
         {
            Console.WriteLine( "Android - Requesting camera permission." );
            PKApplication.RequestCameraPermission( Activity );
            return false;
         }

         return true;
      }

      void ICalibrateViewModel.NavigateToPageTwo( ) => viewPager.SetCurrentItem( viewPager.CurrentItem + 1, smoothScroll: true );

      void ICalibrateViewModel.NavigateToCameraCalibration( ) => rootActivity.NavigateToCameraCalibration( );

      void ICalibrateViewModel.NavigateToManualCalibration( )
      {
      }

      void ICalibrateViewModel.NavigateToConfigureZones( )
      {
      }

      
   }
}
