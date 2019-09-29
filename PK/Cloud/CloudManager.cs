using System;
using System.Collections.Generic;
using Firebase.CloudFirestore;
using Firebase.Core;
using PK.Models;

namespace PK.Cloud
{
   public static class CloudManager
   {
      private const string GOOGLE_APP_ID = "1:1046813025994:ios:92dc5a01265663afd63633";
      private const string GCM_SENDER_ID = "1046813025994";
      private const string API_KEY = "AIzaSyBGAYmkbthCEH0bOHWKAgjPc98Qd2umq_I";
      private const string BUNDLE_ID = "com.paulodacaya.pk";
      private const string CLIENT_ID = "1046813025994-t6jc28h5rcc690egb959co67r3v0jbhc.apps.googleusercontent.com";
      private const string DATABASE_URL = "https://perfectlykeyless-75ba6.firebaseio.com";
      private const string PROJECT_ID = "perfectlykeyless-75ba6";
      private const string STORAGE_BUCKET = "perfectlykeyless-75ba6.appspot.com";

      private static class Collections
      {
         public const string CalibrationData = "CalibrationData";
      }

      private readonly static Firestore db;

      static CloudManager( )
      {
         var firebaseOptions = new Options( GOOGLE_APP_ID, GCM_SENDER_ID ) {
            ApiKey = API_KEY,
            BundleId = BUNDLE_ID,
            ClientId = CLIENT_ID,
            DatabaseUrl = DATABASE_URL,
            ProjectId = PROJECT_ID,
            StorageBucket = STORAGE_BUCKET
         };

         App.Configure( firebaseOptions );

         db = Firestore.Create( App.DefaultInstance );

         // Update db settings
         var settings = db.Settings;
         settings.TimestampsInSnapshotsEnabled = true;
         db.Settings = settings;

         if( db == null )
            throw new NullReferenceException( "Firestore Database does not exist. Check Firebase options." );
      }

      public static void GetCalibrationData( string deviceModel, DocumentSnapshotHandler completion )
      {
         var docRef = db.GetCollection( Collections.CalibrationData ).GetDocument( deviceModel );

         docRef.GetDocument( completion );
      }

      public static void SetCalibrationData( Calibration calibrationData, DocumentActionCompletionHandler completion )
      {
         var dataDictionary = new Dictionary<object, object>( );

         dataDictionary.Add( Calibration.KEY_DEVICE_MODEL, calibrationData.DeviceModel );
         dataDictionary.Add( Calibration.KEY_RSSI_ONE_METRE, calibrationData.Rssi_One_Metre );

         var docRef = db.GetCollection( Collections.CalibrationData ).GetDocument( calibrationData.DeviceModel );

         docRef.SetData( dataDictionary, completion );
      }
   }
}
