using System;
using PK.Models;
using Realms;

namespace PK.ViewModels
{
   public interface ICalibrateViewModel
   {
      bool VerifyCameraPermission( );
      void NavigateToConfigureZones( );
      void NavigateToPageTwo( );
      void NavigateToCameraCalibration( );
      void NavigateToManualCalibration( );
   }

   public class CalibrateViewModel
   {
      public string Title => "CALIBRATE";

      public string ConfigureZonesText = "Configure Zones";

      public string PageOneTitleText => "Let's Calibrate your Device!";
      public string PageOneDescriptionText => "We need to know how far you are from your vehicle. You have a number options to complete this calibration.";
      public string PageOneActionText => "NEXT";

      public string PageTwoTitleText => "Select an option:";

      public bool CalibrationCompleted
      {
         get
         {
            var realm = Realm.GetInstance( PKRealm.Configuration );
            var calibrationData = realm.Find<CalibrationData>( 0 );
            return calibrationData?.Completed ?? false;
         }
      }

      public OptionListViewModel[ ] OptionList;

      private readonly ICalibrateViewModel viewModel;

      public CalibrateViewModel( ICalibrateViewModel viewModel )
      {
         this.viewModel = viewModel;

         OptionList = new OptionListViewModel[ ]
         {
            new OptionListViewModel( OptionListViewModel.Camera, "Semi-automatic calibration using the device camera." ),
            //new OptionListViewModel( OptionListViewModel.Manual, "Manually stand at different distances and calibrate." )
         };

         foreach( var option in OptionList )
         {
            option.OnSelected += HandleOptionSelected;
         }
      }

      private void HandleOptionSelected( object sender, EventArgs e )
      {
         var option = sender as OptionListViewModel;

         switch( option.Option )
         {
            case OptionListViewModel.Camera:
               if( viewModel.VerifyCameraPermission( ) )
                  viewModel.NavigateToCameraCalibration( );
               break;
            case OptionListViewModel.Manual:
               viewModel.NavigateToManualCalibration( );
               break;
         }
      }

      public void ConfigureZonesSelected( ) => viewModel.NavigateToConfigureZones( );

      public void PageOneActionSelected( ) => viewModel.NavigateToPageTwo( );
   }

   public class OptionListViewModel
   {
      public event EventHandler OnSelected;

      public const int Camera = 0;
      public const int Manual = 1;

      public readonly int Option;
      public readonly string Text;

      public OptionListViewModel( int option, string text )
      {
         Option = option;
         Text = text;
      }

      public void Selected( ) => OnSelected?.Invoke( this, new EventArgs( ) );
   }
}
