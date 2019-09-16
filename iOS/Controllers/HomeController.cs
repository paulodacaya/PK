using System;
using CoreGraphics;
using Foundation;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class HomeController : UITableViewController, IHomeViewModel
   {
      private readonly HomeViewModel viewModel;

      // UI Elements

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
      }

      public override nint NumberOfSections( UITableView tableView ) => 2;

      public override UIView GetViewForHeader( UITableView tableView, nint section )
      {
         if( section == 0 )
         {
            var greetingLabel = Components.UILabel( "Good afternoon", Colors.BoschBlue, Fonts.BoschBold.WithSize( 26 ) );
            var vehicleInfolabel = Components.UILabel( "Current Vehicle: Mazda 3 (ABE 674)", Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ) );
            var vehicleStateLabel = Components.UILabel( "State: Locked", Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ) );
            var vehicleStateImageView = Components.UIImageView( Images.PadlockLock, Colors.BoschBlue );

            var stackView = VStack(
               greetingLabel,
               vehicleInfolabel,
               HStack(
                  vehicleStateLabel,
                  vehicleStateImageView.WithSquareSize( 12 ),
                  new UIView( )
               ),
               new UIView( )
            ).With( spacing: 6 ).WithPadding( new UIEdgeInsets( 24, 12, 34, 12 ) );
            stackView.SetCustomSpacing( 12, greetingLabel );

            var containerView = new UIView {
               PreservesSuperviewLayoutMargins = true
            };

            containerView.AddSubview( stackView );

            stackView.Anchor( leading: containerView.LayoutMarginsGuide.LeadingAnchor, top: containerView.TopAnchor,
               trailing: containerView.LayoutMarginsGuide.TrailingAnchor, bottom: containerView.BottomAnchor );

            return containerView;
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
         var cell = tableView.DequeueReusableCell( CardCell.CellId, indexPath ) as CardCell;
         return cell;
      }
   }

   public class CardCell : UITableViewCell
   {
      public const string CellId = "CardCellId";

      private UIView cardContainer;

      public CardCell( IntPtr handler ) : base( handler )
      {
         PreservesSuperviewLayoutMargins = true;
         BackgroundColor = Colors.Clear;

         ContentView.PreservesSuperviewLayoutMargins = true;
         ContentView.BackgroundColor = Colors.Clear;

         cardContainer = new UIView { BackgroundColor = Colors.White };
         cardContainer.WithShadow( 0.2f, offset: new CGSize( 0, 4 ), radius: 4, color: Colors.BoschBlack );
         cardContainer.WithCornerRadius( Values.CornerRadius );

         var titleLabel = Components.UILabel( "Re-calibrate your device", Colors.BoschBlue, Fonts.BoschBold.WithSize( 18 ) );
         var iconImageView = Components.UIImageView( Images.Restart, Colors.BoschBlue );
         var messageLabel = Components.UILabel( "A simple AR (Augmeneted Reality) calibration experience to ensure your mobiles proximity is accurate with your vehicle.", Colors.BoschBlack, Fonts.BoschLight.WithSize( 15 ) );

         var stackView = VStack(
            HStack(
               titleLabel,
               iconImageView.WithSquareSize( 20 )
            ),
            messageLabel
         ).With( spacing: 12 ).WithPadding( new UIEdgeInsets( 24, 16, 24, 16 ) );

         ContentView.AddSubview( cardContainer );

         cardContainer.Anchor( leading: ContentView.LayoutMarginsGuide.LeadingAnchor, top: ContentView.TopAnchor,
            trailing: ContentView.LayoutMarginsGuide.TrailingAnchor, bottom: ContentView.BottomAnchor );

         cardContainer.AddSubview( stackView );

         stackView.FillSuperview( );
      }

      public override void SetHighlighted( bool highlighted, bool animated )
      {
         AnimateNotify( duration: 0.2f, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {
            if( highlighted )
            {
               cardContainer.Transform = CGAffineTransform.MakeScale( sx: 0.98f, sy: 0.98f );
            }
            else
            {
               cardContainer.Transform = CGAffineTransform.MakeIdentity( );
            }
         }, completion: null );
      }

      public override void SetSelected( bool selected, bool animated ) { /* PREVENT DEFAULT BEHAVOIR */ }
   }
}
