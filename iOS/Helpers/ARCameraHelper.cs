using System;
using ARKit;

namespace PK.iOS.Helpers
{
   public static class ARCameraHelper
   {
      public static string PresentationStringForState( ARTrackingState trackingState, ARTrackingStateReason trackingStateReason )
      {
         var message = string.Empty;

         switch( trackingState )
         {
            case ARTrackingState.NotAvailable:
               message = "Tracking Unavailable";
               break;
            case ARTrackingState.Limited:
               message = "Tracking Limited";
               break;
            case ARTrackingState.Normal:
               message = "Tracking Normal";
               break;
         }

         switch( trackingStateReason )
         {
            case ARTrackingStateReason.ExcessiveMotion:
               message += "\nExcessive motion: Try slow down movement.";
               break;
            case ARTrackingStateReason.Initializing:
               message += "\nInitializing";
               break;
            case ARTrackingStateReason.InsufficientFeatures:
               message += "\nLow Detail: Try clearer lighting.";
               break;
            case ARTrackingStateReason.None:
               break;
            case ARTrackingStateReason.Relocalizing:
               message += "\nRecovering: Try returning to previous location.";
               break;
         }

         Console.WriteLine( $"iOS - Tracking state changed: {message}" );

         return message;
      }
   }
}
