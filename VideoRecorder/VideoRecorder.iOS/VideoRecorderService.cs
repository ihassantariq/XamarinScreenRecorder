using System;
using AVFoundation;
using ReplayKit;
using System.Linq;
using Foundation;
using UIKit;
using System.IO;
using VideoRecorder.iOS;
using AssetsLibrary;
using System.Diagnostics;

namespace VideoRecorder.iOS
{
    //It is failing few times have to look at it
    public class VideoRecorderService : IVideoRecorderService
    {
        RPScreenRecorder rp = RPScreenRecorder.SharedRecorder;
        int numberOfVideos = 0;
        AVAssetWriter assetWriter;
        AVAssetWriterInput videoInput;
        VideoSettings videoSettings;


        public void StartScreenRecording()
        {
            videoSettings = new VideoSettings();
            NSError wError;
            assetWriter = new AVAssetWriter(videoSettings.OutputUrl, AVFileType.AppleM4A, out wError);
            videoInput = new AVAssetWriterInput(AVMediaType.Video, videoSettings.OutputSettings);

            videoInput.ExpectsMediaDataInRealTime = true;

            assetWriter.AddInput(videoInput);

            if (rp.Available)
            {
                rp.StartCaptureAsync((buffer, sampleType, error) =>
                {
                    try
                    {
                        if (buffer.DataIsReady)
                        {

                            if (assetWriter.Status == AVAssetWriterStatus.Unknown)
                            {

                                assetWriter.StartWriting();

                                assetWriter.StartSessionAtSourceTime(buffer.PresentationTimeStamp);

                            }

                            if (assetWriter.Status == AVAssetWriterStatus.Failed)
                            {
                                return;
                            }

                            if (sampleType == RPSampleBufferType.Video)
                            {
                                if (videoInput.ReadyForMoreMediaData)
                                {
                                    videoInput.AppendSampleBuffer(buffer);
                                }
                            }

                        }
                    }
                    finally
                    {
                        buffer.Dispose();
                    }

                });
            }

        }

        public void StopRecording()
        {
            rp.StopCapture((error) =>
            {
                if (error == null)
                {
                    if (assetWriter.Status != AVAssetWriterStatus.Unknown)
                        assetWriter.FinishWriting(() => { });
                }
            });


        }
        public async void MoveFinishedMovieToAlbum()
        {
            try
            {
                var lib = new ALAssetsLibrary();
                Debug.WriteLine($"URL:{videoSettings.OutputUrl}");
                NSUrl url = await lib.WriteVideoToSavedPhotosAlbumAsync(videoSettings.OutputUrl);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }

    public class VideoSettings
    {
        public string VideoFilename => "ScreenCapturing";
        public string VideoFilenameExt = "mp4";
        public nfloat Width { get; set; }
        public nfloat Height { get; set; }
        public AVVideoCodec AvCodecKey => AVVideoCodec.H264;

        public NSUrl OutputUrl
        {
            get; set;

        }

        NSUrl CreateFileName(string name, string extension, long index = 0)
        {
            NSError error;
            extension = extension.ToLowerInvariant();
            var docs = new NSFileManager().GetUrl(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, null, true, out error).ToString();

            var epoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var postpendName = index == 0 ? string.Empty : $"{index}";
            postpendName += Math.Abs(epoch);

            name = $"{name}_VID_{postpendName}.{extension ?? "mp4"}";

            if (error == null)
            {
                return new NSUrl(Path.Combine(docs, name));
            }
            return null;
        }
        public VideoSettings()
        {
            OutputUrl = CreateFileName(VideoFilename, VideoFilenameExt, 0);
            Debug.WriteLine($"URL:{OutputUrl}");
        }


        public AVVideoSettingsCompressed OutputSettings
        {
            get
            {
                return new AVVideoSettingsCompressed
                {
                    Codec = AvCodecKey,
                    Width = Convert.ToInt32(UIScreen.MainScreen.Bounds.Size.Width),
                    Height = Convert.ToInt32(UIScreen.MainScreen.Bounds.Size.Height)
                };
            }
        }
    }


}
