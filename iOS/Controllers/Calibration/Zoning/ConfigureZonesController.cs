using System;
using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using PK.iOS.Helpers;
using PK.iOS.Views;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class ConfigureZonesController : UIViewController, IConfigureZonesViewModel
   {
      private readonly ConfigureZonesViewModel viewModel;
      private readonly List<ZoneView> zoneViews;

      // UI Elements
      private UIView zoneContainerView;
      private UIImageView carImageView;
      private ZoneSelectionController zoneSelectionController;
      private UISlider slider;
      private UILabel minDistanceLabel;
      private UILabel distanceLabel;
      private UILabel maxDistanceLabel;

      public ConfigureZonesController( )
      {
         viewModel = new ConfigureZonesViewModel( this );
         zoneViews = new List<ZoneView>( );
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupNavigation( );
         SetupViews( );
         SetupZoneViews( );
         SetupZoneSelection( );
         SetupSliderAndActionButton( );
         SetupDistanceViews( );

         viewModel.ZoneModels[ 0 ].Selected( ); // Select Welcome Zone
      }

      private void SetupNavigation( )
      {
         NavigationController.SetNavigationBarHidden( true, animated: false );
      }

      private void SetupViews( )
      {
         View.BackgroundColor = Colors.OuterSpace;
      }

      private void SetupZoneViews( )
      {
         var titleLabel = new PKLabel( viewModel.Title, Colors.White, Fonts.Bold.WithSize( 24 ) );
         var subTitleLabel = new PKLabel( viewModel.SubTitle, Colors.Gray, Fonts.Medium.WithSize( 18 ) );

         var stackView = VStack(
            titleLabel,
            subTitleLabel
         ).With( spacing: 4 );

         View.AddSubview( stackView );

         stackView.Anchor( leading: View.LayoutMarginsGuide.LeadingAnchor, top: View.LayoutMarginsGuide.TopAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor );
            
         zoneContainerView = new UIView( );

         carImageView = new UIImageView {
            UserInteractionEnabled = true,
            Image = Images.Mazda3TopView,
            ContentMode = UIViewContentMode.ScaleAspectFit
         };
         var longPressGesture = new UILongPressGestureRecognizer( HandleCarLongPress ) { 
            MinimumPressDuration = 0
         };
         carImageView.AddGestureRecognizer( longPressGesture );

         var welcomeZoneView = new CircularZoneView( ZoneType.Welcome, Colors.BoschLightGreen.ColorWithAlpha( 0.4f ), Colors.BoschDarkGreen.ColorWithAlpha( 0.4f ) );
         var driverZoneView = new SemiCircularZoneView( ZoneType.Driver, SemiCircularZoneView.ArcDirection.Right, Colors.BoschLightRed.ColorWithAlpha( 0.4f ), Colors.BoschDarkRed.ColorWithAlpha( 0.4f ) );
         var passengerZoneView = new SemiCircularZoneView( ZoneType.Passenger, SemiCircularZoneView.ArcDirection.Left, Colors.BoschLightRed.ColorWithAlpha( 0.4f ), Colors.BoschDarkRed.ColorWithAlpha( 0.4f ) );
         var bootZoneView = new SemiCircularZoneView( ZoneType.Boot, SemiCircularZoneView.ArcDirection.Bottom, Colors.BoschLightRed.ColorWithAlpha( 0.4f ), Colors.BoschDarkRed.ColorWithAlpha( 0.4f ) );
          
         zoneViews.Add( welcomeZoneView );
         zoneViews.Add( driverZoneView );
         zoneViews.Add( passengerZoneView );
         zoneViews.Add( bootZoneView );

         // Update distance text with model
         for( int i = 0; i < zoneViews.Count; i++ )
            zoneViews[ i ].DistanceText = $"{viewModel.ZoneModels[ i ].Distance}m";

         View.AddSubview( zoneContainerView );

         var zoneContainerViewHeight = UIScreen.MainScreen.Bounds.Height * 0.425f;
         zoneContainerView.Anchor( leading: View.LeadingAnchor, top: stackView.BottomAnchor,
            trailing: View.TrailingAnchor, size: new CGSize( 0, zoneContainerViewHeight ) );

         zoneContainerView.AddSubviews( welcomeZoneView, driverZoneView, passengerZoneView, bootZoneView, carImageView );

         welcomeZoneView.MaxSize = zoneContainerViewHeight - 32;
         welcomeZoneView.MinSize = zoneContainerViewHeight - 82;
         welcomeZoneView.CenterInSuperView( size: new CGSize( welcomeZoneView.Size, welcomeZoneView.Size ) );

         driverZoneView.MaxSize = 70;
         driverZoneView.MinSize = 64;
         driverZoneView.CenterInSuperView( size: new CGSize( driverZoneView.Size, driverZoneView.Size ), offset: new UIOffset( 36, 0 ) );

         passengerZoneView.MaxSize = 70;
         passengerZoneView.MinSize = 64;
         passengerZoneView.CenterInSuperView( size: new CGSize( passengerZoneView.Size, passengerZoneView.Size ), offset: new UIOffset( -36, 0 ) );

         bootZoneView.MaxSize = 90;
         bootZoneView.MinSize = 78;
         bootZoneView.CenterInSuperView( size: new CGSize( bootZoneView.Size, bootZoneView.Size ), offset: new UIOffset( 0, 70 ) );

         carImageView.CenterInSuperView( size: new CGSize( 120, zoneContainerViewHeight ), offset: new UIOffset( 1, 0 ) );
      }

      private void SetupZoneSelection( )
      {
         zoneSelectionController = new ZoneSelectionController( viewModel.ZoneModels );

         View.AddSubview( zoneSelectionController.View );
         AddChildViewController( zoneSelectionController );

         zoneSelectionController.View.Anchor( top: zoneContainerView.BottomAnchor, leading: View.LeadingAnchor,
            trailing: View.TrailingAnchor, size: new CGSize( 0,  80 ) );
      }

      private void SetupSliderAndActionButton( )
      {
         var actionButton = new UIButton( UIButtonType.System );
         actionButton.SetImage( Images.ChevronRight, UIControlState.Normal );
         actionButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
         actionButton.ImageView.WithSquareSize( 20 );
         actionButton.TintColor = Colors.OuterSpace;
         actionButton.BackgroundColor = Colors.White;
         actionButton.WithSquareSize( 50 );
         actionButton.WithCornerRadius( 25 );
         actionButton.WithShadow( opacity: 0.5f, radius: 2, color: Colors.White );
         actionButton.TouchUpInside += HandleActionButtonTouchUpInside;

         View.AddSubview( actionButton );

         actionButton.Anchor( bottom: View.LayoutMarginsGuide.BottomAnchor, trailing: View.LayoutMarginsGuide.TrailingAnchor,
            padding: new UIEdgeInsets( 0, 0, 14, 0 ) );

         var leftImageView = new UIImageView {
            Image = Images.CircleOutline,
            TintColor = Colors.WhiteWithTransparancy,
         };

         var rightImageView = new UIImageView {
            Image = Images.CircleOutline,
            TintColor = Colors.WhiteWithTransparancy,
         };

         slider = new UISlider {
            ThumbTintColor = Colors.White,
            MinimumTrackTintColor = Colors.LightWhite,
            MaximumTrackTintColor = Colors.LightWhite,
            TintColor = Colors.WhiteWithTransparancy,
         };
         slider.ValueChanged += SliderValueChanged;

         var stackView = HStack(
            leftImageView.WithSquareSize( 12 ),
            slider,
            rightImageView.WithSquareSize( 22 )
         ).With( spacing: 12, alignment: UIStackViewAlignment.Center );

         View.AddSubview( stackView );

         stackView.Anchor( leading: View.LayoutMarginsGuide.LeadingAnchor, bottom: actionButton.TopAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor, padding: new UIEdgeInsets( 0, 0, 24, 0 ) );
      }

      private void SetupDistanceViews( )
      {
         distanceLabel = new UILabel {
            TextColor = Colors.White,
            TextAlignment = UITextAlignment.Center,
            Font = Fonts.Medium.WithSize( 30 )
         };

         minDistanceLabel = new UILabel {
            TextColor = Colors.WhiteWithTransparancy,
            TextAlignment = UITextAlignment.Left,
            Font = Fonts.Medium.WithSize( 20 )
         };

         maxDistanceLabel = new UILabel {
            TextColor = Colors.WhiteWithTransparancy,
            TextAlignment = UITextAlignment.Right,
            Font = Fonts.Medium.WithSize( 20 )
         };

         var stackView = HStack(
            minDistanceLabel,
            distanceLabel,
            maxDistanceLabel
         ).With( distribution: UIStackViewDistribution.FillEqually, alignment: UIStackViewAlignment.Center );

         View.AddSubview( stackView );

         stackView.Anchor( leading: View.LayoutMarginsGuide.LeadingAnchor, top: zoneSelectionController.View.BottomAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor, bottom: slider.TopAnchor );
      }

      private void SliderValueChanged( object sender, EventArgs e ) => UpdateZoneViewSize( );

      private void HandleCarLongPress( UILongPressGestureRecognizer gesture )
      {
         UIView.AnimateNotify( duration: 0.15, animation: ( ) => {

            if( gesture.State == UIGestureRecognizerState.Began )
            {
               carImageView.Transform = CGAffineTransform.MakeScale( sx: 0.98f, sy: 0.98f );
               foreach( var zoneView in zoneViews )
                  zoneView.Alpha = 1;
            }
            else if( gesture.State == UIGestureRecognizerState.Ended )
            {
               carImageView.Transform = CGAffineTransform.MakeIdentity( );
               foreach( var zoneView in zoneViews )
                  zoneView.Alpha = zoneView.ZoneType == viewModel.SelectedZoneType ? 1 : 0;
            }

         }, completion: null );
      }

      private void HandleActionButtonTouchUpInside( object sender, EventArgs e ) => viewModel.ActionSelected( );

      void IConfigureZonesViewModel.ZoneSelected( ZoneType zoneType, double distance, double min, double max )
      {
         UIView.AnimateNotify( duration: 0.15, animation: ( ) => {

            foreach( var zoneView in zoneViews )
            {
               zoneView.Alpha = zoneView.ZoneType == zoneType ? 1 : 0;

               slider.MinValue = ( float )min;
               slider.MaxValue = ( float )max;
               slider.Value = ( float )distance;

               minDistanceLabel.Text = $"{min}m";
               maxDistanceLabel.Text = $"{max}m";
               distanceLabel.Text = $"{distance}m";

               UpdateZoneViewSize( );
            }

         }, completion: null );
      }

      void IConfigureZonesViewModel.NavigateToHome( ) => NavigationController.PopToRootViewController( animated: true );

      private void UpdateZoneViewSize( )
      {
         var zoneView = zoneViews.Single( z => z.ZoneType == viewModel.SelectedZoneType );
         var widthConsraint = zoneView.Constraints.First( c => c.FirstAttribute == NSLayoutAttribute.Width );
         var heightConstraint = zoneView.Constraints.First( c => c.FirstAttribute == NSLayoutAttribute.Height );

         // Get percent if slider value relative to slider min and max. ( ( input - min ) * 100 ) / ( max - min )
         var sliderPercent = ( slider.Value - slider.MinValue ) * 100f / ( slider.MaxValue - slider.MinValue );

         // User the percent of slider to update correct frame size for zone ((percent * (max - min) / 100) + min
         var size = ( sliderPercent * ( zoneView.MaxSize - zoneView.MinSize ) / 100f ) + zoneView.MinSize;

         widthConsraint.Constant = size;
         heightConstraint.Constant = size;
         zoneContainerView.LayoutIfNeeded( );

         // Update displayed values
         var roundedValue = ( float )Math.Round( slider.Value, 1 );
         distanceLabel.Text = $"{roundedValue}m";
         zoneView.DistanceText = $"{roundedValue}m";

         var zoneModel = viewModel.ZoneModels.Single( z => z.ZoneType == viewModel.SelectedZoneType );
         zoneModel.Distance = roundedValue;
      }
   }

   public abstract class ZoneView : UIView
   {
      protected readonly UIColor fillColor;
      protected readonly UIColor borderColor;
      protected readonly nfloat distancePadding = 8;
      protected readonly UILabel distanceLabel;

      public ZoneType ZoneType { get; private set; }
      public nfloat MinSize { get; set; }
      public nfloat MaxSize { get; set; }

      private nfloat size;
      public nfloat Size 
      {
         get
         {
            if( size == default || size == 0 )
               return MaxSize;

            return size;
         }
         set => size = value;
      }

      public string DistanceText
      {
         get => distanceLabel.Text;
         set 
         {
            distanceLabel.Text = value;
            // TODO Update Size 
         } 
      }

      protected ZoneView( ZoneType zoneType, UIColor fillColor, UIColor borderColor )
      {
         ZoneType = zoneType;
         this.fillColor = fillColor;
         this.borderColor = borderColor;

         distanceLabel = new UILabel { 
            TextColor = Colors.White, 
            Font = Fonts.Medium.WithSize( 15 ) 
         };
      }
   }

   public class CircularZoneView : ZoneView
   {
      private CAShapeLayer layer;

      public CircularZoneView( ZoneType zoneType, UIColor fillColor, UIColor borderColor ) : base( zoneType, fillColor, borderColor )
      {
      }

      public override void Draw( CGRect rect )
      {
         var path = UIBezierPath.FromOval( rect );

         layer = new CAShapeLayer {
            BackgroundColor = UIColor.Blue.CGColor,
            Path = path.CGPath,
            FillColor = fillColor.CGColor,
            LineWidth = 2,
            StrokeColor = borderColor.CGColor
         };

         Layer.AddSublayer( layer );

         AddSubview( distanceLabel );

         distanceLabel.Anchor( top: TopAnchor, trailing: TrailingAnchor, padding: new UIEdgeInsets( 8, 0, 0, 8 ) );
      }

      public override void LayoutSubviews( )
      {
         base.LayoutSubviews( );

         if( layer != null )
         {
            var path = UIBezierPath.FromOval( Bounds );
            layer.Path = path.CGPath;
         }
      }
   }

   public class SemiCircularZoneView : ZoneView
   {
      public enum ArcDirection { Top, Right, Bottom, Left }

      private readonly ArcDirection arcDirection;
      private CAShapeLayer layer;
      private nfloat startAngle;
      private nfloat endAngle;

      public SemiCircularZoneView( ZoneType zoneType, ArcDirection arcDirection, UIColor fillColor, UIColor borderColor ) : base( zoneType, fillColor, borderColor )
      {
         this.arcDirection = arcDirection;
      }

      public override void Draw( CGRect rect )
      {
         SetupArcLayer( rect );
         SetupDistanceLabel( );
      }

      private void SetupArcLayer( CGRect rect )
      {
         switch( arcDirection )
         {
            case ArcDirection.Top:
               startAngle = ( nfloat )Math.PI;
               endAngle = 0;
               break;
            case ArcDirection.Right:
               startAngle = ( nfloat )Math.PI * 1.5f;
               endAngle = ( nfloat )Math.PI * 0.5f;
               break;
            case ArcDirection.Bottom:
               startAngle = 0;
               endAngle = ( nfloat )Math.PI;
               break;
            case ArcDirection.Left:
               startAngle = ( nfloat )Math.PI * 0.5f;
               endAngle = ( nfloat )Math.PI * 1.5f;
               break;
         }

         var path = UIBezierPath.FromArc( center: new CGPoint( x: rect.Width / 2, y: rect.Height / 2 ),
            radius: rect.Height / 2, startAngle: startAngle, endAngle, clockwise: true );

         layer = new CAShapeLayer {
            Path = path.CGPath,
            FillColor = fillColor.CGColor,
            LineWidth = 2,
            StrokeColor = borderColor.CGColor
         };

         Layer.AddSublayer( layer );
      }

      private void SetupDistanceLabel( )
      {
         AddSubview( distanceLabel );

         switch( arcDirection )
         {
            case ArcDirection.Top:
               distanceLabel.Anchor( centerY: CenterYAnchor, trailing: TrailingAnchor, padding: new UIEdgeInsets( 0, 0, 0, distancePadding ) );
               break;
            case ArcDirection.Right:
               distanceLabel.Anchor( centerY: CenterYAnchor, leading: TrailingAnchor, padding: new UIEdgeInsets( 0, distancePadding, 0, 0 ) );
               break;
            case ArcDirection.Bottom:
               distanceLabel.Anchor( centerX: CenterXAnchor, top: BottomAnchor, padding: new UIEdgeInsets( distancePadding, 0, 0, 0 ) );
               break;
            case ArcDirection.Left:
               distanceLabel.Anchor( centerY: CenterYAnchor, trailing: LeadingAnchor, padding: new UIEdgeInsets( 0, 0, 0, distancePadding ) );
               break;
         }
      }

      public override void LayoutSubviews( )
      {
         base.LayoutSubviews( );

         if( layer != null )
         {
            var path = UIBezierPath.FromArc( center: new CGPoint( x: Bounds.Width / 2, y: Bounds.Height / 2 ),
            radius: Bounds.Height / 2, startAngle: startAngle, endAngle, clockwise: true );
            layer.Path = path.CGPath;
         }
      }
   }
}
