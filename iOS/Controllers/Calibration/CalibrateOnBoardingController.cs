using System;
using CoreGraphics;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class CalibrateOnBoardingController : UIViewController, IOnBoardingViewModel
   {
      private readonly CalibrateOnBoardingViewModel viewModel;
      private UIButton actionButton;

      // UI Elements

      public CalibrateOnBoardingController( )
      {
         viewModel = new CalibrateOnBoardingViewModel( this );
      }

      public override UIStatusBarStyle PreferredStatusBarStyle( ) => UIStatusBarStyle.LightContent;

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNavigation( );
         SetupView( );
         SetupContent( );
         SetupActionButton( );
      }

      public override void ViewDidAppear( bool animated )
      {
         base.ViewDidAppear( animated );

         UIView.AnimateNotify( duration: 1, delay: 0, options: UIViewAnimationOptions.CurveEaseOut,
            animation: ( ) => {
               actionButton.Alpha = 1;
            }, completion: null );

         PresentViewController( new CameraCalibrationPopUpController( null ), animated: false, null );
      }

      private void SetupNavigation( )
      {
         NavigationController.SetNavigationBarHidden( true, animated: false );
      }

      private void SetupView( )
      {
         View.BackgroundColor = Colors.OuterSpace;
      }

      private void SetupContent( )
      {
         var title = new PKLabel( "Welcome to the Perfectly Keyless App!", Colors.White, Fonts.Bold.WithSize( 24 ) );
         var subTitle = new PKLabel( "Your new digital key on a mobile phone", Colors.Gray, Fonts.Medium.WithSize( 18 ) );
         var number1 = new PKNumberView( "1" );
         var instruction1 = new PKLabel( "Before we begin, we need to first calibrate your device with your vehicle.", Colors.White, Fonts.Regular.WithSize( 16 ) );
         var number2 = new PKNumberView( "2" );
         var instruction2_0 = new PKLabel( "Get your backup Access Card.", Colors.White, Fonts.Regular.WithSize( 16 ) );
         var instruction2_1 = new PKLabel( "This card can also unlock your vehicle.", Colors.InnerSpace, Fonts.Regular.WithSize( 15 ) );
         var image2 = new PKImageView( Images.CardBulletedOutline, Colors.White );
         var number3 = new PKNumberView( "3" );
         var instruction3 = new PKLabel( "Stick the card on your passanger door handle. Image must be facing you.", Colors.White, Fonts.Regular.WithSize( 16 ) );
         var number4 = new PKNumberView( "4" );
         var instruction4 = new PKLabel( "Follow the on screen instructions to complete the calibration.", Colors.White, Fonts.Regular.WithSize( 16 ) );

         var stackView = VStack(
            new CustomUIView( new CGSize( 0, 18 ) ),
            title,
            new CustomUIView( new CGSize( 0, 18 ) ),
            subTitle,
            new CustomUIView( new CGSize( 0, 22 ) ),
            HStack(
               number1.WithSquareSize( 30 ).WithCornerRadius( 15 ),
               instruction1
            ).With( spacing: 12, alignment: UIStackViewAlignment.Top ),
            new CustomUIView( new CGSize( 0, 40 ) ),
            HStack(
               number2.WithSquareSize( 30 ).WithCornerRadius( 15 ),
               VStack(
                  instruction2_0,
                  instruction2_1
               ).With( spacing: 4 )
            ).With( spacing: 12, alignment: UIStackViewAlignment.Top ),
            new CustomUIView( new CGSize( 0, 40 ) ),
            image2.WithHeight( 65 ),
            new CustomUIView( new CGSize( 0, 40 ) ),
            HStack(
               number3.WithSquareSize( 30 ).WithCornerRadius( 15 ),
               instruction3
            ).With( spacing: 12, alignment: UIStackViewAlignment.Top ),
            new CustomUIView( new CGSize( 0, 40 ) ),
            HStack(
               number4.WithSquareSize( 30 ).WithCornerRadius( 15 ),
               instruction4
            ).With( spacing: 12, alignment: UIStackViewAlignment.Top ),
            new CustomUIView( new CGSize( 0, 40 ) )
         );

         var scrollView = new UIScrollView( );

         View.AddSubview( scrollView );

         scrollView.Anchor( leading: View.LayoutMarginsGuide.LeadingAnchor, top: View.TopAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor, bottom: View.BottomAnchor );

         scrollView.AddSubview( stackView );

         stackView.FillSuperview( );
         stackView.WidthAnchor.ConstraintEqualTo( scrollView.WidthAnchor ).Active = true;
      }

      private void SetupActionButton( )
      {
         actionButton = new UIButton( UIButtonType.System );
         actionButton.SetImage( Images.ChevronRight, UIControlState.Normal );
         actionButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
         actionButton.ImageView.WithSquareSize( 20 );
         actionButton.TintColor = Colors.OuterSpace;
         actionButton.BackgroundColor = Colors.White;
         actionButton.WithSquareSize( 50 );
         actionButton.WithCornerRadius( 25 );
         actionButton.WithShadow( opacity: 0.5f, radius: 2, color: Colors.White );
         actionButton.Alpha = 0;
         actionButton.TouchUpInside += HandleActionButtonTouchUpInside;

         View.AddSubview( actionButton );

         actionButton.Anchor( bottom: View.LayoutMarginsGuide.BottomAnchor, trailing: View.LayoutMarginsGuide.TrailingAnchor,
            padding: new UIEdgeInsets( 0, 0, 14, 0 ) );
      }

      private void HandleActionButtonTouchUpInside( object sender, EventArgs e ) => viewModel.GoSelected( );

      void IOnBoardingViewModel.NavigateToCameraCalibration( )
      {
         var cameraCalibrationController = new CameraCalibrationController( );
         NavigationController.PushViewController( cameraCalibrationController, animated: true );
      }
   }
}
