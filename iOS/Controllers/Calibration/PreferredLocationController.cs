using System;
using CoreGraphics;
using PK.iOS.Helpers;
using PK.ViewModels;
using static PK.iOS.Helpers.Stacks;
using UIKit;
using CoreAnimation;
using Foundation;
using System.Timers;

namespace PK.iOS.Controllers
{
   public class PreferredLocationController : UIViewController
   {
      private readonly ManualCalibrationViewModel viewModel;
      private static Timer counterTimer;
      private int counter;
      private readonly int INITIAL_COUNTDOWN = 5;

      private UIView containerView;
      private nfloat containerViewHeight;
      private AnchoredConstraints containerAnchoredConstraints;
      private UILabel counterLabel;
      private CircularCheckBox checkbox;
      private UIButton actionButton;
      private UIButton doneButton;

      public PreferredLocationController( ManualCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;

         ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
         ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;

         SetupTimer( );
      }

      private void SetupTimer( )
      {
         counterTimer = new Timer {
            Interval = 1000,
            AutoReset = true,
         };
         counterTimer.Elapsed += HandleTimerElapsed;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupViews( );
      }

      public override void ViewDidAppear( bool animated )
      {
         base.ViewDidAppear( animated );

         PresentPopUp( );
      }

      public override void ViewDidLayoutSubviews( )
      {
         base.ViewDidLayoutSubviews( );

         containerView.CornerRadius( UIRectCorner.TopLeft | UIRectCorner.TopRight, Values.DialogCornerRadius );
      }

      private void SetupViews( )
      {
         containerViewHeight = View.Frame.Height * 0.5f;

         containerView = new UIView {
            BackgroundColor = Colors.White,
         };

         var title = new UILabel {
            Text = "Preferred Location",
            TextColor = Colors.OuterSpace,
            TextAlignment = UITextAlignment.Center,
            Font = Fonts.Bold.WithSize( 18 )
         };

         var subTitle = new UILabel( );
         subTitle.SetAttributedLineSpaceText(
            text: "Press 'Go', then place your device in your most common location. This may be your handbag, pocket or in hand. This improves calibration.",
            textColor: Colors.AuroMetalSaurus,
            textFont: Fonts.Medium.WithSize( 14 ),
            lineSpacing: 3.8f,
            textAlignment: UITextAlignment.Center
         );

         var blockView1 = CreateImageBlock( image: Images.ShoppingBag, Colors.BoschLightRed, Colors.BoschDarkRed ).WithHeight( 80 );
         var blockView2 = CreateImageBlock( image: Images.Pants, Colors.BoschLightBlue, Colors.BoschDarkBlue ).WithHeight( 80 );
         var blockView3 = CreateImageBlock( image: Images.Man, Colors.BoschLightPurple, Colors.BoschDarkPurple ).WithHeight( 80 );

         var counterAndcheckboxContainer = new UIView( );

         counterLabel = new UILabel {
            Text = INITIAL_COUNTDOWN.ToString( ),
            TextColor = Colors.OuterSpace,
            TextAlignment = UITextAlignment.Center,
            Font = Fonts.Bold.WithSize( 32 ),
            Hidden = true,
         };

         checkbox = new CircularCheckBox {
            Hidden = true,
         };
         checkbox.WithSize( new CGSize( 42, 42 ) );

         counterAndcheckboxContainer.AddSubviews( counterLabel, checkbox );
         counterLabel.CenterInSuperView( );
         checkbox.CenterInSuperView( );

         var stackView = VStack(
            title,
            subTitle,
            HStack( blockView1, blockView2, blockView3 ).With( spacing: 18, distribution: UIStackViewDistribution.FillEqually ),
            counterAndcheckboxContainer
         ).With( spacing: 14 ).PaddingTop( 20 ).PaddingBottom( 20 );

         actionButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 10, 32, 10, 32 ),
            BackgroundColor = Colors.LightGray,
         };
         actionButton.TitleLabel.Font = Fonts.Bold.WithSize( 18 );
         actionButton.SetTitle( "Go", UIControlState.Normal );
         actionButton.SetTitleColor( Colors.OuterSpace, UIControlState.Normal );
         actionButton.SetTitleColor( Colors.DarkGray, UIControlState.Disabled );
         actionButton.WithCornerRadius( Values.DialogCornerRadius );
         actionButton.TouchUpInside += HandleActionButtonTouchUpInside;

         doneButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 10, 32, 10, 32 ),
            BackgroundColor = Colors.BoschLightGreen.ColorWithAlpha( 0.2f ),
            Hidden = true,
         };
         doneButton.TitleLabel.Font = Fonts.Bold.WithSize( 18 );
         doneButton.SetTitle( "Done", UIControlState.Normal );
         doneButton.SetTitleColor( Colors.BoschDarkGreen, UIControlState.Normal );
         doneButton.SetTitleColor( Colors.BoschDarkGreen.ColorWithAlpha( 0.2f ), UIControlState.Disabled );
         doneButton.WithCornerRadius( Values.DialogCornerRadius );
         doneButton.TouchUpInside += HandleDoneButtonTouchUpInside;

         var buttonStackView = HStack( actionButton, doneButton ).With( spacing: 12 );

         View.AddSubview( containerView );

         containerAnchoredConstraints = containerView.Anchor( leading: View.LeadingAnchor, bottom: View.BottomAnchor,
            trailing: View.TrailingAnchor, size: new CGSize( 0, containerViewHeight ),
            padding: new UIEdgeInsets( 0, 0, -containerViewHeight, 0 ) );

         containerView.AddSubviews( buttonStackView, stackView );

         buttonStackView.Anchor( centerX: containerView.CenterXAnchor, bottom: containerView.BottomAnchor, padding: new UIEdgeInsets( 0, 0, 28, 0 ) );

         stackView.Anchor( top: containerView.TopAnchor, leading: View.LayoutMarginsGuide.LeadingAnchor,
            bottom: actionButton.TopAnchor, trailing: View.LayoutMarginsGuide.TrailingAnchor );
      }

      private UIView CreateImageBlock( UIImage image, UIColor gradientColor1, UIColor gradientColor2 )
      {
         var blockView = new BlockView( gradientColor1, gradientColor2 );

         var imageView = new UIImageView {
            Image = image,
            TintColor = Colors.White,
            ContentMode = UIViewContentMode.ScaleAspectFit
         };

         blockView.AddSubview( imageView );

         imageView.CenterInSuperView( );
         imageView.HeightAnchor.ConstraintEqualTo( blockView.HeightAnchor, multiplier: 0.6f ).Active = true;

         return blockView;
      }

      #region Event handlers
      private void HandleActionButtonTouchUpInside( object sender, EventArgs e )
      {
         counter = INITIAL_COUNTDOWN;
         counterLabel.Text = counter.ToString( );

         counterLabel.Hidden = false;
         checkbox.Hidden = true;
         checkbox.On = false;
         actionButton.Enabled = false;
         doneButton.Enabled = false;

         AnimateCounterLabel( );

         counterTimer.Start( );
      }

      private void HandleDoneButtonTouchUpInside( object sender, EventArgs e )
      {
         DismissPopUp( );
      }

      private void HandleTimerElapsed( object sender, ElapsedEventArgs e )
      {
         counter--;

         if( counter == 0 )
         {
            try
            {
               counterTimer.Stop( );

               InvokeOnMainThread( ( ) => {
                  actionButton.SetTitle( "Try Again", UIControlState.Normal );
                  actionButton.Enabled = true;

                  counterLabel.Hidden = true;
                  checkbox.Hidden = false;
                  checkbox.SetOn( true, animated: true );

                  doneButton.Hidden = false;
                  doneButton.Enabled = true;
               } );
            }
            catch( Exception )
            {
               throw new Exception( message: "Failed to stop/dispose of timer." );
            }
         }

         InvokeOnMainThread( ( ) => {
            counterLabel.Text = counter.ToString( );
            AnimateCounterLabel( );
         } );
      }
      #endregion

      private void PresentPopUp( )
      {
         UIView.AnimateNotify( duration: 0.3, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {

            View.BackgroundColor = Colors.Black.ColorWithAlpha( 0.2f );
            containerAnchoredConstraints.Bottom.Constant = 0;

            View.LayoutIfNeeded( );

         }, completion: null );
      }

      private void DismissPopUp( )
      {
         UIView.AnimateNotify( duration: 0.3, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {

            View.BackgroundColor = Colors.Clear;
            containerAnchoredConstraints.Bottom.Constant = containerViewHeight;

            View.LayoutIfNeeded( );

         }, completion: ( bool finished ) => {

            DismissViewController( animated: false, completionHandler: null );

         } );
      }

      private void AnimateCounterLabel( )
      {
         if( !counterLabel.Transform.IsIdentity )
            counterLabel.Transform = CGAffineTransform.MakeIdentity( );

         UIView.AnimateNotify( duration: 0.3, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, animation: ( ) => {

            counterLabel.Transform = CGAffineTransform.MakeScale( sx: 1.4f, sy: 1.4f );

         }, completion: null );
      }

      private class BlockView : UIView
      {
         private readonly UIColor gradientColor1;
         private readonly UIColor gradientColor2;

         private CAGradientLayer gradientLayer;

         public BlockView( UIColor gradientColor1, UIColor gradientColor2 )
         {
            this.gradientColor1 = gradientColor1;
            this.gradientColor2 = gradientColor2;

            ClipsToBounds = true;
            Layer.CornerRadius = Values.DialogCornerRadius;

            SetupGradient( );
         }

         private void SetupGradient( )
         {
            gradientLayer = new CAGradientLayer {
               Colors = new CGColor[ ] { gradientColor1.CGColor, gradientColor2.CGColor },
               Locations = new NSNumber[ ] { 0, 1 },
            };

            Layer.AddSublayer( gradientLayer );
         }

         public override void LayoutSubviews( )
         {
            base.LayoutSubviews( );

            gradientLayer.Frame = Bounds;
         }
      }
   }
}
