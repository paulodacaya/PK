using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace PK.iOS.Helpers
{
   /// <summary>
   /// UIView+NSLayoutConstraint
   /// </summary>
   public class AnchoredConstraints
   {
      public NSLayoutConstraint Top, Leading, Bottom, Trailing, CenterX, CenterY, Width, Height;
   }

   public static class Anchors
   {
      public static AnchoredConstraints Anchor( this UIView view, NSLayoutYAxisAnchor top = null, NSLayoutXAxisAnchor leading = null, NSLayoutYAxisAnchor bottom = null,
         NSLayoutXAxisAnchor trailing = null, NSLayoutXAxisAnchor centerX = null, NSLayoutYAxisAnchor centerY = null, 
         UIEdgeInsets padding = default, UIOffset offset = default, CGSize size = default )
      {
         view.TranslatesAutoresizingMaskIntoConstraints = false;

         var anchoredConstraints = new AnchoredConstraints( );

         if( top != null )   
         {
            anchoredConstraints.Top = view.TopAnchor.ConstraintEqualTo( anchor: top, constant: padding.Top );
            anchoredConstraints.Top.Active = true;
         }

         if( leading != null )
         {
            anchoredConstraints.Leading = view.LeadingAnchor.ConstraintEqualTo( anchor: leading, constant: padding.Left );
            anchoredConstraints.Leading.Active = true;
         }

         if( bottom != null )
         {
            anchoredConstraints.Bottom = view.BottomAnchor.ConstraintEqualTo( anchor: bottom, constant: -padding.Bottom );
            anchoredConstraints.Bottom.Active = true;
         }

         if( trailing != null )
         {
            anchoredConstraints.Trailing = view.TrailingAnchor.ConstraintEqualTo( anchor: trailing, constant: -padding.Right );
            anchoredConstraints.Trailing.Active = true;
         }

         if( centerX != null )
         {
            anchoredConstraints.CenterX = view.CenterXAnchor.ConstraintEqualTo( anchor: centerX, constant: offset.Horizontal );
            anchoredConstraints.CenterX.Active = true;
         }

         if( centerY != null )
         {
            anchoredConstraints.CenterY = view.CenterYAnchor.ConstraintEqualTo( anchor: centerY, constant: offset.Vertical );
            anchoredConstraints.CenterY.Active = true;
         }

         if( size.Width > 0 )
         {
            anchoredConstraints.Width = view.WidthAnchor.ConstraintEqualTo( size.Width );
            anchoredConstraints.Width.Active = true;
         }

         if( size.Height > 0 )
         {
            anchoredConstraints.Height = view.HeightAnchor.ConstraintEqualTo( size.Height );
            anchoredConstraints.Height.Active = true;
         }

         return anchoredConstraints;
      }

      public static AnchoredConstraints FillSuperview( this UIView view, UIEdgeInsets padding = default )
      {
         return view.Anchor( top: view.Superview.TopAnchor, leading: view.Superview.LeadingAnchor, bottom: view.Superview.BottomAnchor, 
            trailing: view.Superview.TrailingAnchor, padding: padding );
      }

      public static AnchoredConstraints FillLayoutMarginsGuideSuperView( this UIView view, UIEdgeInsets padding = default )
      {
         return view.Anchor( top: view.Superview.LayoutMarginsGuide.TopAnchor, leading: view.Superview.LayoutMarginsGuide.LeadingAnchor, 
            bottom: view.Superview.LayoutMarginsGuide.BottomAnchor, trailing: view.Superview.LayoutMarginsGuide.TrailingAnchor, padding: padding );
      }

      public static AnchoredConstraints CenterInSuperView( this UIView view, CGSize size = default, UIOffset offset = default )
      {
         return view.Anchor( centerX: view.Superview.CenterXAnchor, centerY: view.Superview.CenterYAnchor, size: size, offset: offset );
      }

      public static AnchoredConstraints ConstraintWidthEqualTo( this UIView view, nfloat constant )
      {
         return view.Anchor( size: new CGSize( constant, 0 ) );
      }

      public static AnchoredConstraints ConstraintHeightEqualTo( this UIView view, nfloat constant )
      {
         return view.Anchor( size: new CGSize( 0, constant ) );
      }
   }

   /// <summary>
   /// UIView+UIStackView
   /// </summary>
   public static class Stacks
   {
      private static UIStackView Stack( UIView[ ] views, UILayoutConstraintAxis axis )
      {
         var stackView = new UIStackView( views: views ) {
            Axis = axis,
         };

         return stackView;
      }

      public static UIStackView HStack( params UIView[ ] views )
      {
         return Stack( views: views, axis: UILayoutConstraintAxis.Horizontal );
      }

      public static UIStackView VStack( params UIView[ ] views )
      {
         return Stack( views: views, axis: UILayoutConstraintAxis.Vertical );
      }

      public static UIStackView With( this UIStackView stackView, nfloat spacing = default, UIStackViewAlignment alignment = default, UIStackViewDistribution distribution = default )
      {
         stackView.Spacing = spacing;
         stackView.Alignment = alignment;
         stackView.Distribution = distribution;

         return stackView;
      }

      public static UIStackView WithPadding( this UIStackView stackView, UIEdgeInsets padding )
      {
         stackView.LayoutMarginsRelativeArrangement = true;
         stackView.LayoutMargins = padding;
         return stackView;
      }

      public static UIStackView PaddingLeft( this UIStackView stackView, nfloat left )
      {
         stackView.LayoutMarginsRelativeArrangement = true;
         stackView.LayoutMargins = new UIEdgeInsets( stackView.LayoutMargins.Top, left, stackView.LayoutMargins.Bottom, stackView.LayoutMargins.Right );
         return stackView;
      }

      public static UIStackView PaddingTop( this UIStackView stackView, nfloat top )
      {
         stackView.LayoutMarginsRelativeArrangement = true;
         stackView.LayoutMargins = new UIEdgeInsets( top, stackView.LayoutMargins.Left, stackView.LayoutMargins.Bottom, stackView.LayoutMargins.Right );
         return stackView;
      }

      public static UIStackView PaddingBottom( this UIStackView stackView, nfloat bottom )
      {
         stackView.LayoutMarginsRelativeArrangement = true;
         stackView.LayoutMargins = new UIEdgeInsets( stackView.LayoutMargins.Top, stackView.LayoutMargins.Left, bottom, stackView.LayoutMargins.Right );
         return stackView;
      }

      public static UIStackView PaddingRight( this UIStackView stackView, nfloat right )
      {
         stackView.LayoutMarginsRelativeArrangement = true;
         stackView.LayoutMargins = new UIEdgeInsets( stackView.LayoutMargins.Top, stackView.LayoutMargins.Left, stackView.LayoutMargins.Bottom, right );
         return stackView;
      }

      public static UIView WithSize( this UIView view, CGSize size )
      {
         view.TranslatesAutoresizingMaskIntoConstraints = false;
         view.WidthAnchor.ConstraintEqualTo( size.Width ).Active = true;
         view.HeightAnchor.ConstraintEqualTo( size.Height ).Active = true;

         return view;
      }

      public static UIView WithSquareSize( this UIView view, nfloat size )
      {
         view.TranslatesAutoresizingMaskIntoConstraints = false;
         view.WidthAnchor.ConstraintEqualTo( size ).Active = true;
         view.HeightAnchor.ConstraintEqualTo( size ).Active = true;

         return view;
      }

      public static UIView WithWidth( this UIView view, nfloat width )
      {
         view.TranslatesAutoresizingMaskIntoConstraints = false;
         view.WidthAnchor.ConstraintEqualTo( width ).Active = true;

         return view;
      }

      public static UIView WithHeight( this UIView view, nfloat height )
      {
         view.TranslatesAutoresizingMaskIntoConstraints = false;
         view.HeightAnchor.ConstraintEqualTo( height ).Active = true;

         return view;
      }
   }

   /// <summary>
   /// UIView
   /// </summary>
   public static class Views
   {
      public static UIView WithCornerRadius( this UIView view, nfloat radius )
      {
         view.Layer.CornerRadius = radius;

         return view;
      }

      // Custom radius for selected corners
      public static void CornerRadius( this UIView view, UIRectCorner corners, nfloat radius )
      {
         var path = UIBezierPath.FromRoundedRect( view.Bounds, corners, new CGSize( radius, radius ) );

         var maskLayer = new CAShapeLayer {
            Path = path.CGPath
         };

         view.Layer.Mask = maskLayer;
      }

      public static UIView WithBorder( this UIView view, nfloat width, UIColor color )
      {
         view.Layer.BorderWidth = width;
         view.Layer.BorderColor = color.CGColor;

         return view;
      }

      public static UIView WithShadow( this UIView view, float opacity = default, nfloat radius = default, CGSize offset = default, UIColor color = default )
      {
         view.Layer.ShadowOpacity = opacity;
         view.Layer.ShadowRadius = radius;
         view.Layer.ShadowOffset = offset;
         view.Layer.ShadowColor = color.CGColor;

         return view;
      }
   }

   /// <summary>
   /// UIEdgeInsets
   /// </summary>
   public static class EdgeInsets
   {
      public static UIEdgeInsets AllSides( this UIEdgeInsets edgeInsets, nfloat side )
      {
         return new UIEdgeInsets( side, side, side, side );
      }
   }

   /// <summary>
   /// UILabel
   /// </summary>
   public static class Labels
   {
      public static void SetAttributedLineSpaceText( this UILabel label, string text, UIColor textColor, UIFont textFont, nfloat lineSpacing, UITextAlignment textAlignment = UITextAlignment.Left )
      {
         var attributedString = new UIStringAttributes {
            Font = textFont,
            ForegroundColor = textColor,
            ParagraphStyle = new NSMutableParagraphStyle { LineSpacing = lineSpacing }
         };

         var attributedText = new NSMutableAttributedString( text );
         attributedText.AddAttributes( attributedString, new NSRange( 0, text.Length ) );

         label.AttributedText = attributedText;
         label.Lines = 0;
         label.TextAlignment = textAlignment;
      }
   }
}
