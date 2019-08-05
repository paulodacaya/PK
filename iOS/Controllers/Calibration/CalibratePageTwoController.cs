using System;
using Foundation;
using PK.iOS.Helpers;
using static PK.iOS.Helpers.Stacks;
using PK.ViewModels;
using UIKit;
using PK.iOS.Views;

namespace PK.iOS.Controllers
{
   public class CalibratePageTwoController : UIViewController, IUITableViewDataSource, IUITableViewDelegate
   {
      private readonly CalibrateViewModel viewModel;

      private readonly string optionCellId = "optionCellId";

      public CalibratePageTwoController( CalibrateViewModel viewModel )
      {
         this.viewModel = viewModel;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupViews( );
      }

      private void SetupViews( )
      {
         var titleLabel = new UILabel {
            Text = viewModel.PageTwoTitleText,
            TextColor = Colors.White,
            Font = Fonts.Medium.WithSize( 18 ),
            TextAlignment = UITextAlignment.Center,
         };

         var tableView = new UITableView {
            DataSource = this,
            Delegate = this,
            SeparatorInset = new UIEdgeInsets( 0, 0, 0, 0 ),
            RowHeight = 72,
            SectionHeaderHeight = 0.2f,
            SectionFooterHeight = 0.5f, // Different sizing for footer to achieve same size as Header.
         };
         tableView.RegisterClassForCellReuse( typeof( OptionListItemCell ), optionCellId );

         var stackView = VStack(
            titleLabel,
            tableView
         ).With( spacing: 28 );

         View.AddSubview( stackView );

         stackView.FillSuperview( padding: new UIEdgeInsets( 0, 0, 22, 0 ) );
      }

      [Export( "numberOfSectionsInTableView:" )]
      public nint NumberOfSections( UITableView tableView ) => 1;

      [Export( "tableView:viewForHeaderInSection:" )]
      public UIView GetViewForHeader( UITableView tableView, nint section )
      {
         return new UIView {
            BackgroundColor = tableView.SeparatorColor,
         };
      }

      public nint RowsInSection( UITableView tableView, nint section ) => viewModel.OptionList.Length;

      public UITableViewCell GetCell( UITableView tableView, NSIndexPath indexPath )
      {
         var cell = tableView.DequeueReusableCell( reuseIdentifier: optionCellId, indexPath ) as OptionListItemCell;
         cell.OptionListViewModel = viewModel.OptionList[ indexPath.Row ];

         return cell;
      }

      [Export( "tableView:didSelectRowAtIndexPath:" )]
      public void RowSelected( UITableView tableView, NSIndexPath indexPath )
      {
         viewModel.OptionList[ indexPath.Row ].Selected( );
         tableView.DeselectRow( indexPath, animated: true );
      }

      [Export( "tableView:viewForFooterInSection:" )]
      public UIView GetViewForFooter( UITableView tableView, nint section )
      {
         return new UIView {
            BackgroundColor = tableView.SeparatorColor,
         };
      }
   }
}
