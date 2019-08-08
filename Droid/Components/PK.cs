using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace PK.Droid.Components
{
   public static class PKToast
   {
      public static Toast MakeText( Activity activity, int imageResourceID, string message, ToastLength toastLength )
      {
         var layoutInflator = activity.LayoutInflater;
         var layoutView = layoutInflator.Inflate( Resource.Layout.Toast_PK, root: null );

         var toastImageView = layoutView.FindViewById<ImageView>( Resource.Id.imageview_icon );
         toastImageView.SetImageResource( imageResourceID );

         var toastTextView = layoutView.FindViewById<TextView>( Resource.Id.textview_message );
         toastTextView.Text = message;

         var toast = new Toast( context: activity ) {
            Duration = toastLength,
            View = layoutView
         };
         toast.SetGravity( GravityFlags.CenterHorizontal | GravityFlags.Bottom, xOffset: 0, yOffset: 210 );

         return toast;
      }
   }

   public class PKSnackBar
   {
      public event EventHandler OnActionClick;
      public readonly Snackbar SnackBar;

      public PKSnackBar( Context context, View view, string message, string actionText )
      {
         SnackBar = Snackbar.Make( view: view, text: message, duration: 10000 );

         var snackBarView = SnackBar.View;
         snackBarView.SetBackgroundColor( Color.White );

         var snackBarTextView = snackBarView.FindViewById<TextView>( Resource.Id.snackbar_text );
         snackBarTextView.SetTextColor( new Color( ContextCompat.GetColor( context, Resource.Color.AuroMetalSaurus ) ) );
         snackBarTextView.Typeface = context.Resources.GetFont( Resource.Font.BrixSansMedium );
         snackBarTextView.TextSize = 14f;

         var snackBarActionButton = snackBarView.FindViewById<Button>( Resource.Id.snackbar_action );
         snackBarActionButton.SetTextColor( new Color( ContextCompat.GetColor( context, Resource.Color.OuterSpace ) ) );
         snackBarActionButton.Typeface = context.Resources.GetFont( Resource.Font.BrixSansBold );
         snackBarActionButton.TextSize = 16f;

         SnackBar.SetAction( text: actionText, clickHandler: HandleActionClick );
      }

      public void Show( )
      {
         SnackBar.Show( );
      }

      private void HandleActionClick( View view )
      {
         OnActionClick?.Invoke( SnackBar, new EventArgs( ) );
      }

      public class PKSnackBarCallBack : Snackbar.Callback
      {
         private readonly EventHandler dismissEventOnAction;

         public PKSnackBarCallBack( EventHandler dismissEventOnAction )
         {
            this.dismissEventOnAction = dismissEventOnAction;
         }

         public override void OnDismissed( Snackbar transientBottomBar, int e )
         {
            // Can handle different dissmiss types :)
            if( e == DismissEventAction )
            {
               dismissEventOnAction?.Invoke( transientBottomBar, new EventArgs( ) );
            }
         }
      }
   }

   public class VerticalSpaceItemDecoration : RecyclerView.ItemDecoration
   {
      private readonly int verticalSpaceHeight;

      public VerticalSpaceItemDecoration( int verticalSpaceHeight )
      {
         this.verticalSpaceHeight = verticalSpaceHeight;
      }

      public override void GetItemOffsets( Rect outRect, View view, RecyclerView parent, RecyclerView.State state )
      {
         outRect.Bottom = verticalSpaceHeight;
      }
   }
}
