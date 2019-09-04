   using System;
using CoreGraphics;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Views
{
   public class ListItemCell : UITableViewCell
   {
      private ListItemViewModel listItemViewModel;
      public ListItemViewModel ListItemViewModel
      {
         get => listItemViewModel;
         set
         {
            listItemViewModel = value;

            switch( listItemViewModel.ListType )
            {
               case ListItemViewModel.Type.PhoneKey:
                  listIconImageView.Image = Images.CellPhoneKey;
                  actionImageView.Image = Images.InformationOutline;
                  break;
               case ListItemViewModel.Type.Account:
                  listIconImageView.Image = Images.Account;
                  actionImageView.Image = Images.ChevronRight;
                  break;
               case ListItemViewModel.Type.Location:
                  listIconImageView.Image = Images.Navigation;
                  actionImageView.Image = Images.ChevronRight;
                  break;
               case ListItemViewModel.Type.ShareKey:
                  listIconImageView.Image = Images.KeyVarient;
                  actionImageView.Image = Images.ChevronRight;
                  break;
               case ListItemViewModel.Type.CalibrateDevice:
                  break;
               case ListItemViewModel.Type.ConfigureZones:
                  break;
            }

            listIconImageView.Hidden = listItemViewModel.LeftImageHidden;
            actionImageView.Hidden = listItemViewModel.RightImageHidden;   

            titleLabel.Text = listItemViewModel.Title;
            titleLabel.Hidden = string.IsNullOrEmpty( listItemViewModel.Title );

            subTitleLabel.Text = ListItemViewModel.SubTitle;
            subTitleLabel.Hidden = string.IsNullOrEmpty( listItemViewModel.SubTitle );
         }
      }

      // UI Elements
      private UIImageView listIconImageView;
      private UILabel titleLabel;
      private UILabel subTitleLabel;
      private UIImageView actionImageView;
      private AnchoredConstraints stackViewAnchoredConstraints;

      public ListItemCell( IntPtr ptr ) : base( ptr )
      {
         SetupViews( );
      }

      private void SetupViews( )
      {
         listIconImageView = new CustomUIImageView( new CGSize( 24, 24 ) ) {
            TintColor = Colors.White,
         };

         titleLabel = new UILabel {
            TextColor = Colors.White,
            Font = Fonts.Bold.WithSize( 17f ),
            Hidden = string.IsNullOrEmpty( listItemViewModel?.Title ),
         };

         subTitleLabel = new UILabel {
            TextColor = Colors.InnerSpace,
            Font = Fonts.Regular.WithSize( 13f ),
            Hidden = string.IsNullOrEmpty( listItemViewModel?.SubTitle ),
         };

         actionImageView = new CustomUIImageView( new CGSize( 24, 24 ) ) {
            TintColor = Colors.White
         };

         var stackView = HStack(
            listIconImageView,
            VStack( titleLabel, subTitleLabel ).With( alignment: UIStackViewAlignment.Leading ).WithPadding( new UIEdgeInsets( 6, 0, 6, 0 ) ),
            actionImageView
         ).With( spacing: 21 );

         ContentView.AddSubview( stackView );

         stackViewAnchoredConstraints =  stackView.FillLayoutMarginsGuideSuperView( padding: new UIEdgeInsets( 0, 12, 0, 12 ) );
      }

      public override void SetHighlighted( bool highlighted, bool animated )
      {
         listIconImageView.Alpha = highlighted ? 0.3f : 1;
         titleLabel.Alpha = highlighted ? 0.3f : 1;
         subTitleLabel.Alpha = highlighted ? 0.3f : 1;
         actionImageView.Alpha = highlighted ? 0.3f : 1;
      }

      public override void SetSelected( bool selected, bool animated )
      {
         listIconImageView.Alpha = selected ? 0.3f : 1;
         titleLabel.Alpha = selected ? 0.3f : 1;
         subTitleLabel.Alpha = selected ? 0.3f : 1;
         actionImageView.Alpha = selected ? 0.3f : 1;
      }
   }
}
