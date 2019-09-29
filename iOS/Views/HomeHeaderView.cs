using PK.iOS.Helpers;
using UIKit;
using static PK.iOS.Helpers.Stacks;

namespace PK.iOS.Views
{
   public class HomeHeaderView : UIView
   {
      public UILabel GreetingLabel { get; }
      public UILabel VehicleInfoLabel { get; }
      public UILabel VehicleStateLabel { get; }
      public UIImageView VehicleStateImageView { get; }


      public HomeHeaderView( )
      {
         PreservesSuperviewLayoutMargins = true;

         GreetingLabel = Components.UILabel( string.Empty, Colors.BoschBlue, Fonts.BoschBold.WithSize( 26 ) );
         VehicleInfoLabel = Components.UILabel( string.Empty, Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ) );
         VehicleStateLabel = Components.UILabel( string.Empty, Colors.BoschBlue, Fonts.BoschLight.WithSize( 15 ), lines: 1 );
         VehicleStateImageView = Components.UIImageView( null, Colors.BoschBlue );

         var stackView = VStack(
            GreetingLabel,
            VehicleInfoLabel,
            HStack(
               VehicleStateLabel,
               VehicleStateImageView.WithSquareSize( 12 ),
               new UIView( )
            ).With( spacing: 8 ),
            new UIView( )
         ).With( spacing: 6 ).WithPadding( new UIEdgeInsets( 24, 16, 34, 16 ) );

         stackView.SetCustomSpacing( 12, GreetingLabel );

         AddSubview( stackView );

         stackView.Anchor( leading: LayoutMarginsGuide.LeadingAnchor, top: TopAnchor,
            trailing: LayoutMarginsGuide.TrailingAnchor, bottom: BottomAnchor );
      }
   }
}
