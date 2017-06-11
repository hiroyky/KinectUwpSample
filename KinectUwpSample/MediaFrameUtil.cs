using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;

namespace KinectUwpSample {
    public class MediaFrameUtil {
        public static string GetSubtypeForFrameReader(MediaFrameSourceKind kind, MediaFrameFormat format) {
            switch (kind) {
                case MediaFrameSourceKind.Color:
                    return MediaEncodingSubtypes.Bgra8;
                case MediaFrameSourceKind.Depth:
                    return String.Equals(format.Subtype, MediaEncodingSubtypes.D16, StringComparison.OrdinalIgnoreCase) ?
                        format.Subtype : null;
                default:
                    return null;
            }
        }
    }
}

