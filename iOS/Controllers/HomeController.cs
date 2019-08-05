using System;
using CoreGraphics;
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
      private readonly string cellID = "zoningListItemCellID";

      public HomeController( ) : base( withStyle: UITableViewStyle.Grouped )
      {
         viewModel = new HomeViewModel( viewModel: this );
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNavigation( );
         SetupTableView( );
      }

      public override void ViewWillAppear( bool animated )
      {
         base.ViewWillAppear( animated );

         ( NavigationController as RootNavigationController )?.ShowVehicle( );
      }

      private void SetupNavigation( )
      {
         NavigationItem.Title = viewModel.Title;

         var settingButton = new UIButton {
            TintColor = Colors.White,
         };
         settingButton.SetBackgroundImage( Images.Setting, UIControlState.Normal );
         settingButton.TouchUpInside += HandleSettingButtonTouchUpInside;
         settingButton.Anchor( size: new CGSize( 20, 20 ) );

         NavigationItem.LeftBarButtonItem = new UIBarButtonItem( customView: settingButton );
      }

      private void SetupTableView( )
      {
         TableView.RowHeight = 62;
         TableView.SectionHeaderHeight = 120;
         TableView.ShowsVerticalScrollIndicator = false;
         TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
         TableView.ContentInset = new UIEdgeInsets( View.Frame.Height * 0.33f, 0, 0, 0 );

         TableView.RegisterClassForCellReuse( typeof( HomeListItemCell ), cellID );
      }

      public override nint NumberOfSections( UITableView tableView ) => 1;

      public override UIView GetViewForHeader( UITableView tableView, nint section ) => new HomeSectionItemView( );

      public override nint RowsInSection( UITableView tableView, nint section ) => viewModel.ListItemViewModels.Count;

      public override UITableViewCell GetCell( UITableView tableView, NSIndexPath indexPath )
      {
         var cell = tableView.DequeueReusableCell( cellID, indexPath ) as HomeListItemCell;
         cell.ListItemViewModel = viewModel.ListItemViewModels[ indexPath.Row ];

         return cell;
      }

      public override void RowSelected( UITableView tableView, NSIndexPath indexPath )
      {
         viewModel.ListItemViewModels[ indexPath.Row ].Selected( );
         tableView.DeselectRow( indexPath, animated: false );
      }

      #region IZoningViewModel
      void IHomeViewModel.NavigateToCalibrateScreen( )
      {
         var vc = new CalibrateController( );
         NavigationController.PushViewController( viewController: vc, animated: true );
      }

      void IHomeViewModel.NotifyLockChanged( )
      {
      }
      #endregion

      #region Event Handlers
      private void HandleSettingButtonTouchUpInside( object sender, EventArgs e )
      {
         ( NavigationController as RootNavigationController ).ReverseTransitioning = true;

         var vc = new SettingController( );
         NavigationController.PushViewController( viewController: vc, animated: true );
      }
      #endregion
   }
}
