using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace KinectUwpSample {
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page {
        MediaFrameInput sensor;
        SoftwareBitmap colorBuffer;
        bool colorTaskRunning = false;

        public MainPage() {
            this.InitializeComponent();
            sensor = new MediaFrameInput();
            sensor.ColorFrameAquired += Sensor_ColorFrameAquired;
            sensor.DepthFrameAquired += Sensor_DepthFrameAquired;
            ColorImage.Source = new SoftwareBitmapSource();
        }

        private void Sensor_ColorFrameAquired(object sender, FrameAquiredEventArgs e) {
            var softwareBitmap = e.Frame.VideoMediaFrame?.SoftwareBitmap;
            if (softwareBitmap == null) {
                return;
            }

            if (softwareBitmap.BitmapPixelFormat != Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8 ||
                softwareBitmap.BitmapAlphaMode != Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied) {

                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            softwareBitmap = Interlocked.Exchange(ref colorBuffer, softwareBitmap);
            softwareBitmap?.Dispose();

            ColorImage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                if (colorTaskRunning) {
                    return;
                }
                colorTaskRunning = true;
                SoftwareBitmap latest;
                while ((latest = Interlocked.Exchange(ref colorBuffer, null)) != null) {
                    var imageSource = (SoftwareBitmapSource)ColorImage.Source;
                    await imageSource.SetBitmapAsync(latest);
                    latest.Dispose();
                }
                colorTaskRunning = false;
            });
        }

        private void Sensor_DepthFrameAquired(object sender, FrameAquiredEventArgs e) {
        }

        private async void Page_LoadedAsync(object sender, RoutedEventArgs e) {
            await sensor.InitializeAsync();
            await sensor.StartAsync();
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e) {
            await sensor.StopAsync();
        }
    }
}
