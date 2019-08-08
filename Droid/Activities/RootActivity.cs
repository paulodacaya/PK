using System;
using Android;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.Transitions;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using PK.Common;
using PK.Droid.Bluetooth;
using PK.Droid.Components;
using PK.Droid.Fragments;
using PK.Interfaces;

namespace PK.Droid.Activities
{
   [Activity( Label = "PK", MainLauncher = true, Icon = "@mipmap/perfectlykeylessappicon", RoundIcon = "@mipmap/perfectlykeylessappicon_round", ScreenOrientation = ScreenOrientation.Portrait )]
   public class RootActivity : Android.Support.V7.App.AppCompatActivity, IBluetoothLEState
   {
      public bool DidPresentCalibrationScreen;

      // UI elements
      public TextView toolbarTitle;
      private ConstraintLayout constraintLayout;
      private PKSnackBar pkSnackBar;
      private ImageView toolbarSettingsImageView;
      private ImageView toolbarLeftImageView;

      private ConstraintSet constraintSet1;
      private ConstraintSet constraintSet2;
      private View staticRadialView;
      private View pulseRadialView;
      private ImageView isomectricMaleImageView;
      private Animator animatorSet;

      public RootActivity( )
      {
         AndroidBluetoothLE.Instance.StateDelegate = this;
      }

      protected override void OnCreate( Bundle savedInstanceState )
      {
         base.OnCreate( savedInstanceState );
         SetContentView( Resource.Layout.Activity_Root );

         SetupRadialBackground( );
         SetupActionBar( );
         SetupHomeFragment( );
         SetupAnimations( );
      }

      protected override void OnResume( )
      {
         base.OnResume( );

         //AndroidBluetoothLE.Instance.ScanForAdvertisements( );
      }

      private void SetupRadialBackground( )
      {
         constraintLayout = FindViewById<ConstraintLayout>( Resource.Id.root_constraintLayout_container );
         var radialBackgroundDrawable = constraintLayout.Background as GradientDrawable;

         var displayMetrics = new DisplayMetrics( );
         WindowManager.DefaultDisplay.GetMetrics( displayMetrics );

         radialBackgroundDrawable.SetGradientRadius( displayMetrics.WidthPixels / 2 );
      }

      private void SetupActionBar( )
      {
         var toolBar = FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.root_content_toolbar );
         toolbarTitle = toolBar.FindViewById<TextView>( Resource.Id.content_toolbar_textview_title );

         SetSupportActionBar( toolBar );
         SupportActionBar.SetDisplayShowTitleEnabled( false );

         var toolbarSettingContainer = toolBar.FindViewById<FrameLayout>( Resource.Id.content_toolbar_left_imageview_container );
         toolbarSettingContainer.Click += HandleToolbarLeftClick;

         toolbarSettingsImageView = toolBar.FindViewById<ImageView>( Resource.Id.content_toolbar_left_imageview_setting );
         toolbarLeftImageView = toolBar.FindViewById<ImageView>( Resource.Id.content_toolbar_left_imageview_left );
      }

      private void SetupHomeFragment( )
      {
         var fragmentTransation = SupportFragmentManager.BeginTransaction( );
         var homeFragment = new HomeFragment( );
         fragmentTransation.Add( containerViewId: Resource.Id.root_framelayout_fragment_container, homeFragment );
         fragmentTransation.Commit( );
      }

      private void SetupAnimations( )
      {
         constraintSet1 = new ConstraintSet( );
         constraintSet1.Clone( constraintLayout );
         constraintSet2 = new ConstraintSet( );
         constraintSet2.Clone( context: this, Resource.Layout.Activity_Root_Alt );

         staticRadialView = FindViewById<View>( Resource.Id.root_radial_view_static );
         staticRadialView.Visibility = ViewStates.Invisible;

         pulseRadialView = FindViewById<View>( Resource.Id.root_radial_view_pulse );
         pulseRadialView.Visibility = ViewStates.Invisible;

         isomectricMaleImageView = FindViewById<ImageView>( Resource.Id.root_imageview_iso_male );
         isomectricMaleImageView.Visibility = ViewStates.Invisible;

         animatorSet = AnimatorInflater.LoadAnimator( context: this, Resource.Animation.Pulse_Infinite );
         animatorSet.SetTarget( pulseRadialView );
      }

      public override void OnRequestPermissionsResult( int requestCode, string[ ] permissions, [GeneratedEnum] Permission[ ] grantResults )
      {
         if( requestCode == PKApplication.REQUESTCODE_LOCATION_ID )
         {
            if( grantResults[ 0 ] == Permission.Granted )
               AndroidBluetoothLE.Instance.ScanForAdvertisements( );
            else
               NotifyLocationPermissionRationale( );
         }
         else if( requestCode == PKApplication.REQUESTCODE_CAMERA_ID )
         {
            if( grantResults[ 0 ] == Permission.Granted )
               NavigateToCameraCalibration( );
         }
      }

      protected override void OnActivityResult( int requestCode, [GeneratedEnum] Result resultCode, Intent data )
      {
         if( resultCode != Result.Ok )
            return;
      }

      void IBluetoothLEState.NotifyBluetoothIsOff( ) => PKToast.MakeText( this, Resource.Drawable.Bluetooth, Strings.BluetoothTurnedOnText, ToastLength.Long ).Show( );

      void IBluetoothLEState.NotifyBluetoothNotSupported( )
      {
         var dialog = new MessageDialogFragment {
            Title = Strings.UhOh,
            Message = Strings.NoBluetoothSuportText,
            NegativeButtonText = string.Empty,
            PostiveButtonText = Strings.Understood,
         };

         dialog.Show( SupportFragmentManager, "no_bluetooth_support" );
      }

      void IBluetoothLEState.NotifyBluetoothIsOn( )
      {
         // Do nothing - A toast is shown when bluetooth is turned off. Bluetooth is then automatically turned on.
      }

      bool IBluetoothLEState.VerifyLocationPermission( )
      {
         if( Application.Context.CheckSelfPermission( Manifest.Permission.AccessFineLocation ) != Permission.Granted )
         {
            if( ShouldShowRequestPermissionRationale( Manifest.Permission.AccessFineLocation ) )
            {
               Console.WriteLine( "Android - User has denied location permission previously. Requesting Rationale." );
               NotifyLocationPermissionRationale( ); 
               return false;
            }

            Console.WriteLine( "Android - Requestion location permission." );
            PKApplication.RequestLocationPermission( this );
            return false;
         }

         return true;
      }

      #region Event Handlers
      private void HandleToolbarLeftClick( object sender, EventArgs e )
      {
         if( toolbarSettingsImageView.Visibility == ViewStates.Visible )
         {
            // TODO Navigate to settings activity.
         }
         else if( toolbarLeftImageView.Visibility == ViewStates.Visible )
         {
            OnBackPressed( );
         }
      }

      private void HandleSnackBarActionClick( object sender, EventArgs e ) => PKApplication.RequestLocationPermission( this );
      #endregion

      public void NotifyLocationPermissionRationale( )
      {
         if( pkSnackBar == null )
         {
            pkSnackBar = new PKSnackBar( this, constraintLayout, Strings.BluetoothRationaleText, Strings.GotIt );
            pkSnackBar.OnActionClick += HandleSnackBarActionClick;
         }

         pkSnackBar.Show( );
      }

      public void ShowToolbarBack( )
      {
         toolbarSettingsImageView.Visibility = ViewStates.Gone;
         toolbarLeftImageView.Visibility = ViewStates.Visible;
      }

      public void ShowToolbarSettings( )
      {
         toolbarSettingsImageView.Visibility = ViewStates.Visible;
         toolbarLeftImageView.Visibility = ViewStates.Gone;
      }

      public void ReplaceFragment( Android.Support.V4.App.Fragment fragment )
      {
         var fragmentTransation = SupportFragmentManager.BeginTransaction( );

         if( !fragment.IsAdded )
         {
            fragmentTransation.SetCustomAnimations( 
               enter: Resource.Animation.Enter_From_Right, 
               exit: Resource.Animation.Exit_To_Left,
               popEnter: Resource.Animation.Enter_From_Left,
               popExit: Resource.Animation.Exit_To_Right
            );
            fragmentTransation.Replace( containerViewId: Resource.Id.root_framelayout_fragment_container, fragment );
            fragmentTransation.AddToBackStack( null );
            fragmentTransation.Commit( );
         }
         else
         {
            fragmentTransation.Show( fragment );
         }
      }

      public void EnterCalibrationAnimation( )
      {
         // before ~ Activity_Root.xml

         PerformTransition( );
         animatorSet.Start( );

         // after ~ Activity_Root_Alt.xml

         DidPresentCalibrationScreen = true;
      }

      public void ExitCalibrationAnimation( )
      {
         // before ~ Activity_Root_Alt.xml
         staticRadialView.Visibility = ViewStates.Invisible;
         pulseRadialView.Visibility = ViewStates.Invisible;
         isomectricMaleImageView.Visibility = ViewStates.Invisible;

         PerformTransition( );
         animatorSet.Cancel( );

         // after ~ Activity_Root.xml
         staticRadialView.Visibility = ViewStates.Invisible;
         pulseRadialView.Visibility = ViewStates.Invisible;
         isomectricMaleImageView.Visibility = ViewStates.Invisible;

         DidPresentCalibrationScreen = false;
      }

      private void PerformTransition( )
      {
         var transition = new AutoTransition( );
         transition.SetDuration( 250 );
         transition.SetInterpolator( new AccelerateDecelerateInterpolator( ) );

         TransitionManager.BeginDelayedTransition( sceneRoot: constraintLayout );

         var constraintSet = DidPresentCalibrationScreen ? constraintSet1 : constraintSet2;
         constraintSet.ApplyTo( constraintLayout );
      }

      public void NavigateToCameraCalibration( )
      {
         var cameraCalibrationActivityIntent = new Intent( packageContext: this, typeof( CameraCalibrationActivity ) );
         StartActivity( cameraCalibrationActivityIntent );
      }
   }
}
