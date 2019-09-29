using System;
using CoreGraphics;
using PK.iOS.Controllers;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Views
{
   public class CardCell : UITableViewCell, IBindViewModel<CardItemViewModel>
   {
      public const string CellId = "CardCellId";

      private readonly UIView cardContainer;
      private readonly UILabel titleLabel;
      private readonly UIImageView iconImageView;
      private readonly UILabel messageLabel;

      protected readonly UIStackView contentStackView;

      public CardCell( IntPtr handler ) : base( handler )
      {
         PreservesSuperviewLayoutMargins = true;
         BackgroundColor = Colors.Clear;

         ContentView.PreservesSuperviewLayoutMargins = true;
         ContentView.BackgroundColor = Colors.Clear;

         cardContainer = new UIView { BackgroundColor = Colors.White };
         cardContainer.WithShadow( 0.2f, offset: new CGSize( 0, 4 ), radius: 4, color: Colors.BoschBlack );
         cardContainer.WithCornerRadius( Values.CornerRadius );

         titleLabel = Components.UILabel( null, Colors.BoschBlue, Fonts.BoschBold.WithSize( 18 ) );
         iconImageView = Components.UIImageView( null, Colors.BoschBlue );
         messageLabel = Components.UILabel( null, Colors.BoschBlack, Fonts.BoschLight.WithSize( 15 ) );

         contentStackView = VStack(
            HStack(
               titleLabel,
               VStack(
                  iconImageView.WithSquareSize( 20 )
               )
            ).With( spacing: 14, alignment: UIStackViewAlignment.Center ),
            messageLabel,
            new UIView( )
         ).With( spacing: 12 ).WithPadding( new UIEdgeInsets( 24, 16, 24, 16 ) );

         ContentView.AddSubview( cardContainer );

         cardContainer.Anchor( leading: ContentView.LayoutMarginsGuide.LeadingAnchor, top: ContentView.TopAnchor,
            trailing: ContentView.LayoutMarginsGuide.TrailingAnchor, bottom: ContentView.BottomAnchor );

         cardContainer.AddSubview( contentStackView );

         contentStackView.FillSuperview( );
      }

      public virtual void SetViewModel( CardItemViewModel viewModel )
      {
         titleLabel.Text = viewModel.Title;
         messageLabel.Text = viewModel.Message;

         switch( viewModel.Id )
         {
            case CardItemViewModel.c_id_calibrate:
               iconImageView.Image = Images.Restart;
               break;
            case CardItemViewModel.c_id_vehicleZoneVisual:
               iconImageView.Image = Images.CircleSlice;
               break;
         }
      }

      public override void SetHighlighted( bool highlighted, bool animated )
      {
         AnimateNotify( duration: 0.2f, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {
            if( highlighted )
               cardContainer.Transform = CGAffineTransform.MakeScale( sx: 0.98f, sy: 0.98f );
            else
               cardContainer.Transform = CGAffineTransform.MakeIdentity( );
         }, completion: null );
      }

      public override void SetSelected( bool selected, bool animated ) { /* PREVENT DEFAULT BEHAVIOR */ }
   }
}
