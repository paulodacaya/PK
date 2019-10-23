using System;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Views
{
   public class ExpandableCardCell : CardCell
   {
      public new const string CellId = "ExpandableCardCellId";

      public UIImageView VisualImageView { get; }
      public UILabel RssiLabel { get; }

      private readonly UIStackView expandedContentStackView;

      public ExpandableCardCell( IntPtr handler ) : base( handler )
      {
         VisualImageView = Components.UIImageView( Images.PK4, Colors.BoschBlue );
         RssiLabel = Components.UILabel( string.Empty, Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ), lines: 1 );

         expandedContentStackView = VStack(
            VisualImageView.WithHeight( 180 ),
            new UIView( )
         ).With( alignment: UIStackViewAlignment.Center );

         contentStackView.AddArrangedSubview( expandedContentStackView );

         ContentView.AddSubview( RssiLabel );

         RssiLabel.Anchor( bottom: ContentView.BottomAnchor, trailing: ContentView.LayoutMarginsGuide.TrailingAnchor,
            padding: new UIEdgeInsets( 0, 0, 24, 16 ) );
      }

      public override void SetViewModel( CardItemViewModel viewModel )
      {
         base.SetViewModel( viewModel );

         expandedContentStackView.Hidden = !viewModel.IsExpanded;
         RssiLabel.Hidden = !viewModel.IsExpanded;
      }
   }
}
