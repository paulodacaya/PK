using System;
using CoreGraphics;
using Foundation;
using PK.iOS.Helpers;
using PK.iOS.Views;
using PK.ViewModels;
using UIKit;
using static PK.ViewModels.ConfigureZonesViewModel;

namespace PK.iOS.Controllers
{
   public class ZoneSelectionController : UICollectionViewController, IUICollectionViewDelegateFlowLayout
   {
      private readonly string zoneCellId = "zoneCellId";

      private readonly ZoneModel[ ] zoneModels;

      private nfloat CellWidth => CollectionView.Frame.Width / 4;

      private AnchoredConstraints barViewAnchoredConstraints;

      public ZoneSelectionController( ZoneModel[ ] zoneModels ) : base( layout: new UICollectionViewFlowLayout( ) )
      {
         this.zoneModels = zoneModels;
      }

      public override void ViewDidLoad( )
      {
         base.ViewDidLoad( );

         CollectionView.BackgroundColor = Colors.Clear;
         CollectionView.RegisterClassForCell( typeof( ZoneCell ), zoneCellId );

         SetupBottomBar( );
      }

      private void SetupBottomBar( )
      {
         var bar = new UIView { 
            BackgroundColor = Colors.WhiteWithTransparancy,
         };
         bar.WithCornerRadius( 2 );

         View.AddSubview( bar );

         barViewAnchoredConstraints = bar.Anchor( leading: View.LeadingAnchor, bottom: View.BottomAnchor, size: new CGSize( CellWidth, 2 ) );
      }

      public override nint GetItemsCount( UICollectionView collectionView, nint section ) => zoneModels.Length;

      public override UICollectionViewCell GetCell( UICollectionView collectionView, NSIndexPath indexPath )
      {
         var cell = collectionView.DequeueReusableCell( reuseIdentifier: zoneCellId, indexPath: indexPath ) as ZoneCell;
         cell.ZoneSelectionModel = zoneModels[ indexPath.Item ];

         return cell;
      }

      public override void ItemSelected( UICollectionView collectionView, NSIndexPath indexPath )
      {
         UIView.AnimateNotify( duration: 0.15, animation: ( ) => {

            barViewAnchoredConstraints.Leading.Constant = CellWidth * indexPath.Item;
            View.LayoutIfNeeded( );

         }, completion: finished => {

            zoneModels[ indexPath.Item ].Selected( );

         } );
      }

      [Export( "collectionView:layout:sizeForItemAtIndexPath:" )]
      public CGSize GetSizeForItem( UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath )
      {
         return new CGSize( width: CellWidth, height: collectionView.Frame.Height );
      }

      [Export( "collectionView:layout:minimumInteritemSpacingForSectionAtIndex:" )]
      public nfloat GetMinimumInteritemSpacingForSection( UICollectionView collectionView, UICollectionViewLayout layout, nint section ) => 0;
   }
}
