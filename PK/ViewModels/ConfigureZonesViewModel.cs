using System;
using System.Linq;
using PK.Models;
using Realms;

namespace PK.ViewModels
{
   public interface IConfigureZonesViewModel
   {
   }

   public class ConfigureZonesViewModel
   {
      private readonly IConfigureZonesViewModel viewModel;

      public ConfigureZonesViewModel( IConfigureZonesViewModel viewModel )
      {
      }
   }
}
