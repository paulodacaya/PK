using System;
using CoreGraphics;
using PK.iOS.Helpers;
using UIKit;

namespace PK.iOS.Controllers
{
   public class SettingController : UIViewController
   {
      public SettingController( )
      {
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNavigation( );
         SetupViews( );
      }

      private void SetupNavigation( )
      {
         NavigationItem.Title = "Settings";

         NavigationItem.HidesBackButton = true;

         var backButton = new UIButton {
            TintColor = Colors.White,
         };
         backButton.SetBackgroundImage( Images.ChevronRight, UIControlState.Normal );
         backButton.TouchUpInside += HandleBackButtonTouchUpInside;
         backButton.Anchor( size: new CGSize( 20, 20 ) );

         NavigationItem.RightBarButtonItem = new UIBarButtonItem( customView: backButton );
      }

      private void SetupViews( )
      {
         View.BackgroundColor = Colors.OuterSpace;
      }

      #region Event Handlers
      private void HandleBackButtonTouchUpInside( object sender, EventArgs e )
      {
         NavigationController.PopViewController( animated: true );

         ( NavigationController as RootNavigationController ).ReverseTransitioning = false;
      }
      #endregion
   }
}
