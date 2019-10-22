using System;
using System.Linq;
using Foundation;
using PK.iOS.Helpers;
using PK.iOS.Views;
using PK.ViewModels;
using UIKit;

namespace PK.iOS.Controllers
{
   public class HomeController : UITableViewController, IHomeViewModel
   {
      private readonly HomeViewModel viewModel;

      // UI Elements
      private HomeHeaderView homeHeaderView;

      public override UIStatusBarStyle PreferredStatusBarStyle( ) => UIStatusBarStyle.LightContent;

      public HomeController( )
      {
         viewModel = new HomeViewModel( this );
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNavigation( );
         SetupTableView( );
      }

      private void SetupNavigation( )
      {
         NavigationItem.SetHidesBackButton( true, animated: false );
      }

      private void SetupTableView( )
      {
         TableView.BackgroundView = Components.UIImageView( Images.BoschSuperGraphicBackground, null, UIViewContentMode.ScaleAspectFill );
         TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
         TableView.RegisterClassForCellReuse( typeof( CardCell ), CardCell.CellId );
         TableView.RegisterClassForCellReuse( typeof( ExpandableCardCell ), ExpandableCardCell.CellId );
      }

      public override nint NumberOfSections( UITableView tableView ) => viewModel.CardItemViewModels.Count;

      public override UIView GetViewForHeader( UITableView tableView, nint section )
      {
         if( section == 0 )
         {
            homeHeaderView = new HomeHeaderView( );
            homeHeaderView.GreetingLabel.Text = viewModel.GreetingMessage;
            homeHeaderView.VehicleInfoLabel.Text = viewModel.VehicleMessage;
            homeHeaderView.VehicleStateLabel.Text = "State: Locked";
            homeHeaderView.VehicleStateImageView.Image = Images.PadlockLock;

            return homeHeaderView;
         }

         return new UIView( );
      }

      public override nfloat GetHeightForHeader( UITableView tableView, nint section )
      {
         if( section == 0 )
            return UITableView.AutomaticDimension;

         return 12;
      }

      public override nint RowsInSection( UITableView tableView, nint section ) => 1;

      public override UITableViewCell GetCell( UITableView tableView, NSIndexPath indexPath )
      {
         var cardItemViewModel = viewModel.CardItemViewModels[ indexPath.Section ];

         IBindViewModel<CardItemViewModel> cell;

         if( cardItemViewModel.Type == CardItemViewModel.c_type_expandable )
            cell = tableView.DequeueReusableCell( ExpandableCardCell.CellId, indexPath ) as IBindViewModel<CardItemViewModel>;
         else
            cell = tableView.DequeueReusableCell( CardCell.CellId, indexPath ) as IBindViewModel<CardItemViewModel>;

         cell.SetViewModel( viewModel.CardItemViewModels[ indexPath.Section ] );

         return ( UITableViewCell )cell;
      }

      public override void RowSelected( UITableView tableView, NSIndexPath indexPath )
      {
         var cardItemViewModel = viewModel.CardItemViewModels[ indexPath.Section ];
         cardItemViewModel.ActionSelected( );
      }

      void IHomeViewModel.UpdateRssi( int rssi )
      {
         InvokeOnMainThread( ( ) => {
            if( homeHeaderView != null )
            {
               homeHeaderView.RssiLabel.Text = rssi.ToString( );
            }
         } );
      }

      void IHomeViewModel.NotifyDeviceInZoneStateChanged( bool isInZone )
      {
         var exapandableCell = TableView.VisibleCells.SingleOrDefault( c => c is ExpandableCardCell ) as ExpandableCardCell;

         InvokeOnMainThread( ( ) => {
            if( isInZone )
            {
               if( homeHeaderView != null )
               {
                  homeHeaderView.VehicleStateLabel.Text = "State: Unlock";
                  homeHeaderView.VehicleStateImageView.Image = Images.PadlockUnlock;
               }

               if( exapandableCell != null )
                  exapandableCell.VisualImageView.Image = Images.PK5;
            }
            else
            {
               if( homeHeaderView != null )
               {
                  homeHeaderView.VehicleStateLabel.Text = "State: Locked";
                  homeHeaderView.VehicleStateImageView.Image = Images.PadlockLock;
               }

               if( exapandableCell != null )
                  exapandableCell.VisualImageView.Image = Images.PK4;
            }
         } );
      }

      void IHomeViewModel.ShowRecalibratePrompt( string title, string message, string cancelText, string actionText )
      {
         var actionDialogController = new ActionDialogController( title, message );

         actionDialogController.AddAction( cancelText, null );
         actionDialogController.AddAction( actionText, viewModel.ActionRecalibrateSelected );

         PresentViewController( actionDialogController, animated: true, completionHandler: null );
      }

      void IHomeViewModel.ShowDeleteDataPrompt( string title, string message, string cancelText, string actionText )
      {
         var actionDialogController = new ActionDialogController( title, message );

         actionDialogController.AddAction( cancelText, null );
         actionDialogController.AddAction( actionText, viewModel.ActionDeleteDatabase );

         PresentViewController( actionDialogController, animated: true, completionHandler: null );
      }

      void IHomeViewModel.PresentCameraRecalibration( )
      {
         var cameraCalibrationController = new CameraCalibrationController( isRecalibrating: true );
         PresentViewController( cameraCalibrationController, animated: true, completionHandler: null );
      }

      void IHomeViewModel.ReloadRowAt( int row )
      {
         TableView.ReloadRows( new[ ] { NSIndexPath.FromRowSection( 0, row ) }, UITableViewRowAnimation.Fade );
      }

      void IHomeViewModel.NavigateToOnBoarding( )
      {
         if( NavigationController.ViewControllers[ 0 ] is CalibrateOnBoardingController == false )
            NavigationController.SetViewControllers( controllers: new UIViewController[ ] {
               new CalibrateOnBoardingController( ),
               this
            }, animated: false );

         NavigationController.PopToRootViewController( animated: true );
      }
   }

   public interface IBindViewModel<T>
   {
      void SetViewModel( T ViewModel );
   }
}
