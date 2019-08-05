using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using SaturdayMP.XPlugins.iOS;
using UIKit;

namespace PK.iOS.Helpers
{
   public class CustomUIView : UIView
   {
      private CGSize intrinsticContentSize;
      public override CGSize IntrinsicContentSize => intrinsticContentSize;

      public CustomUIView( CGSize intrinsticContentSize )
      {
         this.intrinsticContentSize = intrinsticContentSize;
      }
   }

   public class CustomUIImageView : UIImageView
   {
      private CGSize intrinsticContentSize;
      public override CGSize IntrinsicContentSize => intrinsticContentSize;

      public CustomUIImageView( CGSize intrinsticContentSize )
      {
         this.intrinsticContentSize = intrinsticContentSize;
         ContentMode = UIViewContentMode.ScaleAspectFit;
      }
   }

   public class PKButton : UIButton
   {
      public override bool Highlighted 
      { 
         get => base.Highlighted; 
         set
         {
            base.Highlighted = value;

            BackgroundColor = value ? Colors.WhiteWithTransparancy : Colors.Clear;
         }
      }

      public override bool Enabled 
      { 
         get => base.Enabled;
         set
         {
            base.Enabled = value;

            Layer.BorderColor = value ? Colors.White.CGColor : Colors.OuterSpaceLight.CGColor;
            Layer.ShadowColor = value ? Colors.White.CGColor : Colors.OuterSpaceLight.CGColor;
         }
      }

      public PKButton( )
      {
         ContentEdgeInsets = new UIEdgeInsets( 10, 26, 10, 26 );
         BackgroundColor = Colors.Clear;

         TitleLabel.Font = Fonts.Bold.WithSize( 14 );

         Layer.BorderColor = Colors.White.CGColor;
         Layer.BorderWidth = 1;
         Layer.CornerRadius = Values.CornerRadius;
         Layer.ShadowColor = Colors.White.CGColor;
         Layer.ShadowOpacity = 0.4f;
         Layer.ShadowOffset = new CGSize( 0, 0 );
         Layer.ShadowRadius = Values.CornerRadius;
      }
   }

   public class RadialGradientLayer : CALayer
   {
      private readonly CGPoint center;
      private readonly CGColor[ ] colors;

      public RadialGradientLayer( CGPoint center, CGColor[ ] colors )
      {
         this.center = center;
         this.colors = colors;

         NeedsDisplayOnBoundsChange = true;
      }

      public override void DrawInContext( CGContext ctx )
      {
         base.DrawInContext( ctx );

         var locations = new nfloat[ ] { 0, 1 };

         var endRadius = Bounds.Size.Width / 2;

         var gradient = new CGGradient( colorspace: null, colors );

         ctx.DrawRadialGradient(
            gradient: gradient,
            startCenter: center,
            startRadius: 0,
            endCenter: center,
            endRadius: endRadius,
            options: CGGradientDrawingOptions.DrawsAfterEndLocation
         );
      }
   }

   public class EllipseView : UIView
   {
      private readonly string pulseKey = "pulse";
      private bool isPulsating;

      private CAShapeLayer ellipseLayer;
      private CAShapeLayer pulsatingLayer;
      private CABasicAnimation animation;

      public EllipseView( )
      {
         SetupAnimations( );
      }

      private void SetupAnimations( )
      {
         animation = CABasicAnimation.FromKeyPath( "transform.scale" );
         animation.Duration = 1;
         animation.To = NSNumber.FromFloat( 1.2f );
         animation.TimingFunction = CAMediaTimingFunction.FromName( CAMediaTimingFunction.EaseInEaseOut );
         animation.AutoReverses = true;
         animation.RepeatCount = float.PositiveInfinity;
      }

      public override void Draw( CGRect rect )
      {
         ellipseLayer = new CAShapeLayer {
            Frame = rect,
            Path = UIBezierPath.FromOval( inRect: rect ).CGPath,
            FillColor = Colors.OuterSpaceLight.ColorWithAlpha( 0.4f ).CGColor,
         };

         pulsatingLayer = new CAShapeLayer {
            Frame = rect,
            Path = UIBezierPath.FromOval( inRect: rect.Inset( dx: -6, dy: -6 ) ).CGPath,
            FillColor = Colors.OuterSpaceLight.ColorWithAlpha( 0.2f ).CGColor,
         };

         Layer.AddSublayer( ellipseLayer );
      }

      public void BeginPulsating( )
      {
         if( isPulsating )
            return;

         isPulsating = true;
         Layer.AddSublayer( pulsatingLayer );
         pulsatingLayer.AddAnimation( animation, key: pulseKey );
      }

      public void EndPulsating( ) 
      {
         if( !isPulsating )
            return;

         isPulsating = false;
         pulsatingLayer.RemoveAnimation( pulseKey );
         pulsatingLayer.RemoveFromSuperLayer( );
      } 
   }

   public class CircularCheckBox : BEMCheckBox
   {
      public CircularCheckBox( )
      {
         BoxType = BEMBoxType.Circle;
         OnAnimationType = BEMAnimationType.Bounce;
         OffAnimationType = BEMAnimationType.Bounce;
         TintColor = Colors.OuterSpace.ColorWithAlpha( 0.1f );
         OnTintColor = Colors.OuterSpace;
         OnCheckColor = Colors.OuterSpace;
         OnFillColor = Colors.Clear;
         OffFillColor = Colors.Clear;
         LineWidth = 1;
      }
   }
}
