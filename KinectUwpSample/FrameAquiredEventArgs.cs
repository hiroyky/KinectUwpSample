using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;

namespace KinectUwpSample {
    public class FrameAquiredEventArgs : EventArgs {
        public MediaFrameReference Frame { get; }

        public FrameAquiredEventArgs(MediaFrameReference frame) {
            this.Frame = frame;
        }
    }
}
