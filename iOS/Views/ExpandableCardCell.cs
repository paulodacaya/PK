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

      private readonly UIStackView expandedContentStackView;
      public UIImageView VisualImageView;

      public ExpandableCardCell( IntPtr handler ) : base( handler )
      {
         VisualImageView = Components.UIImageView( Images.PK4, Colors.BoschBlue );

         expandedContentStackView = VStack(
            VisualImageView.WithHeight( 180 ),
            new UIView( )
         ).With( alignment: UIStackViewAlignment.Center );

         contentStackView.AddArrangedSubview( expandedContentStackView );
      }

      public override void SetViewModel( CardItemViewModel viewModel )
      {
         base.SetViewModel( viewModel );

         expandedContentStackView.Hidden = !viewModel.IsExpanded;
      }
   }
}
