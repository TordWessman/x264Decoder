using System;
using System.Collections.Generic;
using System.Linq;

namespace x264Decoder
{
    public class H264Parser : IH264Parser {

        private List<byte> nal;
        private int zeroCount = 0;
        private I264Decoder decoder;
        private bool gotFirstIDR;
        private byte[] pps = null;
        private byte[] sps = null;
        private static readonly object parseLock = new object();

        private const int IDRFrameDescriptor = 0x5;
        private const int PPSDescriptor = 0x8;
        private const int SPSDescriptor = 0x7;

        public event H264LoadingComplete LoadingComplete;

        /// <summary>
        /// Number of 0x0 required before making a header check. Defaults to 2
        /// </summary>
        public int ZeroHeaderRequirements = 0x2;

        public static int GetNalType(byte[] nalUnit, int headerSize) {

            return nalUnit[headerSize] & 0x1F;

        }

        public H264Parser(I264Decoder decoder) {

            this.decoder = decoder;
            nal = new List<byte>();

        }

        public void Reset() {

            lock(parseLock) {

                pps = null;
                sps = null;
                gotFirstIDR = false;
                nal = new List<byte>();
                zeroCount = 0;

            }

        }

        public bool Parse(byte[] data, int size) {

            lock (parseLock) {

                for (int i = 0; i < size; i++) {

                    byte b = data[i];

                    nal.Add(b);

                    if (b == 0) { zeroCount++; }
                    else if (b != 1) { zeroCount = 0; }

                    if (b == 1 && zeroCount >= ZeroHeaderRequirements) {

                       if (!CheckNalUnit()) { return false;  }

                    }

                }

                return true;

            }

        }

        /// <summary>
        /// Returns false if there was a nal unit to parse, but and the parsing failed. Otherwise true.
        /// </summary>
        /// <returns><c>true</c>, if nal unit was checked, <c>false</c> otherwise.</returns>
        private bool CheckNalUnit() {

            int nalSizeWithoutLastHeader = nal.Count - (zeroCount + 1);

            if (nalSizeWithoutLastHeader > zeroCount)
            {

                byte[] nalBytes = new byte[nalSizeWithoutLastHeader];
                Array.Copy(nal.ToArray(), 0, nalBytes, 0, nalSizeWithoutLastHeader);

                if (!Process(nalBytes, (zeroCount + 1))) { return false; }

                nal = new List<byte>();
                for (int j = 0; j < zeroCount; j++) { nal.Add(0x0); }
                nal.Add(0x1);

            }

            zeroCount = 0;

            return true;

        }

        /// <summary>
        /// Check the nalUnit for type and delegate result to decoder.
        /// Returns false if the decoder encoundered an error, otherwise false.
        /// </summary>
        /// <returns>The process.</returns>
        /// <param name="nalUnit">Nal unit.</param>
        /// <param name="headerSize">Header size.</param>
        private bool Process(byte[] nalUnit, int headerSize) {

            int len = nalUnit.Length - headerSize;
            int nalType = H264Parser.GetNalType(nalUnit, headerSize);

            if (nalType == PPSDescriptor) {

                pps = nalUnit;

            } else if (nalType == SPSDescriptor) {

                sps = nalUnit;

            }

            if (pps != null && sps != null) {

                if (!decoder.ParseHeader(headerSize, pps, sps)) {

                    Console.WriteLine("** AVC ERROR ** Decoder was unable to parse header.");
                    return false;

                }

                pps = null;
                sps = null;

            }

            if (decoder.GotFormatDescription) {

                if (!decoder.Decode(headerSize, nalUnit)) {

                    Console.WriteLine("** AVC ERROR ** Decoder was unable to decode frame.");
                    return false;

                } else if (!gotFirstIDR && nalType == IDRFrameDescriptor) {

                    gotFirstIDR = true;
                    LoadingComplete(decoder);

                }

            }

            return true;

        }

    }

}
