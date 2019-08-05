using System;
using CoreGraphics;
using PK.iOS.Helpers;
using UIKit;

namespace PK.iOS.Views
{
   public class HomeSectionItemView : UIView
   {
      private bool isLocked;
      public bool IsLocked
      {
         get => isLocked;
         set
         {
            isLocked = value;

            iconImageView.Image = isLocked ? Images.PadlockLock : Images.PadlockUnlock;
            textLabel.Text = isLocked ? "Locked" : "Unlocked";
         }
      }

      // UI Elements
      private UIView containerView;
      private CustomUIImageView iconImageView;
      private UILabel textLabel;

      public HomeSectionItemView( )
      {
         SetupViews( );
      }

      private void SetupViews( )
      {
         var longPressGesture = new UILongPressGestureRecognizer( HandleLongPressGesture ) {
            MinimumPressDuration = 0,
         };

         containerView = new UIView { 
            UserInteractionEnabled = true,
         };
         containerView.Layer.BorderColor = Colors.White.CGColor;
         containerView.Layer.BorderWidth = 1;
         containerView.AddGestureRecognizer( longPressGesture );

         iconImageView = new CustomUIImageView( new CGSize( 24, 24 ) ) {
            Image = isLocked ? Images.PadlockLock : Images.PadlockUnlock,
            TintColor = Colors.White,
         };

         textLabel = new UILabel {
            Text = isLocked ? "Locked" : "Unlocked",
            TextColor = Colors.White,
            Font = Fonts.Regular.WithSize( 13 ),
         };

         var iconArrangedViews = new UIView[ ] { iconImageView, new CustomUIView( new CGSize( 5, 0 ) ) };
         var iconStackView = new UIStackView( views: iconArrangedViews ); 

         var arrangedViews = new UIView[ ] { iconStackView, textLabel };
         var stackView = new UIStackView( views: arrangedViews ) {
            Axis = UILayoutConstraintAxis.Vertical,
            Spacing = 4,
         };

         AddSubview( containerView );
         containerView.CenterInSuperView( size: new CGSize( 78, 78 ) );

         containerView.AddSubview( stackView );
         stackView.CenterInSuperView( );
      }

      public override void LayoutSubviews( )
      {
         base.LayoutSubviews( );

         containerView.Layer.CornerRadius = containerView.Frame.Height / 2;
      }

      #region Event Handlers
      private void HandleLongPressGesture( UILongPressGestureRecognizer gesture )
      {
         if( gesture.State == UIGestureRecognizerState.Began )
         {
            containerView.BackgroundColor = Colors.WhiteWithTransparancy;
         }
         else if( gesture.State == UIGestureRecognizerState.Ended )
         {
            containerView.BackgroundColor = Colors.Clear;
            IsLocked = !isLocked;
         }
      }
      #endregion
   }
}