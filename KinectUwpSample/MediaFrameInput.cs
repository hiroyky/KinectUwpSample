using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;

namespace KinectUwpSample {
    public class MediaFrameInput : IDisposable, ISensorDevice {
        public event EventHandler<FrameAquiredEventArgs> ColorFrameAquired;
        public event EventHandler<FrameAquiredEventArgs> DepthFrameAquired;

        MediaCapture mediaCapture;
        List<MediaFrameReader> frameReaders = new List<MediaFrameReader>();

        public async Task InitializeAsync() {
            var allGroups = await MediaFrameSourceGroup.FindAllAsync();
            await initializeMediaCapterAsync(allGroups[0]);
            await initializeFrameReaders();
        }

        public async Task StartAsync() {
            foreach (var reader in frameReaders) {
                var status = await reader.StartAsync();
                if (status != MediaFrameReaderStartStatus.Success) {
                    throw new Exception("Failed to start sensor. " + status.ToString());
                }
            }
        }

        public async Task StopAsync() {
            if (mediaCapture != null) {
                mediaCapture = null;
                foreach (var reader in frameReaders) {
                    reader.FrameArrived -= FrameReader_FrameArrived;
                    await reader.StopAsync();
                    reader.Dispose();
                }
            }
        }

        async Task initializeMediaCapterAsync(MediaFrameSourceGroup sourceGroup) {
            if (mediaCapture != null) {
                return;
            }
            mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings {
                SourceGroup = sourceGroup,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu
            };
            await mediaCapture.InitializeAsync(settings);
        }

        async Task initializeFrameReaders() {
            string requestedSubtype = null;
            foreach (var source in mediaCapture.FrameSources.Values) {
                var kind = source.Info.SourceKind;
                foreach (var format in source.SupportedFormats) {
                    requestedSubtype = MediaFrameUtil.GetSubtypeForFrameReader(kind, format);
                    if (requestedSubtype != null) {
                        await source.SetFormatAsync(format);
                        break;
                    }
                }
                if (requestedSubtype == null) {
                    continue;
                }

                var frameReader = await mediaCapture.CreateFrameReaderAsync(source, requestedSubtype);
                frameReader.FrameArrived += FrameReader_FrameArrived;
                frameReaders.Add(frameReader);
            }
        }

        void FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args) {
            using (var frame = sender.TryAcquireLatestFrame()) {
                if (frame == null) {
                    return;
                }

                switch (frame.SourceKind) {
                    case MediaFrameSourceKind.Color:
                        ColorFrameAquired?.Invoke(this, new FrameAquiredEventArgs(frame));
                        break;
                    case MediaFrameSourceKind.Depth:
                        DepthFrameAquired?.Invoke(this, new FrameAquiredEventArgs(frame));
                        break;
                }
            }
        }

        public async void Dispose() {
            await StopAsync();
        }
    }
}
