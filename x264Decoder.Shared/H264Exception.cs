using System;

namespace x264Decoder {

    public class H264Exception : ApplicationException {

        public H264Exception(Exception exception) : base($"H264 Internal exception: `{exception.Message}`", exception) { }

        public H264Exception(string message) : base(message) { }

        public H264Exception(string message, Exception exception) : base(message, exception) { }

    }

}
