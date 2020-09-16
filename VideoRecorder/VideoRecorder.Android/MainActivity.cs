using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bundle = Android.OS.Bundle;
using Com.Hbisoft.Hbrecorder;
using System.Diagnostics;
using Android.Content;
using Autofac;
using Android.Preferences;
using Android.OS;
using Android.Graphics;
using Java.IO;
using System.IO;
using Java.Text;
using Java.Util;
using Debug = System.Diagnostics.Debug;
using Android.Provider;
using Android.Net;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using File = Java.IO.File;
using Android;
using AndroidX.Annotations;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Xamarin.Essentials;
using Android.Media.Projection;

namespace VideoRecorder.Droid
{
    [Activity(Label = "VideoRecorder", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IHBRecorderListener
    {
        //Permissions
        public const int SCREEN_RECORD_REQUEST_CODE = 777;
        public const int PERMISSION_REQ_ID_RECORD_AUDIO = 22;
        public const int PERMISSION_REQ_ID_WRITE_EXTERNAL_STORAGE = PERMISSION_REQ_ID_RECORD_AUDIO + 1;
        public Boolean hasPermissions = false;

        //Reference to checkboxes and radio buttons
        Boolean wasHDSelected = true;
        Boolean isAudioEnabled = true;

        //Declare HBRecorder
        public HBRecorder hbRecorder;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);


            hbRecorder = new HBRecorder(this, this);
            App.Builder.RegisterInstance<IVideoRecorderService>(new VideoRecorderService(this, hbRecorder)).SingleInstance();
            App.BuildContainer();


            LoadApplication(new App());
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {

            switch (requestCode)
            {
                case PERMISSION_REQ_ID_RECORD_AUDIO:
                    if (grantResults[0] == Permission.Granted)
                    {
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage, PERMISSION_REQ_ID_WRITE_EXTERNAL_STORAGE);
                    }
                    else
                    {
                        hasPermissions = false;
                        //showLongToast("No permission for " + Manifest.permission.RECORD_AUDIO);
                    }
                    break;
                case PERMISSION_REQ_ID_WRITE_EXTERNAL_STORAGE:
                    if (grantResults[0] == Permission.Granted)
                    {
                        hasPermissions = true;
                        //Permissions was provided
                        //Start screen recording
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            //Start screen recording
                            App.Container.Resolve<IVideoRecorderService>().StartScreenRecording();
                        }
                    }
                    else
                    {
                        hasPermissions = false;
                    }
                    break;
                default:
                    break;
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //Check if permissions was granted
        public Boolean CheckSelfPermission(String permission, int requestCode)
        {
            if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] { permission }, requestCode);
                return false;
            }
            return true;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
          // base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == SCREEN_RECORD_REQUEST_CODE)
            {
                if (resultCode == Result.Ok)
                {
                    //Set file path or Uri depending on SDK version
                    SetOutputPath();
                    //Start screen recording
                    hbRecorder.StartScreenRecording(data, (int)resultCode, this);
                }
            }

        }

        public void HBRecorderOnComplete()
        {
            System.Diagnostics.Debug.WriteLine("Recording Completed.");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                //Update gallery depending on SDK Level
                if (hbRecorder.WasUriSet())
                {
                    updateGalleryUri();
                }
                else
                {
                    refreshGalleryFile();
                }
            }
        }


        [RequiresApi(Value = (int)BuildVersionCodes.Lollipop)]
        private void refreshGalleryFile()
        {
            //    new MediaScannerConnection.OnScanCompletedListener()
            //    {
            //            public void onScanCompleted(String path, Uri uri)
            //    {
            //        Log.i("ExternalStorage", "Scanned " + path + ":");
            //        Log.i("ExternalStorage", "-> uri=" + uri);
            //    }
            //});

        }

        private void updateGalleryUri()
        {
            //contentValues.clear();
            //contentValues.put(MediaStore.Video.Media.IS_PENDING, 0);
            //getContentResolver().update(mUri, contentValues, null, null);
        }


        public void HBRecorderOnError(int p0, string p1)
        {
            Debug.WriteLine($"Error occured during recording: {p1}");
            if (p0 == 38)
            {
                Debug.WriteLine("Some settings are not supported by your device");
            }
            else
            {
                Debug.WriteLine("HBRecorderOnError - See Log");
                //Log.e("HBRecorderOnError", reason);
            }
        }

        public void HBRecorderOnStart()
        {
            Debug.WriteLine($"Recording Started.");
        }

        [Obsolete]
        private void CustomSettings()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            //Is audio enabled
            bool audio_enabled = prefs.GetBoolean("key_record_audio", true);
            hbRecorder.IsAudioEnabled(audio_enabled);

            //Audio Source
            String audio_source = prefs.GetString("key_audio_source", null);
            if (audio_source != null)
            {
                switch (audio_source)
                {
                    case "0":
                        hbRecorder.SetAudioSource("DEFAULT");
                        break;
                    case "1":
                        hbRecorder.SetAudioSource("CAMCODER");
                        break;
                    case "2":
                        hbRecorder.SetAudioSource("MIC");
                        break;
                }
            }
            //Video Encoder
            String video_encoder = prefs.GetString("key_video_encoder", null);
            if (video_encoder != null)
            {
                switch (video_encoder)
                {
                    case "0":
                        hbRecorder.SetVideoEncoder("DEFAULT");
                        break;
                    case "1":
                        hbRecorder.SetVideoEncoder("H264");
                        break;
                    case "2":
                        hbRecorder.SetVideoEncoder("H263");
                        break;
                    case "3":
                        hbRecorder.SetVideoEncoder("HEVC");
                        break;
                    case "4":
                        hbRecorder.SetVideoEncoder("MPEG_4_SP");
                        break;
                    case "5":
                        hbRecorder.SetVideoEncoder("VP8");
                        break;
                }
            }

            //NOTE - THIS MIGHT NOT BE SUPPORTED SIZES FOR YOUR DEVICE
            //Video Dimensions
            String video_resolution = prefs.GetString("key_video_resolution", null);
            if (video_resolution != null)
            {
                switch (video_resolution)
                {
                    case "0":
                        hbRecorder.SetScreenDimensions(426, 240);
                        break;
                    case "1":
                        hbRecorder.SetScreenDimensions(640, 360);
                        break;
                    case "2":
                        hbRecorder.SetScreenDimensions(854, 480);
                        break;
                    case "3":
                        hbRecorder.SetScreenDimensions(1280, 720);
                        break;
                    case "4":
                        hbRecorder.SetScreenDimensions(1920, 1080);
                        break;
                }
            }

            //Video Frame Rate
            String video_frame_rate = prefs.GetString("key_video_fps", null);
            if (video_frame_rate != null)
            {
                switch (video_frame_rate)
                {
                    case "0":
                        hbRecorder.SetVideoFrameRate(60);
                        break;
                    case "1":
                        hbRecorder.SetVideoFrameRate(50);
                        break;
                    case "2":
                        hbRecorder.SetVideoFrameRate(48);
                        break;
                    case "3":
                        hbRecorder.SetVideoFrameRate(30);
                        break;
                    case "4":
                        hbRecorder.SetVideoFrameRate(25);
                        break;
                    case "5":
                        hbRecorder.SetVideoFrameRate(24);
                        break;
                }
            }

            //Video Bitrate
            String video_bit_rate = prefs.GetString("key_video_bitrate", null);
            if (video_bit_rate != null)
            {
                switch (video_bit_rate)
                {
                    case "1":
                        hbRecorder.SetVideoBitrate(12000000);
                        break;
                    case "2":
                        hbRecorder.SetVideoBitrate(8000000);
                        break;
                    case "3":
                        hbRecorder.SetVideoBitrate(7500000);
                        break;
                    case "4":
                        hbRecorder.SetVideoBitrate(5000000);
                        break;
                    case "5":
                        hbRecorder.SetVideoBitrate(4000000);
                        break;
                    case "6":
                        hbRecorder.SetVideoBitrate(2500000);
                        break;
                    case "7":
                        hbRecorder.SetVideoBitrate(1500000);
                        break;
                    case "8":
                        hbRecorder.SetVideoBitrate(1000000);
                        break;
                }
            }

            //Output Format
            String output_format = prefs.GetString("key_output_format", null);
            if (output_format != null)
            {
                switch (output_format)
                {
                    case "0":
                        hbRecorder.SetOutputFormat("DEFAULT");
                        break;
                    case "1":
                        hbRecorder.SetOutputFormat("MPEG_4");
                        break;
                    case "2":
                        hbRecorder.SetOutputFormat("THREE_GPP");
                        break;
                    case "3":
                        hbRecorder.SetOutputFormat("WEBM");
                        break;
                }
            }


        }


        //Get/Set the selected settings
        [RequiresApi(Value = (int)BuildVersionCodes.Lollipop)]
        public void QuickSettings()
        {
            hbRecorder.SetAudioBitrate(128000);
            hbRecorder.SetAudioSamplingRate(44100);
            hbRecorder.RecordHDVideo(false);
            hbRecorder.IsAudioEnabled(false);

            hbRecorder.SetNotificationSmallIcon(Resource.Drawable.icon);
            hbRecorder.SetNotificationTitle("Recording your screen");
            hbRecorder.SetNotificationDescription("Drag down to stop the recording");

        }
        //Generate a timestamp to be used as a file name
        private String GenerateFileName()
        {
            var epoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var name = $"VID_{epoch}";

            return name;
        }



        //For Android 10> we will pass a Uri to HBRecorder
        //This is not necessary - You can still use getExternalStoragePublicDirectory
        //But then you will have to add android:requestLegacyExternalStorage="true" in your Manifest
        //IT IS IMPORTANT TO SET THE FILE NAME THE SAME AS THE NAME YOU USE FOR TITLE AND DISPLAY_NAME
        ContentResolver resolver;
        ContentValues contentValues;
        Uri mUri;
        //Get/Set the selected settings
        [RequiresApi(Value = (int)BuildVersionCodes.Lollipop)]
        private void SetOutputPath()
        {
            String filename = GenerateFileName();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                resolver = this.ContentResolver;
                contentValues = new ContentValues();
                contentValues.Put(MediaStore.MediaColumns.RelativePath, "Movies/" + "HBRecorder");
                contentValues.Put(MediaStore.MediaColumns.Title, filename);
                contentValues.Put(MediaStore.MediaColumns.DisplayName, filename);
                contentValues.Put(MediaStore.MediaColumns.MimeType, "video/mp4");
                mUri = resolver.Insert(MediaStore.Video.Media.ExternalContentUri, contentValues);
                //FILE NAME SHOULD BE THE SAME
                hbRecorder.FileName = filename;
                hbRecorder.SetOutputUri(mUri);
            }
            else
            {
                CreateFolder();
                string path = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDocuments) + "/HBRecorder";

                hbRecorder.SetOutputPath(path);
            }
        }

        //Create Folder
        //Only call this on Android 9 and lower (getExternalStoragePublicDirectory is deprecated)
        //This can still be used on Android 10> but you will have to add android:requestLegacyExternalStorage="true" in your Manifest

        private void CreateFolder()
        {
            string outputFolder = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDocuments) + "";
            File f1 = new File(outputFolder, "HBRecorder");
            if (!f1.Exists())
            {
                if (f1.Mkdirs())
                {
                    Debug.WriteLine("Folder ", "created");
                }
            }
        }
        //remove this code

        public void StartScreenRecording()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                //first check if permissions was granted
                if (CheckSelfPermission(Manifest.Permission.RecordAudio, MainActivity.PERMISSION_REQ_ID_RECORD_AUDIO) && CheckSelfPermission(Manifest.Permission.WriteExternalStorage, MainActivity.PERMISSION_REQ_ID_WRITE_EXTERNAL_STORAGE))
                {
                    hasPermissions = true;
                }
                if (hasPermissions)
                {
                    //check if recording is in progress
                    //and stop it if it is
                    if (hbRecorder.IsBusyRecording)
                    {
                        hbRecorder.StopScreenRecording();
                    }
                    //else start recording
                    else
                    {
                        StartRecordingNow();
                    }
                }
            }
            else
            {
                //showLongToast("This library requires API 21>");
            }
        }
        private MediaProjectionManager mediaProjectionManager;
        public void StartRecordingNow()
        {
            QuickSettings();
            mediaProjectionManager = (MediaProjectionManager)GetSystemService(Context.MediaProjectionService);
            Intent permissionIntent = mediaProjectionManager != null ? mediaProjectionManager.CreateScreenCaptureIntent() : null;

            StartActivityForResult(permissionIntent, MainActivity.SCREEN_RECORD_REQUEST_CODE);

        }


        public void StopRecording()
        {
            hbRecorder.StopScreenRecording();
        }

    }

}