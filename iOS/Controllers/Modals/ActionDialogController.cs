using System;
using PK.iOS.Helpers;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class ActionDialogController : AbstractDialogController
   {
      private UILabel titleLabel;
      private UILabel messageLabel;
      private UIStackView buttonStackView;


      public ActionDialogController( string title, string message )
      {
         titleLabel = Components.UILabel( title, Colors.BoschBlue, Fonts.BoschLight.WithSize( 18 ) );
         messageLabel = Components.UILabel( message, Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ) );
         buttonStackView = HStack( ).With( spacing: 12, distribution: UIStackViewDistribution.FillEqually );
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         var stackView = VStack(
            titleLabel,
            messageLabel.WithWidth( 310 ),
            buttonStackView
         ).With( spacing: 12 ).WithPadding( new UIEdgeInsets( 14, 14, 14, 14 ) );

         stackView.SetCustomSpacing( 24, messageLabel );

         DialogContainer.AddSubview( stackView );

         stackView.FillSuperview( );
      }

      public void AddAction( string title, Action touchUpInsideHandler )
      {
         var button = Components.UIButton( title, Colors.White, Fonts.Bold.WithSize( 15 ), padding: new UIEdgeInsets( 8, 12, 8, 12 ) );
         button.BackgroundColor = Colors.BoschBlue;
         button.WithCornerRadius( Values.CornerRadius );
         button.TouchUpInside += ( sender, eventArgs ) => {
            DismissViewController( animated: true, touchUpInsideHandler );
         };

         buttonStackView.AddArrangedSubview( button );
      }
   }
}
