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

         var endRadius = Bounds.Size.Width / 1.5f;

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

   public class CircularCheckBox : BEMCheckBox
   {
      public CircularCheckBox( )
      {
         BoxType = BEMBoxType.Circle;
         OnAnimationType = BEMAnimationType.Bounce;
         OffAnimationType = BEMAnimationType.Bounce;
         TintColor = Colors.White.ColorWithAlpha( 0.1f );
         OnTintColor = Colors.White;
         OnCheckColor = Colors.White;
         OnFillColor = Colors.Clear;
         OffFillColor = Colors.Clear;
         LineWidth = 1;
      }
   }

   public class PKLabel : UILabel
   {
      public PKLabel( string text, UIColor textColor, UIFont font, int lines = 0 )
      {
         Text = text;
         TextColor = textColor;
         Font = font;
         Lines = lines;
      }
   }

   public class PKNumberView: UIView
   {
      public PKNumberView( string number )
      {
         Layer.BorderWidth = 1.2f;
         Layer.BorderColor = Colors.White.CGColor;

         var numberLabel = new UILabel {
            Text = number,
            TextColor = Colors.White,
            Font = Fonts.Bold.WithSize( 18 )
         };

         AddSubview( numberLabel );

         numberLabel.CenterInSuperView( );
      }
   }

   public class PKImageView: UIImageView
   {
      public PKImageView( UIImage image, UIColor tintColor, UIViewContentMode contentMode = UIViewContentMode.ScaleAspectFit )
      {
         Image = image;
         TintColor = tintColor;
         ContentMode = contentMode;
      }
   }
}
