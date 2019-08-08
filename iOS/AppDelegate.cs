using System;
using Foundation;
using ObjCRuntime;
using PK.iOS.Controllers;
using PK.iOS.Helpers;
using PK.Models;
using UIKit;

namespace PK.iOS
{
   // The UIApplicationDelegate for the application. This class is responsible for launching the
   // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
   [Register( "AppDelegate" )]
   public class AppDelegate : UIApplicationDelegate
   {
      public override UIWindow Window { get; set; }

      public override bool FinishedLaunching( UIApplication application, NSDictionary launchOptions )
      {
#if DEBUG
         // Log Realm file location
         try
         {
            Console.WriteLine( $"Realm file location: {PKRealm.Path}" );
         }
         catch( Exception ex )
         {
            Console.WriteLine( $"Exception: {ex.Message}" );
         }
#endif

         Window = new UIWindow( );
         Window.MakeKeyAndVisible( );
         Window.RootViewController = new RootNavigationController( );

         ApplyGlobalStyling( );

         return true;
      }

      private void ApplyGlobalStyling( )
      {
         UINavigationBar.Appearance.SetBackgroundImage( backgroundImage: new UIImage( ), barMetrics: UIBarMetrics.Default );
         UINavigationBar.Appearance.ShadowImage = new UIImage( );
         UINavigationBar.Appearance.Translucent = true;
         UINavigationBar.Appearance.BackgroundColor = Colors.Clear;
         UINavigationBar.Appearance.SetTitleTextAttributes( new UITextAttributes {
            Font = Fonts.Bold.WithSize( 21f ),
            TextColor = Colors.White,
         } );

         UITableView.Appearance.BackgroundColor = Colors.Clear;

         UITableViewCell.Appearance.BackgroundColor = Colors.Clear;

         UIPageControl.Appearance.CurrentPageIndicatorTintColor = Colors.White;
      }

      public override void OnResignActivation( UIApplication application )
      {
         // Invoked when the application is about to move from active to inactive state.
         // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
         // or when the user quits the application and it begins the transition to the background state.
         // Games should use this method to pause the game.
      }

      public override void DidEnterBackground( UIApplication application )
      {
         // Use this method to release shared resources, save user data, invalidate timers and store the application state.
         // If your application supports background exection this method is called instead of WillTerminate when the user quits.
      }

      public override void WillEnterForeground( UIApplication application )
      {
         // Called as part of the transiton from background to active state.
         // Here you can undo many of the changes made on entering the background.
      }

      public override void OnActivated( UIApplication application )
      {
         // Restart any tasks that were paused (or not yet started) while the application was inactive. 
         // If the application was previously in the background, optionally refresh the user interface.
      }

      public override void WillTerminate( UIApplication application )
      {
         // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
      }

      public UIInterfaceOrientationMask OrientationLock = UIInterfaceOrientationMask.Portrait; // Default Orientation

      public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations( UIApplication application, [Transient] UIWindow forWindow )
      {
         return OrientationLock;
      }
   }
}

