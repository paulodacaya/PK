using System;
using CoreGraphics;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Views
{
   public class HomeListItemCell : UITableViewCell
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
                  break;
               case ListItemViewModel.Type.Account:
                  listIconImageView.Image = Images.Account;
                  break;
               case ListItemViewModel.Type.Location:
                  listIconImageView.Image = Images.Navigation;
                  break;
               case ListItemViewModel.Type.Calibrate:
                  listIconImageView.Image = Images.CellPhoneWireless;
                  break;
               case ListItemViewModel.Type.ShareKey:
                  listIconImageView.Image = Images.KeyVarient;
                  break;
            }

            titleLabel.Text = listItemViewModel.Title;
            titleLabel.Hidden = string.IsNullOrEmpty( listItemViewModel.Title );

            subTitleLabel.Text = ListItemViewModel.SubTitle;
            subTitleLabel.Hidden = string.IsNullOrEmpty( listItemViewModel.SubTitle );

            switch( listItemViewModel.ListOperation )
            {
               case ListItemViewModel.Operation.Inform:
                  actionImageView.Image = Images.InformationOutline;
                  break;
               case ListItemViewModel.Operation.Navigate:
                  actionImageView.Image = Images.ChevronRight;
                  break;
            }
         }
      }

      // UI Elements
      private UIImageView listIconImageView;
      private UILabel titleLabel;
      private UILabel subTitleLabel;
      private UIImageView actionImageView;

      public HomeListItemCell( IntPtr ptr ) : base( ptr )
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
            TextColor = Colors.White,
            Font = Fonts.Regular.WithSize( 13f ),
            Hidden = string.IsNullOrEmpty( listItemViewModel?.SubTitle ),
         };

         actionImageView = new CustomUIImageView( new CGSize( 24, 24 ) ) {
            TintColor = Colors.White,
         };

         var stackView = HStack(
            listIconImageView,
            VStack( titleLabel, subTitleLabel ).With( alignment: UIStackViewAlignment.Leading ).WithPadding( new UIEdgeInsets( 6, 0, 6, 0 ) ),
            actionImageView
         ).With( spacing: 21 ).WithPadding( new UIEdgeInsets( 0, 12, 0, 12 ) );

         ContentView.AddSubview( stackView );

         stackView.FillLayoutMarginsGuideSuperView( );
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
