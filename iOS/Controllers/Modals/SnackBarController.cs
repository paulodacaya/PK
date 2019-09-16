using System;
using CoreGraphics;
using PK.iOS.Helpers;
using static PK.iOS.Helpers.Stacks;
using UIKit;

namespace PK.iOS.Controllers
{
   public class SnackBarController : UIViewController
   {
      public event EventHandler OnActionTouchUpInside;

      private UIImage iconImage;
      public UIImage IconImage
      {
         get => iconImage;
         set
         {
            iconImage = value;

            if( iconImageView != null )
            {
               iconImageView.Image = iconImage;
               iconImageView.Hidden = iconImage == null;
            }
         }
      }

      private UIColor iconTintColor = Colors.BoschLightRed;
      public UIColor IconTintColor
      {
         get => iconTintColor;
         set
         {
            iconTintColor = value;

            if( iconImageView != null )
            {
               iconImageView.TintColor = iconTintColor;
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

      private string actionText;
      public string ActionText
      {
         get => actionText;
         set
         {
            actionText = value;

            if( actionButton != null )
            {
               actionButton.SetTitle( actionText.ToUpper( ), UIControlState.Normal );
               actionButton.Hidden = string.IsNullOrEmpty( actionText );
            }
         }
      }

      // UI Elements
      private UIImageView iconImageView;
      private UILabel messageLabel;
      private UIButton actionButton;

      public SnackBarController( )
      {
         ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
         ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupViews( );
      }

      private void SetupViews( )
      {
         View.BackgroundColor = Colors.Clear;

         var containerView = new UIView {
            BackgroundColor = Colors.White
         };
         containerView.Layer.CornerRadius = Values.DialogCornerRadius;
         containerView.Layer.ShadowColor = Colors.Black.CGColor;
         containerView.Layer.ShadowOffset = new CGSize( 0, 2 );
         containerView.Layer.ShadowOpacity = 0.6f;

         iconImageView = Components.UIImageView( iconImage, iconTintColor );
         iconImageView.Hidden = iconImage == null;

         messageLabel = new UILabel {
            Text = messageText,
            Font = Fonts.Medium.WithSize( 14f ),
            TextColor = Colors.BoschBlue,
            Lines = 0,
            Hidden = string.IsNullOrEmpty( messageText )
         };

         actionButton = new UIButton {
            BackgroundColor = Colors.Clear,
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
            Hidden = string.IsNullOrEmpty( actionText ),
         };
         actionButton.TitleLabel.Font = Fonts.Bold.WithSize( 14f );
         actionButton.SetTitle( actionText.ToUpper( ), UIControlState.Normal );
         actionButton.SetTitleColor( Colors.OuterSpace, UIControlState.Normal );
         actionButton.WidthAnchor.ConstraintEqualTo( 50 ).Active = true;
         actionButton.TouchUpInside += ActionButtonTouchUpInside;

         var stackView = HStack(
            iconImageView.WithSquareSize( 26 ),
            messageLabel,
            actionButton
         ).With( spacing: 12, alignment: UIStackViewAlignment.Center ).WithPadding( new UIEdgeInsets( 10, 12, 10, 12 ) );

         View.AddSubview( containerView );

         var bottomPadding = UIApplication.SharedApplication.StatusBarFrame.Height;

         containerView.Anchor( bottom: View.SafeAreaLayoutGuide.BottomAnchor, leading: View.LayoutMarginsGuide.LeadingAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor, padding: new UIEdgeInsets( 0, 0, bottomPadding, 0 ) );

         containerView.AddSubview( stackView );

         stackView.FillSuperview( );
      }

      private void ActionButtonTouchUpInside( object sender, EventArgs e )
      {
         DismissViewController( animated: true, completionHandler: ( ) => {
            OnActionTouchUpInside?.Invoke( this, e );
         } );
      }
   }
}

