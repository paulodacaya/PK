using System;
using PK.iOS.Helpers;
using UIKit;

namespace PK.iOS.Controllers
{
   public abstract class BaseModalController : UIViewController
   {
      protected UIToolbar TopToolBar;

      protected BaseModalController( )
      {
         ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
         ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupVisualEffectBlur( );
         SetupTopToolbar( );
      }

      private void SetupVisualEffectBlur( )
      {
         var blurEffect = UIBlurEffect.FromStyle( UIBlurEffectStyle.Dark );
         var visualEffectView = new UIVisualEffectView( effect: blurEffect );

         View.AddSubview( visualEffectView );

         visualEffectView.FillSuperview( );
      }

      private void SetupTopToolbar( )
      {
         var closeButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
            TintColor = Colors.White
         };
         closeButton.ConstraintWidthEqualTo( 22 );
         closeButton.ConstraintHeightEqualTo( 22 );
         closeButton.SetImage( Images.Close, UIControlState.Normal );
         closeButton.TouchUpInside += HandleCloseButtonTouchUpInside;

         var closeBarButton = new UIBarButtonItem( customView: closeButton );
         var flexibleSpace = new UIBarButtonItem( UIBarButtonSystemItem.FlexibleSpace );

         var arrangedBarButtonItems = new UIBarButtonItem[ ] { closeBarButton, flexibleSpace };

         TopToolBar = new UIToolbar {
            Items = arrangedBarButtonItems,
         };
         TopToolBar.SetBackgroundImage( new UIImage( ), UIToolbarPosition.Any, UIBarMetrics.Default );
         TopToolBar.SetShadowImage( new UIImage( ), UIToolbarPosition.Any );

         View.AddSubview( TopToolBar );

         TopToolBar.Anchor( top: View.LayoutMarginsGuide.TopAnchor, leading: View.LeadingAnchor, trailing: View.TrailingAnchor );
      }

      private void HandleCloseButtonTouchUpInside( object sender, EventArgs e )
      {
         DismissViewController( animated: true, completionHandler: null );
      }
   }
}
