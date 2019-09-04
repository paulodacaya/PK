using System;
using CoreGraphics;
using Foundation;
using PK.iOS.Helpers;
using PK.iOS.Views;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;
using Xamarin.Essentials;

namespace PK.iOS.Controllers
{
   public class SettingController : UITableViewController, ISettingViewModel
   {
      private readonly SettingViewModel viewModel;
      private readonly string cellId = "settingListCellId";

      public SettingController( )
      {
         viewModel = new SettingViewModel( this ); 
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNavigation( );
         SetupViews( );
         SetupTableView( );
      }

      private void SetupNavigation( )
      {
         NavigationItem.Title = "Settings";

         NavigationItem.HidesBackButton = true;

         var backButton = new UIButton {
            TintColor = Colors.White,
         };
         backButton.SetBackgroundImage( Images.ChevronRight, UIControlState.Normal );
         backButton.TouchUpInside += HandleBackButtonTouchUpInside;
         backButton.Anchor( size: new CGSize( 20, 20 ) );

         NavigationItem.RightBarButtonItem = new UIBarButtonItem( customView: backButton );
      }

      private void SetupViews( )
      {
         View.BackgroundColor = Colors.OuterSpace;
      }

      private void SetupTableView( )
      {
         TableView.RegisterClassForCellReuse( typeof( ListItemCell ), cellId );
         TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
      }

      public override nint NumberOfSections( UITableView tableView ) => 1;

      public override UIView GetViewForHeader( UITableView tableView, nint section )
      {
         var containerView = new UIView { PreservesSuperviewLayoutMargins = true };
         var lineView = new UIView { BackgroundColor = Colors.InnerSpace };

         containerView.AddSubview( lineView );

         lineView.Anchor( leading: containerView.LayoutMarginsGuide.LeadingAnchor, top: containerView.TopAnchor,
            trailing: containerView.LayoutMarginsGuide.TrailingAnchor, bottom: containerView.BottomAnchor );

         return containerView;
      } 

      public override nfloat GetHeightForHeader( UITableView tableView, nint section )
      {
         if( section == 0 )
            return 0.2f;

         return base.GetHeightForHeader( tableView, section );
      }

      public override UIView GetViewForFooter( UITableView tableView, nint section )
      {
         var containerView = new UIView { PreservesSuperviewLayoutMargins = true };
         var lineView = new UIView { BackgroundColor = Colors.InnerSpace };

         var signOutButton = new UIButton( UIButtonType.System );
         signOutButton.SetTitle( "SIGN OUT", UIControlState.Normal );
         signOutButton.SetTitleColor( Colors.White, UIControlState.Normal );
         signOutButton.TitleLabel.Font = Fonts.Bold.WithSize( 14 );
         signOutButton.ContentEdgeInsets = new UIEdgeInsets( 12, 30, 12, 30 );
         signOutButton.WithBorder( 1, Colors.InnerSpace );
         signOutButton.WithCornerRadius( Values.CornerRadius );

         var versionLabel = new UILabel {
            Text = viewModel.VersionAndBuild,
            TextColor = Colors.Gray,
            Font = Fonts.Regular.WithSize( 14 )
         };

         var stackView = VStack(
            signOutButton,
            versionLabel
         ).With( spacing: 15, alignment: UIStackViewAlignment.Center );

         containerView.AddSubviews( lineView, stackView );

         lineView.Anchor( leading: containerView.LayoutMarginsGuide.LeadingAnchor, top: containerView.TopAnchor,
            trailing: containerView.LayoutMarginsGuide.TrailingAnchor, size: new CGSize( 0, 0.2 ) );

         stackView.Anchor( leading: containerView.LayoutMarginsGuide.LeadingAnchor, top: lineView.BottomAnchor,
            trailing: containerView.LayoutMarginsGuide.TrailingAnchor, bottom: containerView.BottomAnchor,
            padding: new UIEdgeInsets( 22, 0, 0, 0 ) );

         return containerView;
      }

      public override nint RowsInSection( UITableView tableView, nint section ) => viewModel.ListItems.Count;  

      public override UITableViewCell GetCell( UITableView tableView, NSIndexPath indexPath )
      {
         var cell = tableView.DequeueReusableCell( cellId, indexPath ) as ListItemCell;
         cell.ListItemViewModel = viewModel.ListItems[ indexPath.Row ];

         return cell;
      }

      public override void RowSelected( UITableView tableView, NSIndexPath indexPath )
      {
         viewModel.ListItems[ indexPath.Row ].Selected( );
         tableView.DeselectRow( indexPath, animated: false );
      }

      private void HandleBackButtonTouchUpInside( object sender, EventArgs e )
      {
         if( NavigationController is RootNavigationController rootNavigationController )
         {
            rootNavigationController.PopViewController( animated: true );
            rootNavigationController.ReverseTransitioning = false;
         }
      }
   }
}
