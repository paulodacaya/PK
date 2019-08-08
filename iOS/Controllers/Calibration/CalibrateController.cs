using System;
using CoreGraphics;
using Foundation;
using PK.iOS.Helpers;
using PK.ViewModels;
using static PK.iOS.Helpers.Stacks;
using UIKit;
using AVFoundation;

namespace PK.iOS.Controllers
{
   public class CalibrateController : UIViewController, ICalibrateViewModel, IUIPageViewControllerDataSource, IUIPageViewControllerDelegate
   {
      private readonly CalibrateViewModel viewModel;

      private int currentPageIndex;
      private UIViewController[ ] innerPageControllers;

      // UI Elements
      private UIView zoningContainerView;
      private UIPageViewController pageViewController;
      private CalibratePageOneController pageOneController;
      private CalibratePageTwoController pageTwoController;

      public CalibrateController( )
      {
         viewModel = new CalibrateViewModel( this );
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNavigation( );
         SetupZoningButton( );
         SetupPageView( );
      }

      public override void ViewWillAppear( bool animated )
      {
         base.ViewWillAppear( animated );

         ( NavigationController as RootNavigationController )?.EnterCalibrateAnimation( );
      }

      public override void ViewWillDisappear( bool animated )
      {
         base.ViewWillDisappear( animated );

         ( NavigationController as RootNavigationController )?.ExitCalibrateAnimation( );
      }

      public override void ViewDidLayoutSubviews( )
      {
         base.ViewDidLayoutSubviews( );

         zoningContainerView.Layer.CornerRadius = zoningContainerView.Frame.Height / 2;
      }

      private void SetupNavigation( )
      {
         var backButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
            TintColor = Colors.White,
         };
         backButton.SetImage( Images.ChevronLeft, UIControlState.Normal );
         backButton.Anchor( size: new CGSize( 22, 22 ) );
         backButton.TouchUpInside += ( sender, e ) => {
            NavigationController.PopViewController( animated: true );
         };

         NavigationItem.LeftBarButtonItem = new UIBarButtonItem( customView: backButton );
         NavigationItem.Title = viewModel.Title;
      }

      private void SetupZoningButton( )
      {
         zoningContainerView = new UIView( );
         zoningContainerView.WithBorder( 1, Colors.White );

         var tapGesture = new UILongPressGestureRecognizer( HandleConfigureZonesLongPress ) {
            MinimumPressDuration = 0,
         };
         zoningContainerView.AddGestureRecognizer( tapGesture );

         var zoningImageView = new UIImageView {
            Image = Images.CirclesExtended,
            TintColor = Colors.White,
            ContentMode = UIViewContentMode.ScaleAspectFit,
         };

         var zoningText = new UILabel {
            Text = viewModel.ConfigureZonesText,
            TextColor = Colors.White,
            Font = Fonts.Medium.WithSize( 15 )
         };

         var stackView = HStack(
            zoningImageView.WithWidth( 15 ),
            zoningText
         ).With( spacing: 8 ).WithPadding( new UIEdgeInsets( 0, 14, 0, 14 ) );

         zoningContainerView.AddSubview( stackView );

         stackView.FillSuperview( );

         View.AddSubview( zoningContainerView );

         zoningContainerView.Anchor( centerX: View.CenterXAnchor, top: View.LayoutMarginsGuide.TopAnchor,
            padding: new UIEdgeInsets( 10, 0, 0, 0 ), size: new CGSize( 0, 38 ) );
      }

      private void SetupPageView( )
      {
         pageOneController = new CalibratePageOneController( viewModel );
         pageTwoController = new CalibratePageTwoController( viewModel );

         innerPageControllers = new UIViewController[ ] { pageOneController, pageTwoController };

         pageViewController = new UIPageViewController( style: UIPageViewControllerTransitionStyle.Scroll,
            navigationOrientation: UIPageViewControllerNavigationOrientation.Horizontal ) {
            DataSource = this,
            Delegate = this,
         };

         pageViewController.SetViewControllers( new UIViewController[ ] { pageOneController }, direction: UIPageViewControllerNavigationDirection.Forward, animated: true, completionHandler: null );

         View.AddSubview( pageViewController.View );

         pageViewController.View.Anchor( leading: View.LeadingAnchor, bottom: View.SafeAreaLayoutGuide.BottomAnchor,
            trailing: View.TrailingAnchor, size: new CGSize( 0, UIScreen.MainScreen.Bounds.Height / 2 ) );

         AddChildViewController( pageViewController );
         pageViewController.DidMoveToParentViewController( parent: this );
      }

      #region IUIPageViewControllerDataSource, IUIPageViewControllerDelegate
      public UIViewController GetPreviousViewController( UIPageViewController pageViewController, UIViewController referenceViewController )
      {
         var viewControllerIndex = Array.IndexOf( innerPageControllers, referenceViewController );

         if( viewControllerIndex <= 0 )
            return null;

         currentPageIndex--;

         return innerPageControllers[ currentPageIndex ];
      }

      public UIViewController GetNextViewController( UIPageViewController pageViewController, UIViewController referenceViewController )
      {
         var viewControllerIndex = Array.IndexOf( innerPageControllers, referenceViewController );

         if( viewControllerIndex >= innerPageControllers.Length - 1 )
            return null;

         currentPageIndex++;

         return innerPageControllers[ currentPageIndex ];
      }

      [Export( "presentationCountForPageViewController:" )]
      public nint GetPresentationCount( UIPageViewController pageViewController )
      {
         return innerPageControllers.Length;
      }

      [Export( "presentationIndexForPageViewController:" )]
      public nint GetPresentationIndex( UIPageViewController pageViewController )
      {
         return currentPageIndex;
      }
      #endregion

      bool ICalibrateViewModel.VerifyCameraPermission( )
      {
         /*
          * This is using Native iOS API. Xamarin Essentials may release an easier way to do this.
          */

         if( AVCaptureDevice.GetAuthorizationStatus( AVAuthorizationMediaType.Video ) == AVAuthorizationStatus.Authorized )
            return true;

         // Request camera permission because it's not authorized
         AVCaptureDevice.RequestAccessForMediaType( AVAuthorizationMediaType.Video, granted => {
         } );

         return false;
      }

      void ICalibrateViewModel.NavigateToConfigureZones( )
      {
         var configureZonesController = new ConfigureZonesController( );
         NavigationController.PushViewController( configureZonesController, animated: true );
      }

      void ICalibrateViewModel.NavigateToPageTwo( )
      {
         currentPageIndex = Array.IndexOf( innerPageControllers, pageTwoController );
         pageViewController.SetViewControllers( new UIViewController[ ] { pageTwoController }, direction: UIPageViewControllerNavigationDirection.Forward, animated: true, completionHandler: null );
      }

      void ICalibrateViewModel.NavigateToCameraCalibration( )
      {
         var cameraCalibrationController = new CameraCalibrationController( );
         PresentViewController( cameraCalibrationController, animated: true, completionHandler: null );
      }

      void ICalibrateViewModel.NavigateToManualCalibration( )
      {
         var manualCalibrationController = new ManualCalibrationController( );
         NavigationController.PushViewController( manualCalibrationController, animated: true );
      }

      #region Event Handlers
      private void HandleConfigureZonesLongPress( UILongPressGestureRecognizer gesture )
      {
         switch( gesture.State )
         {
            case UIGestureRecognizerState.Began:
               zoningContainerView.BackgroundColor = Colors.WhiteWithTransparancy;
               return;
            case UIGestureRecognizerState.Ended:
               zoningContainerView.BackgroundColor = Colors.Clear;
               viewModel.ConfigureZonesSelected( );
               return;
         }
      }
      #endregion
   }
}