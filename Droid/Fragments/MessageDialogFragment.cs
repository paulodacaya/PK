using System;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PK.Droid.Fragments
{
   public class MessageDialogFragment : Android.Support.V4.App.DialogFragment
   {
      public IDialogInterfaceOnDismissListener DialogDismissListener;

      public string Title { get; set; }
      public string Message { get; set; }
      public string PostiveButtonText { get; set; }
      public string NegativeButtonText { get; set; }

      public override Dialog OnCreateDialog( Bundle savedInstanceState )
      {
         var dialogBuilder = new AlertDialog.Builder( RequireActivity( ) );

         var layoutInflator = RequireActivity( ).LayoutInflater;
         var layoutView = layoutInflator.Inflate( Resource.Layout.Fragment_Message_Dialog, root: null );
         var layoutViewContraintLayoutParamters = layoutView.LayoutParameters;

         var dialogTitleTextView = layoutView.FindViewById<TextView>( Resource.Id.textview_title );
         dialogTitleTextView.Text = Title;

         var dialogMessageTextView = layoutView.FindViewById<TextView>( Resource.Id.textview_message );
         dialogMessageTextView.Text = Message;

         var dialogPositiveButton = layoutView.FindViewById<Button>( Resource.Id.button_positive );
         dialogPositiveButton.Text = PostiveButtonText;
         dialogPositiveButton.Visibility = string.IsNullOrEmpty( PostiveButtonText ) ? ViewStates.Invisible : ViewStates.Visible;
         dialogPositiveButton.Click += DialogPositiveButtonClick;

         //var dialogNegativeButton = layoutView.FindViewById<Button>( Resource.Id.button_negative );
         //dialogNegativeButton.Text = NegativeButtonText;
         //dialogNegativeButton.Visibility = string.IsNullOrEmpty( NegativeButtonText ) ? ViewStates.Invisible : ViewStates.Visible;

         dialogBuilder.SetView( layoutView );

         var dialog = dialogBuilder.Create( );
         dialog.Window.SetBackgroundDrawable( new ColorDrawable( Android.Graphics.Color.Transparent ) );
         dialog.Window.AddFlags( WindowManagerFlags.DimBehind );
         dialog.Window.SetDimAmount( 0.85f );

         return dialog;
      }

      #region Event Handlers
      private void DialogPositiveButtonClick( object sender, EventArgs e )
      {
         Dismiss( );
      }
      #endregion
   }
}
