namespace PK.ViewModels
{
   public interface IOnBoardingViewModel
   {
      void NavigateToCameraCalibration( );
   }

   public class CalibrateOnBoardingViewModel
   {
      public SimpleInformation[ ] Information { get; }

      private IOnBoardingViewModel viewModel;

      public CalibrateOnBoardingViewModel( IOnBoardingViewModel viewModel )
      {
         this.viewModel = viewModel;

         Information = new[ ] {
            new SimpleInformation {
               Title = "Welcome to the Perfectly Keyless app",
               Message = "Firstly, we need to set up and calibrate your device."
            },
            new SimpleInformation {
               Title = "Get your Access Card",
               SubTitle = "This is your backup card to unlock your vehicle.",
               Message = "Stick your access card to the door handle of vehicle, make sure the image is facing you."
            },
            new SimpleInformation {
               Title = "Calibrate your Device",
               Message = "Find the image you stuck on your vehicle and follow the instructions during the calibration experience."
            }
         };

      }

      public void GoSelected( )
      {
         viewModel.NavigateToCameraCalibration( );
      }

      public class SimpleInformation
      {
         public string Title { get; set; }
         public string SubTitle { get; set; }
         public string Message { get; set; }
      }
   }
}
