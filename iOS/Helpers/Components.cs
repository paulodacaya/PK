using UIKit;

namespace PK.iOS.Helpers
{
   public static class Components
   {
      public static UILabel UILabel( string text, UIColor textColor, UIFont font, int lines = 0, UITextAlignment textAlignment = UITextAlignment.Left )
      {
         return new UILabel {
            Text = text,
            TextColor = textColor,
            TextAlignment = textAlignment,
            Font = font,
            Lines = lines
         };
      }

      public static UIImageView UIImageView( UIImage image, UIColor tintColor, UIViewContentMode contentMode = UIViewContentMode.ScaleAspectFit )
      {
         return new UIImageView {
            Image = image,
            TintColor = tintColor,
            ContentMode = contentMode
         };
      }

      public static UIButton UIButton( string title, UIColor titleColor, UIFont font, UIEdgeInsets padding = default )
      {
         var button = new UIButton( UIButtonType.System );
         button.SetTitle( title, UIControlState.Normal );
         button.SetTitleColor( titleColor, UIControlState.Normal );
         button.TitleLabel.Font = font;
         button.ContentEdgeInsets = padding;

         return button;
      }

      public static UIButton UIButton( UIImage image, UIColor tintColor, UIViewContentMode contentMode = UIViewContentMode.ScaleAspectFit, UIEdgeInsets padding = default )
      {
         var button = new UIButton( UIButtonType.System );
         button.SetImage( image, UIControlState.Normal );
         button.TintColor = tintColor;
         button.ImageView.ContentMode = contentMode;
         button.ContentEdgeInsets = padding;

         return button;
      }   
   }
}
