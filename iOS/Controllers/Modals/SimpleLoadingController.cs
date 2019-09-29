using PK.iOS.Helpers;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class SimpleLoadingController : AbstractDialogController
   {
      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         var activityIndicatorView = new UIActivityIndicatorView( UIActivityIndicatorViewStyle.WhiteLarge );
         activityIndicatorView.Color = Colors.BoschGray;
         activityIndicatorView.StartAnimating( );

         var messageLabel = Components.UILabel( "Please wait...", Colors.BoschBlue, Fonts.BoschLight.WithSize( 18 ) );

         var stackView = VStack(
            activityIndicatorView,
            messageLabel
         ).With( spacing: 12, alignment: UIStackViewAlignment.Center ).WithPadding( new UIEdgeInsets( 22, 14, 22, 14 ) );

         DialogContainer.AddSubview( stackView );

         stackView.FillSuperview( );
      }
   }
}
