using System;
using System.Collections.Generic;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class ManualCalibrationController : UIViewController, IManualCalibrationViewModel
   {
      private readonly ManualCalibrationViewModel viewModel;

      // UI Elements
      private UIButton doneButton;
      private CalibrationProgressView progressView;
      private DistancePickerView distancePicker;

      public ManualCalibrationController( )
      {
         viewModel = new ManualCalibrationViewModel( this );
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         ( NavigationController as RootNavigationController )?.HideVehicle( );

         SetupNavigation( );
         SetupViews( );

         viewModel.SelectDistance( 0 ); // Initial selected distance
      }

      public override void ViewDidLayoutSubviews( )
      {
         base.ViewDidLayoutSubviews( );

         doneButton.WithCornerRadius( doneButton.Frame.Height / 2 );
      }

      private void SetupNavigation( )
      {
         var backButton = new UIButton( UIButtonType.System ) {
            ContentEdgeInsets = new UIEdgeInsets( 0, 0, 0, 0 ),
            TintColor = Colors.White,
         };
         backButton.SetImage( Images.ChevronLeft, UIControlState.Normal );
         backButton.Anchor( size: new CGSize( 22, 22 ) );
         backButton.TouchUpInside += HandleBackButtonTouchUpSide;

         doneButton = new UIButton( UIButtonType.System );
         doneButton.TitleLabel.Font = Fonts.Bold.WithSize( 16 );
         doneButton.SetTitle( "Done", UIControlState.Normal );
         doneButton.SetTitleColor( Colors.White, UIControlState.Normal );
         doneButton.WithBorder( width: 1, color: Colors.White );
         doneButton.WithSize( new CGSize( 80, 34 ) );
         doneButton.TouchUpInside += HandleDoneButtonTouchUpInside;

         NavigationItem.LeftBarButtonItem = new UIBarButtonItem( customView: backButton );
         NavigationItem.RightBarButtonItem = new UIBarButtonItem( customView: doneButton );
      }

      private void SetupViews( )
      {
         var titleLabel = new UILabel {
            Text = viewModel.Title,
            TextColor = Colors.White,
            Font = Fonts.Bold.WithSize( 24 )
         };

         var subTitleLabel = new UILabel {
            Text = viewModel.SubTitle,
            TextColor = Colors.White,
            Font = Fonts.Medium.WithSize( 14 ),
            Lines = 0,
         };

         progressView = new CalibrationProgressView( totalColumns: viewModel.TotalDistances );

         var stackView = VStack(
            VStack(
               titleLabel,
               subTitleLabel
            ).With( spacing: 4 ),
            progressView
         ).With( spacing: 18 );

         distancePicker = new DistancePickerView( viewModel );

         var calibrateButton = new PKButton( );
         calibrateButton.SetTitle( "CALIBRATE", UIControlState.Normal );
         calibrateButton.SetTitleColor( Colors.White, UIControlState.Normal );
         calibrateButton.TouchUpInside += HandleCalibrateButtonTouchUpInside;

         View.AddSubviews( stackView, distancePicker, calibrateButton );

         stackView.Anchor( top: View.LayoutMarginsGuide.TopAnchor, leading: View.LayoutMarginsGuide.LeadingAnchor,
            trailing: View.LayoutMarginsGuide.TrailingAnchor );

         distancePicker.Anchor( top: stackView.BottomAnchor, leading: View.LeadingAnchor, trailing: View.TrailingAnchor );

         calibrateButton.Anchor( centerX: View.CenterXAnchor, bottom: View.SafeAreaLayoutGuide.BottomAnchor,
            padding: new UIEdgeInsets( 0, 0, 22, 0 ) );
      }

      void IManualCalibrationViewModel.DistanceSelectedAt( int index ) => progressView.SelectColumnAt( index );

      void IManualCalibrationViewModel.DistanceCalibratedAt( int index, double percent )
      {
         var roundedPercentage = Math.Round( percent, 2 );

         progressView.SetProgressWithAnimationAt( index, roundedPercentage, completion: ( ) => {

            if( roundedPercentage.Equals( 1.0 ) )
            {
               var preferredLocationController = new PreferredLocationController( viewModel );
               PresentViewController( preferredLocationController, animated: false, completionHandler: null );

               var nextDistance = index + 1;

               if( nextDistance >= viewModel.TotalDistances )
                  return;

               distancePicker.Select( row: nextDistance, component: 0, animated: true );
               distancePicker.Selected( pickerView: distancePicker, row: nextDistance, component: 0 );
            }
         } );
      }

      void IManualCalibrationViewModel.CalibrationFinished( )
      {
         // TODO Present zoning configuration
      }

      void IManualCalibrationViewModel.NotifyCalibrationIncomplete( )
      {
         var dialog = new DialogController( prominent: true ) {
            TitleText = "Finished?",
            MessageText = "Calibration was incomplete but your progress will be saved.",
            NegativeText = "NO",
            PositiveText = "Yes",
         };

         dialog.OnPositiveButtonTouchUpInside += (sender, e) => {
            NavigationController.PopViewController( animated: true );
         };

         PresentViewController( dialog, animated: true, completionHandler: null );
      }

      private void HandleBackButtonTouchUpSide( object sender, EventArgs e ) => NavigationController.PopViewController( animated: true );

      private void HandleDoneButtonTouchUpInside( object sender, EventArgs e ) => viewModel.ActionDone( );

      private void HandleCalibrateButtonTouchUpInside( object sender, EventArgs e ) => viewModel.ActionCalibrate( );
   }

   public class CalibrationProgressView : UIView
   {
      private readonly int totalColumns;
      private List<CalibrationColumnStack> calibrationColumns;

      public CalibrationProgressView( int totalColumns )
      {
         BackgroundColor = Colors.White;
         Layer.CornerRadius = Values.CornerRadius;

         this.totalColumns = totalColumns;

         SetupColumns( );
      }

      private void SetupColumns( )
      {

         calibrationColumns = new List<CalibrationColumnStack>( );

         var columnStackView = HStack( ).With( spacing: 18, distribution: UIStackViewDistribution.FillEqually );

         for( int i = 0; i < totalColumns; i++ )
         {
            var calibraitonColumn = new CalibrationColumnStack( );
            calibraitonColumn.DistanceLabel.Text = $"{i + 1}";

            calibrationColumns.Add( calibraitonColumn );

            columnStackView.AddArrangedSubview( calibraitonColumn );
         }

         var metresLabel = new UILabel {
            Text = "METRE",
            TextColor = Colors.OuterSpace,
            TextAlignment = UITextAlignment.Center,
            Font = Fonts.Medium,
         };

         var mainStackView = VStack(
            columnStackView,
            metresLabel
         ).With( spacing: 20 );

         AddSubview( mainStackView );

         mainStackView.FillSuperview( padding: new UIEdgeInsets( 26, 26, 26, 26 ) );
      }

      public void SelectColumnAt( int columnIndex )
      {
         for( var i = 0; i < calibrationColumns.Count; i++ )
         {
            var column = calibrationColumns[ i ];
            column.UpArrowImageView.Alpha = ( i == columnIndex ) ? 1 : 0;
         }
      }

      public void SetProgressAt( int columnIndex, double roundedPercent )
      {



         var column = calibrationColumns[ columnIndex ];
         var constraintValue = 100.0 - ( roundedPercent * 100.0 );

         column.IndicatorBarAnchoredConstraints.Top.Constant = ( nfloat )constraintValue;

         if( ( int )roundedPercent == 1 ) // 100%
         {
            column.CheckBox.SetOn( true, animated: true );
         }
      }

      public void SetProgressWithAnimationAt( int columnIndex, double roundedPercent, Action completion )
      {
         SetProgressAt( columnIndex, roundedPercent );

         UIView.AnimateNotify( duration: 0.2f, animation: ( ) => {
            var column = calibrationColumns[ columnIndex ];
            column.IndicatorBarContainer.LayoutIfNeeded( );
         }, completion: ( bool finished ) => {
            if( finished )
               completion?.Invoke( );
         } );
      }

      public class CalibrationColumnStack : UIStackView
      {
         public CircularCheckBox CheckBox;
         public UIActivityIndicatorView AcitivityIndicationView;
         public UILabel DistanceLabel;
         public UIImageView UpArrowImageView;
         public UIView IndicatorBarContainer;
         public AnchoredConstraints IndicatorBarAnchoredConstraints;

         private UIView indicatorBar;
         private CAGradientLayer indicatorBarGradientLayer;

         public CalibrationColumnStack( )
         {
            Axis = UILayoutConstraintAxis.Vertical;
            Alignment = UIStackViewAlignment.Center;
            Spacing = 8;

            SetupViews( );
         }

         public override void LayoutSubviews( )
         {
            base.LayoutSubviews( );

            IndicatorBarContainer.Layer.CornerRadius = IndicatorBarContainer.Frame.Width / 2;
            indicatorBar.Layer.CornerRadius = IndicatorBarContainer.Frame.Width / 2;
            indicatorBarGradientLayer.Frame = IndicatorBarContainer.Bounds;
         }

         private void SetupViews( )
         {
            CheckBox = new CircularCheckBox( );
            CheckBox.ConstraintHeightEqualTo( 12 );
            CheckBox.ConstraintWidthEqualTo( 12 );

            AcitivityIndicationView = new UIActivityIndicatorView( );

            IndicatorBarContainer = new UIView {
               BackgroundColor = Colors.OuterSpace.ColorWithAlpha( 0.1f ),
            };
            IndicatorBarContainer.ConstraintWidthEqualTo( 12 );
            IndicatorBarContainer.ConstraintHeightEqualTo( 100 );

            indicatorBar = new UIView {
               ClipsToBounds = true,
            };

            indicatorBarGradientLayer = new CAGradientLayer {
               Colors = new CGColor[ ] { Colors.BoschLightGreen.CGColor, Colors.BoschDarkGreen.CGColor },
               Locations = new NSNumber[ ] { 0, 1 },
            };

            indicatorBar.Layer.AddSublayer( indicatorBarGradientLayer );

            IndicatorBarContainer.AddSubview( indicatorBar );

            IndicatorBarAnchoredConstraints = indicatorBar.FillSuperview( padding: new UIEdgeInsets( 80, 0, 0, 0 ) );

            DistanceLabel = new UILabel {
               TextColor = Colors.OuterSpace,
               Font = Fonts.Regular
            };

            UpArrowImageView = new UIImageView {
               Alpha = 0,
               Image = Images.ArrowSlimUp,
               TintColor = Colors.OuterSpace,
               ContentMode = UIViewContentMode.ScaleAspectFit,
            };
            UpArrowImageView.ConstraintHeightEqualTo( 22 );

            AddArrangedSubview( CheckBox );
            AddArrangedSubview( new CustomUIView( new CGSize( 0, 1 ) ) );
            AddArrangedSubview( IndicatorBarContainer );
            AddArrangedSubview( DistanceLabel );
            AddArrangedSubview( UpArrowImageView );
         }
      }
   }

   public class DistancePickerView : UIPickerView, IUIPickerViewDataSource, IUIPickerViewDelegate
   {
      private readonly ManualCalibrationViewModel viewModel;

      public DistancePickerView( ManualCalibrationViewModel viewModel )
      {
         this.viewModel = viewModel;

         DataSource = this;
         Delegate = this;
      }

      public override void SubviewAdded( UIView uiview )
      {
         base.SubviewAdded( uiview );

         if( uiview.Bounds.Height <= 1.0 )
            uiview.BackgroundColor = Colors.White.ColorWithAlpha( 0.2f );
      }

      public nint GetComponentCount( UIPickerView pickerView ) => 1;

      public nint GetRowsInComponent( UIPickerView pickerView, nint component ) => viewModel.Distances.Length;

      [Export( "pickerView:didSelectRow:inComponent:" )]
      public void Selected( UIPickerView pickerView, nint row, nint component ) => viewModel.SelectDistance( ( int )row );

      [Export( "pickerView:attributedTitleForRow:forComponent:" )]
      public NSAttributedString GetAttributedTitle( UIPickerView pickerView, nint row, nint component )
      {
         return new NSAttributedString(
            str: viewModel.Distances[ ( int )row ],
            font: Fonts.Bold.WithSize( 26 ),
            foregroundColor: Colors.Platinum
         );
      }
   }
}

