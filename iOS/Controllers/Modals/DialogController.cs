using CoreGraphics;
using PK.iOS.Helpers;
using static PK.iOS.Helpers.Stacks;
using System;
using UIKit;

namespace PK.iOS.Controllers
{
   public class DialogController : UIViewController
   {
      public event EventHandler OnNegativeButtonTouchUpInside;
      public event EventHandler OnPositiveButtonTouchUpInside;

      private readonly bool prominent;

      private string titleText;
      public string TitleText
      {
         get => titleText;
         set
         {
            titleText = value;

            if( titleLabel != null )
            {
               titleLabel.Text = messageText;
               titleLabel.Hidden = string.IsNullOrEmpty( messageText );
            }
         }
      }

      private string messageText;
      public string MessageText
      {
         get => messageText;
         set
         {
            messageText = value;

            if( messageLabel != null )
            {
               messageLabel.Text = messageText;
               messageLabel.Hidden = string.IsNullOrEmpty( messageText );
            }
         } 
      }

      private string negativeText = "Cancel";
      public string NegativeText
      {
         get => negativeText;
         set
         {
            negativeText = value;

            negativeButton?.SetTitle( negativeText, UIControlState.Normal );
         }
      }

      private string positiveText = "Ok";
      public string PositiveText
      {
         get => negativeText;
         set
         {
            positiveText = value;

            positiveButton?.SetTitle( positiveText, UIControlState.Normal );
         }
      }

      public bool NegativeButtonHidden
      {
         get => negativeButton.Hidden;
         set
         {
            if( negativeButton != null )
               negativeButton.Hidden = value;
         } 
      }

      // UI Elements
      private UILabel titleLabel;
      private UILabel messageLabel;
      private UIButton negativeButton;
      private UIButton positiveButton;

      public DialogController( bool prominent = true )
      {
         this.prominent = prominent;

         ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
         ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupBackground( );
         SetupViews( );
      }

      private void SetupBackground( )
      {
         if( !prominent )
         {
            View.BackgroundColor = Colors.Black.ColorWithAlpha( 0.2f );
            return;
         }

         var blurEffect = UIBlurEffect.FromStyle( UIBlurEffectStyle.Dark );
         var visualEffectView = new UIVisualEffectView( effect: blurEffect );

         View.AddSubview( visualEffectView );

         visualEffectView.FillSuperview( );
      }

      private void SetupViews( )
      {
         var containerView = new UIView {
            BackgroundColor = Colors.OuterSpaceLight,
            ClipsToBounds = true,
         };
         containerView.Layer.CornerRadius = Values.DialogCornerRadius;

         titleLabel = new UILabel {
            Text = titleText,
            Font = Fonts.Bold.WithSize( 22f ),
            TextColor = Colors.White,
            Hidden = string.IsNullOrEmpty( messageText )
         };

         messageLabel = new UILabel {
            Text = messageText,
            Font = Fonts.Regular.WithSize( 16f ),
            TextColor = Colors.Gray,
            Lines = 0,
            Hidden = string.IsNullOrEmpty( messageText )
         };

         negativeButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ) 
         };
         negativeButton.TitleLabel.Font = Fonts.Bold.WithSize( 16f );
         negativeButton.SetTitle( negativeText, UIControlState.Normal );
         negativeButton.SetTitleColor( Colors.White, UIControlState.Normal );
         negativeButton.TouchUpInside += NegativeButtonTouchUpInside;

         positiveButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
         };
         positiveButton.TitleLabel.Font = Fonts.Bold.WithSize( 16f );
         positiveButton.SetTitle( positiveText.ToUpper( ), UIControlState.Normal );
         positiveButton.SetTitleColor( Colors.White, UIControlState.Normal );
         positiveButton.TouchUpInside += PostiveButtonTouchUpInside;

         var stackView = VStack(
            titleLabel,
            messageLabel,
            new CustomUIView( new CGSize( 0, 18 ) ),
            HStack( new UIView( ), negativeButton, positiveButton ).With( spacing: 24 )
         ).With( spacing: 12 ).WithPadding( new UIEdgeInsets( 24, 24, 11, 24 ) );

         View.AddSubview( containerView );

         containerView.Anchor( centerX: View.CenterXAnchor, centerY: View.CenterYAnchor,
            size: new CGSize( width: Math.Min( UIScreen.MainScreen.Bounds.Width - 80, 500 ), height: 0 ) );

         containerView.AddSubviews( stackView );

         stackView.FillSuperview( );
      }

      private void PostiveButtonTouchUpInside( object sender, EventArgs e )
      {
         DismissViewController( animated: true, completionHandler: ( ) => {
            OnPositiveButtonTouchUpInside?.Invoke( sender, e );
         } );
      }

      private void NegativeButtonTouchUpInside( object sender, EventArgs e )
      {
         DismissViewController( animated: true, completionHandler: ( ) => {
            OnNegativeButtonTouchUpInside?.Invoke( sender, e );
         } );
      }
   }
}
