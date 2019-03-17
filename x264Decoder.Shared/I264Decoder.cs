using System;
using System.Collections.Generic;

namespace x264Decoder {

    public interface I264Decoder {

        /// <summary>
        /// Decode the nal unit. The nal units should contain the nalu header according to the byte stream format specifications for avc (i.e. 0x0000001).
        /// headerSize is the size of the nalu header.
        /// Returns <see langword="false"/> if any non-fatal error occured.
        /// </summary>
        /// <returns>The decode.</returns>
        /// <param name="headerSize">Header size.</param>
        /// <param name="nal">Nal.</param>
        bool Decode(int headerSize, byte[] nal);

        /// <summary>
        /// When pps & sps units has been received, this method is called. The sps & pps should contain the nalu header according to the byte stream format specifications for avc (i.e. 0x0000001).
        /// headerSize is the size of the nalu header.
        /// Returns <see langword="false"/> if any non-fatal error occured.
        /// </summary>
        /// <returns><c>true</c>, if header was parsed, <c>false</c> otherwise.</returns>
        /// <param name="headerSize">Header size.</param>
        /// <param name="pps">Pps.</param>
        /// <param name="sps">Sps.</param>
        bool ParseHeader(int headerSize, byte[] pps, byte[] sps);

        /// <summary>
        /// If true, the decoder has received the video format descriptor (sps & pps) and can start to decode video frames.
        /// </summary>
        /// <value><c>true</c> if got format description; otherwise, <c>false</c>.</value>
        bool GotFormatDescription { get; }

        /// <summary>
        /// The width of the video stream.
        /// </summary>
        /// <value>The width.</value>
        int Width { get; }

        /// <summary>
        /// Height of the video stream.
        /// </summary>
        /// <value>The height.</value>
        int Height { get; }

    }

}
