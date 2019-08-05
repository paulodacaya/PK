using System;
using System.Text;

namespace PK.Common
{
   public static class Extensions
   {
      public static string ToReadableByteArray( this byte[ ] bytes )
      {
         var stringBuilder = new StringBuilder( );

         for( var i = 0; i < bytes.Length; i++ )
         {
            var b = bytes[ i ];

            stringBuilder.Append( b );

            if( i < bytes.Length - 1 )
            {
               stringBuilder.Append( ", " );
            }
         }

         return stringBuilder.ToString( );
      }
   }
}
