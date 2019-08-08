
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using PK.ViewModels;

namespace PK.Droid.Fragments
{
   public class CalibratePageOneFragment : Fragment
   {
      private readonly CalibrateViewModel viewModel;

      public CalibratePageOneFragment( CalibrateViewModel viewModel )
      {
         this.viewModel = viewModel;
      }

      public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
      {
         var pageOneLayout = inflater.Inflate( Resource.Layout.Fragment_Calibrate_Page_One, root: container, attachToRoot: false );

         var titleTextView = pageOneLayout.FindViewById<TextView>( Resource.Id.calibrate_pageone_textview_title );
         titleTextView.Text = viewModel.PageOneTitleText;

         var descriptionTextView = pageOneLayout.FindViewById<TextView>( Resource.Id.calibrate_pageone_textview_description );
         descriptionTextView.Text = viewModel.PageOneDescriptionText;

         var nextButton = pageOneLayout.FindViewById<Button>( Resource.Id.calibrate_pageone_button_next );
         nextButton.Text = viewModel.PageOneActionText;
         nextButton.Click += ( object sender, EventArgs e ) => viewModel.PageOneActionSelected( );

         return pageOneLayout;
      }
   }
}
