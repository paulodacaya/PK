using PK.iOS.Helpers;
using UIKit;

namespace PK.iOS.Controllers
{
   public abstract class AbstractDialogController : UIViewController
   {
      public override UIStatusBarStyle PreferredStatusBarStyle( ) => UIStatusBarStyle.LightContent;

      public override UIModalTransitionStyle ModalTransitionStyle => UIModalTransitionStyle.CrossDissolve;
      public override UIModalPresentationStyle ModalPresentationStyle => UIModalPresentationStyle.OverCurrentContext;

      protected UIView DialogContainer;

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         View.BackgroundColor = Colors.BoschBlack.ColorWithAlpha( 0.3f );

         DialogContainer = new UIView { BackgroundColor = Colors.White };
         DialogContainer.WithCornerRadius( Values.CornerRadius );
         DialogContainer.WithShadow( 0.3f, 4, color: Colors.BoschBlack );

         View.AddSubview( DialogContainer );

         DialogContainer.CenterInSuperView( );
      }
   }
}
