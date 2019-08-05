using System;
using CoreGraphics;
using PK.iOS.Helpers;
using PK.ViewModels;
using static PK.iOS.Helpers.Stacks;
using UIKit;

namespace PK.iOS.Views
{
   public class OptionListItemCell : UITableViewCell
   {
      public OptionListItemCell( IntPtr handle ) : base( handle )
      {
         SetupViews( );
      }

      private UIImageView iconImageView;
      private UILabel titleLabel;

      private OptionListViewModel optionListViewModel;
      public OptionListViewModel OptionListViewModel {
         get => optionListViewModel;
         set
         {
            optionListViewModel = value;

            switch( optionListViewModel.Option )
            {
               case OptionListViewModel.Camera:
                  iconImageView.Image = Images.AugmentedReality;
                  break;
               case OptionListViewModel.Manual:
                  iconImageView.Image = Images.ShoePrint;
                  break;
            }

            titleLabel.SetAttributedLineSpaceText( text: optionListViewModel.Text, textColor: Colors.White,
            textFont: Fonts.Regular.WithSize( 16 ), lineSpacing: 4.6f, textAlignment: UITextAlignment.Left );
         }
      }

      private void SetupViews( )
      {
         iconImageView = new UIImageView {
            TintColor = Colors.White,
            ContentMode = UIViewContentMode.ScaleAspectFit,
         };

         titleLabel = new UILabel( );

         var stackView = HStack(
            iconImageView.WithSquareSize( 24 ),
            titleLabel
         ).With( spacing: 21, alignment: UIStackViewAlignment.Center );

         ContentView.AddSubview( stackView );

         stackView.FillLayoutMarginsGuideSuperView( );
      }

      public override void SetHighlighted( bool highlighted, bool animated )
      {
         iconImageView.Alpha = highlighted ? 0.3f : 1;
         titleLabel.Alpha = highlighted ? 0.3f : 1;
      }

      public override void SetSelected( bool selected, bool animated )
      {
         iconImageView.Alpha = selected ? 0.3f : 1;
         titleLabel.Alpha = selected ? 0.3f : 1;
      }
   }
}
