using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace PK.Droid.Adapters
{
   public class CalibrateSlidePagerAdapter : FragmentStatePagerAdapter
   {
      private readonly FragmentManager fragmentManager;
      private readonly Fragment[ ] fragments;

      public CalibrateSlidePagerAdapter( FragmentManager fragmentManager, Fragment[ ] fragments ) : base( fragmentManager )
      {
         this.fragmentManager = fragmentManager;
         this.fragments = fragments;
      }

      public override int Count => fragments.Length;

      public override Fragment GetItem( int position )
      {
         return fragments[ position ];
      }
   }
}
