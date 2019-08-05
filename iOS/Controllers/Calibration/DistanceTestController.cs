using System;
using System.Threading;
using System.Timers;
using PK.Interfaces;
using PK.iOS.Bluetooth;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Controllers
{
   public class DistanceTestController : BaseModalController, IDistanceTestViewModel, IBluetoothLEAdvertisement
   {
      private readonly DistanceTestViewModel viewModel;

      private UILabel RSSILabel;
      private UILabel distanceLabel;
      private UIActivityIndicatorView activityIndicatorView;

      public DistanceTestController( )
      {
         viewModel = new DistanceTestViewModel( this );

         IOSBluetoothLE.Instance.AdvertisementDelegate = this;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         SetupTopToolbar( );
         SetupViews( );
      }

      public override void ViewDidAppear( bool animated )
      {
         base.ViewDidAppear( animated );

         //IOSBluetoothLE.Instance.ScanForAdvertisements( );

         Thread.Sleep( 3000 );
         viewModel.SetRSSI( 3, -87 );
      }

      public override void ViewDidDisappear( bool animated )
      {
         base.ViewDidDisappear( animated );

         //IOSBluetoothLE.Instance.StopScanningForAdvertisements( );
      }

      private void SetupTopToolbar( )
      {
         RSSILabel = new UILabel {
            Text = "-87 dBm",
            TextColor = Colors.White,
            Font = Fonts.Regular.WithSize( 17 )
         };

         var RSSIBarButton = new UIBarButtonItem( customView: RSSILabel );

         TopToolBar.AddItem( RSSIBarButton );
      }

      private void SetupViews( )
      {
         var messageLabel = new UILabel {
            Text = viewModel.MessageText,
            TextColor = Colors.White,
            Font = Fonts.Regular.WithSize( 16 ),
         };

         distanceLabel = new UILabel {
            Text = "0 m",
            TextColor = Colors.White,
            Font = Fonts.Bold.WithSize( 40 ),
            Hidden = true,
         };

         activityIndicatorView = new UIActivityIndicatorView( UIActivityIndicatorViewStyle.WhiteLarge ) {
            Color = Colors.White,
            HidesWhenStopped = true,
         };
         activityIndicatorView.StartAnimating( );

         var stackView = VStack(
            messageLabel,
            distanceLabel,
            activityIndicatorView
         ).With( spacing: 22, alignment: UIStackViewAlignment.Center );

         View.AddSubview( stackView );

         stackView.CenterInSuperView( );
      }

      void IBluetoothLEAdvertisement.ReceivedAdvertisement( int anchorLocationID, int RSSI )
      {
         // TODO Future: Consider multiple anchors in the future 

         viewModel.SetRSSI( anchorLocationID, RSSI );
      }

      void IDistanceTestViewModel.DistanceChanged( string distance )
      {
         if( activityIndicatorView.IsAnimating )
         {
            activityIndicatorView.StopAnimating( );
            distanceLabel.Hidden = false;
         }

         distanceLabel.Text = distance;
      }
   }
}
