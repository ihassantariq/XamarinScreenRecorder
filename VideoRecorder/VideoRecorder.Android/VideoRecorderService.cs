using System;
using Android;
using Android.Content;
using Android.Media.Projection;
using Android.OS;
using Com.Hbisoft.Hbrecorder;
namespace VideoRecorder.Droid
{
    public class VideoRecorderService : IVideoRecorderService
    {
        //Declare HBRecorder
        private HBRecorder hbRecorder;
        private MainActivity _context;
        private MediaProjectionManager mediaProjectionManager;
        public VideoRecorderService(MainActivity activity, HBRecorder _hbRecorder)
        {
            hbRecorder = _hbRecorder;
            _context = activity;

        }

        public void StartScreenRecording()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                //first check if permissions was granted
                if (_context.CheckSelfPermission(Manifest.Permission.RecordAudio, MainActivity.PERMISSION_REQ_ID_RECORD_AUDIO) && _context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage, MainActivity.PERMISSION_REQ_ID_WRITE_EXTERNAL_STORAGE))
                {
                    _context.hasPermissions = true;
                }
                if (_context.hasPermissions)
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

        public void StartRecordingNow()
        {
            _context.QuickSettings();
            mediaProjectionManager = (MediaProjectionManager)_context.GetSystemService(Context.MediaProjectionService);
            Intent permissionIntent = mediaProjectionManager != null ? mediaProjectionManager.CreateScreenCaptureIntent() : null;

            _context.StartActivityForResult(permissionIntent, MainActivity.SCREEN_RECORD_REQUEST_CODE);

        }


        public void StopRecording()
        {
            hbRecorder.StopScreenRecording();
        }
    }

}
