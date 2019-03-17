using System;

namespace x264Decoder {

    /// <summary>
    /// Called when parser have received header parameters and is ready to decode video frames.
    /// </summary>
    public delegate void H264LoadingComplete(I264Decoder decoder);

    /// <summary>
    /// Used to retrieve nal packages from byte array. Intentended to be used as a state machine. Add raw video data from a AVC byte-stream using Parse.
    /// </summary>
    public interface IH264Parser {

        /// <summary>
        /// Add h264 data. `size` is the size of the byte array to use.
        /// Returns <see langword="false"/> if there was an error during parsing/decoding.
        /// </summary>
        /// <returns>The parse.</returns>
        /// <param name="data">Data.</param>
        /// <param name="size">Size.</param>
        bool Parse(Byte[] data, int size);

        /// <summary>
        /// Will be called once as soon as the header parameters and the first key frame has been received.
        /// ** Most likely to be called on a non-main thread **
        /// </summary>
        event H264LoadingComplete LoadingComplete;

        /// <summary>
        /// Call this method to reset the parser to it's empty state.
        /// </summary>
        void Reset();

    }

}