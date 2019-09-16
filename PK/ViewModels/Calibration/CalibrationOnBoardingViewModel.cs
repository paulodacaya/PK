namespace PK.ViewModels
{
   public interface IOnBoardingViewModel
   {
      void PresentCameraCalibration( );
   }

   public class CalibrateOnBoardingViewModel
   {
      private IOnBoardingViewModel viewModel;

      public readonly string Title;
      public readonly string SubTitle;
      public readonly string Message;
      public readonly string SectionOneTitle;
      public readonly string SectionOneMessage;
      public readonly string SectionTwoTitle;
      public readonly string SectionTwoMessage;

      public CalibrateOnBoardingViewModel( IOnBoardingViewModel viewModel )
      {
         this.viewModel = viewModel;

         Title = "Welcome to the Perfectly Keyless App!";
         SubTitle = "Your new digital key on a mobile phone. Before we get started, let's calibrate your device.";
         Message = "This calibration ensures your mobiles phone's position is accurate with your vehicle.";

         SectionOneTitle = "Get your Calibration Card";
         SectionOneMessage = "Place your Calibration Card on the door handle, have the image facing you.";

         SectionTwoTitle = "Follow On-Screen Instructions";
         SectionTwoMessage = "Find the image to track using the camera and hold the device at specific distance until completed.";
      }

      public void ActionSelected( )
      {
         // TODO Connect with Cloud Database and check if this device has been calibrated previously.
         // Propmt if the user wants to use exisitng calibration or do it themselves.

         viewModel.PresentCameraCalibration( );
      }
   }
}
