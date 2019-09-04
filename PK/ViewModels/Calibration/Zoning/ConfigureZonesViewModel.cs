using System;
using System.Linq;
using PK.Models;
using Realms;

namespace PK.ViewModels
{
   public interface IConfigureZonesViewModel
   {
      void ZoneSelected( ZoneType zoneType, double distance, double min, double max );
      void NavigateToHome( );
   }

   public class ConfigureZonesViewModel
   {
      private readonly IConfigureZonesViewModel viewModel;

      public string Title => "Configure Zones";
      public string SubTitle => "Use the slider to configure your desired zones.";

      public ZoneModel[ ] ZoneModels { get; private set; }

      public ZoneType SelectedZoneType { get; private set; }

      public ConfigureZonesViewModel( IConfigureZonesViewModel viewModel )
      {
         this.viewModel = viewModel;

         var realm = Realm.GetInstance( PKRealm.Configuration );
         var zones = realm.All<Zone>( );

         var welcomeZoneModel = new ZoneModel {
            ZoneType = ZoneType.Welcome,
            Title = "WELCOME",
            MinDistance = 1,
            MaxDistance = 2,
            Distance = zones.SingleOrDefault( z => z.ZoneID == ( int )ZoneType.Welcome )?.Distance ?? 2,
         };
         welcomeZoneModel.OnZoneSelected += HandleZoneSelected;

         var driverZoneModel = new ZoneModel {
            ZoneType = ZoneType.Driver,
            Title = "DRIVER",
            MinDistance = 0.6,
            MaxDistance = 1,
            Distance = zones.SingleOrDefault( z => z.ZoneID == ( int )ZoneType.Driver )?.Distance ?? 1,
         };
         driverZoneModel.OnZoneSelected += HandleZoneSelected;

         var passengerZoneModel = new ZoneModel { 
            ZoneType = ZoneType.Passenger, 
            Title = "PASSENGER",
            MinDistance = 0.6,
            MaxDistance = 1,
            Distance = zones.SingleOrDefault( z => z.ZoneID == ( int )ZoneType.Passenger )?.Distance ?? 1,
         };
         passengerZoneModel.OnZoneSelected += HandleZoneSelected;

         var bootZoneModel = new ZoneModel {
            ZoneType = ZoneType.Boot,
            Title = "BOOT",
            MinDistance = 1,
            MaxDistance = 1.5,
            Distance = zones.SingleOrDefault( z => z.ZoneID == ( int )ZoneType.Boot )?.Distance ?? 1.5,
         };
         bootZoneModel.OnZoneSelected += HandleZoneSelected;

         ZoneModels = new ZoneModel[ ] { welcomeZoneModel, passengerZoneModel, driverZoneModel, bootZoneModel };
      }

      private void HandleZoneSelected( object sender, EventArgs e ) 
      {
         var zoneModel = sender as ZoneModel;

         SelectedZoneType = zoneModel.ZoneType;
         viewModel.ZoneSelected( zoneModel.ZoneType, zoneModel.Distance, zoneModel.MinDistance, zoneModel.MaxDistance );
      }

      public void ActionSelected( )
      {
         var realm = Realm.GetInstance( PKRealm.Configuration );

         try
         {
            using( var transaction = realm.BeginWrite( ) )
            {
               foreach( var model in ZoneModels )
               {
                  var update = true;

                  var zone = realm.Find<Zone>( ( int )model.ZoneType );

                  if( zone == null )
                  {
                     zone = new Zone( );
                     update = false;
                  }

                  zone.ZoneID = ( int )model.ZoneType;
                  zone.Distance = model.Distance;

                  realm.Add( zone, update );
               }

               transaction.Commit( );
            }
         }
         catch( Exception ex )
         {
            throw new Exception( $"Failed to save. Zone data could not be stored: {ex.Message}" );
         }

         viewModel.NavigateToHome( );
      }
   }

   public enum ZoneType { Welcome, Passenger, Driver, Boot }

   public class ZoneModel
   {
      public event EventHandler OnZoneSelected;

      public ZoneType ZoneType { get; set; }
      public string Title { get; set; }
      public double Distance { get; set; }
      public double MinDistance { get; set; }
      public double MaxDistance { get; set; }

      public void Selected( ) => OnZoneSelected?.Invoke( this, new EventArgs( ) );
   }
}
