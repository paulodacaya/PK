namespace PK.ViewModels
{
   public interface IHomeViewModel
   {
   }

   public class HomeViewModel
   {
      private readonly IHomeViewModel viewModel;

      public HomeViewModel( IHomeViewModel viewModel )
      {
         this.viewModel = viewModel;
      }

      
   }
}
