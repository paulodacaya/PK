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

         SetupView( );
         SetupContent( );
         SetupActionButton( );
      }

      private void SetupView( )
      {
         View.BackgroundColor = Colors.White;
      }

      private void SetupContent( )
      {
         var stackView = VStack(
            Components.UILabel( viewModel.Title, Colors.BoschBlue, Fonts.BoschBold.WithSize( 20 ) ),
            Components.UILabel( viewModel.SubTitle, Colors.BoschBlue, Fonts.BoschLight.WithSize( 16 ) ),
            Components.UILabel( viewModel.Message, Colors.BoschBlack, Fonts.BoschLight.WithSize( 15 ) ),
            Components.UILabel( viewModel.SectionOneTitle, Colors.BoschBlue, Fonts.BoschLight.WithSize( 18 ) ),
            Components.UILabel( viewModel.SectionOneMessage, Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ) ),
            Components.UIImageView( Images.PK2, Colors.BoschBlue ).WithHeight( 45 ),
            Components.UILabel( viewModel.SectionTwoTitle, Colors.BoschBlue, Fonts.BoschLight.WithSize( 18 ) ),
            Components.UILabel( viewModel.SectionTwoMessage, Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ) )
         ).With( spacing: 12 ).PaddingTop( 14 );
         stackView.SetCustomSpacing( 24, stackView.ArrangedSubviews[ 4 ] );
         stackView.SetCustomSpacing( 24, stackView.ArrangedSubviews[ 5 ] );

         var scrollView = new UIScrollView {
            PreservesSuperviewLayoutMargins = true,
         };

         View.AddSubview( scrollView );

         scrollView.FillSuperview( );
         scrollView.WidthAnchor.ConstraintEqualTo( View.WidthAnchor ).Active = true;

         scrollView.AddSubview( stackView );

         stackView.Anchor( leading: scrollView.LayoutMarginsGuide.LeadingAnchor, top: scrollView.TopAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor, bottom: scrollView.BottomAnchor );
      }

      private void SetupActionButton( )
      {
         actionButton = Components.UIButton( "Get Started", Colors.BoschBlue, Fonts.BoschLight.WithSize( 16 ), new UIEdgeInsets( 12, 0, 12, 0 ) );
         actionButton.WithBorder( 1, Colors.BoschBlue );
         actionButton.WithCornerRadius( Values.CornerRadius );
         actionButton.TouchUpInside += HandleActionButtonTouchUpInside;

         View.AddSubview( actionButton );

         actionButton.Anchor( leading: View.LayoutMarginsGuide.LeadingAnchor, bottom: View.LayoutMarginsGuide.BottomAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor );
      }

      private void HandleActionButtonTouchUpInside( object sender, EventArgs e ) => viewModel.ActionSelected( );

      void IOnBoardingViewModel.PresentCameraCalibration( )
      {
         PresentViewController( new CameraCalibrationController( ), animated: true, completionHandler: null );
      }
   }
}
