using System;
using PK.iOS.Helpers;
using PK.ViewModels;
using UIKit;
using static PK.ViewModels.ConfigureZonesViewModel;
using static PK.iOS.Helpers.Stacks;
using CoreGraphics;

namespace PK.iOS.Views
{
   public class ZoneCell : UICollectionViewCell
   {
      public ZoneCell( IntPtr handle ) : base( handle: handle )
      {
         SetupViews( );
      }

      private ZoneModel zoneSelectionModel;
      private UIImageView zoneImageView;
      private UILabel titleLabel;

      public ZoneModel ZoneSelectionModel 
      {
         get => zoneSelectionModel;
         set
         {
            zoneSelectionModel = value;

            switch( zoneSelectionModel.ZoneType )
            {
               case ZoneType.Welcome:
                  zoneImageView.Image = Images.CarZoneWelcome;
                  break;
               case ZoneType.Passenger:
                  zoneImageView.Image = Images.CarZonePassenger;
                  break;
               case ZoneType.Driver:
                  zoneImageView.Image = Images.CarZoneDriver;
                  break;
               case ZoneType.Boot:
                  zoneImageView.Image = Images.CarZoneBoot;
                  break;
            }

            titleLabel.Text = zoneSelectionModel.Title;
         } 
      }

      private void SetupViews( )
      {
         zoneImageView = new UIImageView {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            TintColor = Colors.White
         };

         titleLabel = new UILabel {
            TextColor = Colors.LightWhite,
            Font = Fonts.Medium.WithSize( 13 ),
         };

         ContentView.AddSubviews( zoneImageView, titleLabel );

         zoneImageView.Anchor( leading: ContentView.LeadingAnchor, top: ContentView.TopAnchor, trailing: ContentView.TrailingAnchor,
            size: new CGSize( 0, 60 ) );

         titleLabel.Anchor( centerX: ContentView.CenterXAnchor, bottom: ContentView.BottomAnchor, padding: new UIEdgeInsets( 0, 0, 10, 0 ) );
      }

      public override bool Highlighted 
      { 
         get => base.Highlighted;
         set 
         {
            base.Highlighted = value;

            zoneImageView.TintColor = Highlighted ? Colors.OuterSpaceLight : Colors.White;
         } 
      }
   }
}
