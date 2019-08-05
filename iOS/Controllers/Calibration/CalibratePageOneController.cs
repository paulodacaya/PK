using System;
using Foundation;
using PK.iOS.Helpers;
using static PK.iOS.Helpers.Stacks;
using PK.ViewModels;
using UIKit;

namespace PK.iOS.Controllers
{
   public class CalibratePageOneController : UIViewController
   {
      private readonly CalibrateViewModel viewModel;

      public CalibratePageOneController( CalibrateViewModel viewModel )
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
            Text = viewModel.PageOneTitleText,
            TextColor = Colors.White,
            Font = Fonts.Medium.WithSize( 18 ),
         };

         var descriptionLabel = new UILabel( );
         descriptionLabel.SetAttributedLineSpaceText( text: viewModel.PageOneDescriptionText, textColor: Colors.White,
            textFont: Fonts.Regular.WithSize( 16 ), lineSpacing: 4.6f, textAlignment: UITextAlignment.Center );

         var stackView = VStack(
            titleLabel,
            descriptionLabel,
            new UIView( )
         ).With( spacing: 28, alignment: UIStackViewAlignment.Center );

         var actionButton = new PKButton( );
         actionButton.SetTitle( viewModel.PageOneActionText, UIControlState.Normal );
         actionButton.SetTitleColor( Colors.White, UIControlState.Normal );
         actionButton.TouchUpInside += HandleNextButtonTouchUpInside;

         View.AddSubviews( actionButton, stackView );

         actionButton.Anchor( centerX: View.CenterXAnchor, bottom: View.SafeAreaLayoutGuide.BottomAnchor,
            padding: new UIEdgeInsets( 0, 0, 22, 0 ) );

         stackView.Anchor( leading: View.LayoutMarginsGuide.LeadingAnchor, top: View.TopAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor, bottom: actionButton.TopAnchor, padding: new UIEdgeInsets( 0, 0, 22, 0 ) );
      }

      private void HandleNextButtonTouchUpInside( object sender, EventArgs e ) => viewModel.PageOneActionSelected( );
   }
}
