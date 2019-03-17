using System;
using Android.Media;
using Android.Views;
using Java.Nio;

namespace x264Decoder {

        public class H264Decoder: I264Decoder {

        private MediaFormat mediaFormat;
        private MediaCodec decoder;
        private Surface surfaceView;

        private int width;
        private int height;
        private float fps;
        private int presentationTimeInc;
        private long presentationTimeUs;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public H264Decoder(Surface surfaceView) {

            this.surfaceView = surfaceView;
            this.decoder = MediaCodec.CreateDecoderByType(MediaFormat.MimetypeVideoAvc);

        }

        ~H264Decoder(){

            decoder.Stop();
            decoder.Release();

        }

        public bool Decode(int headerSize, byte[] nal) {

            if (mediaFormat == null) {

                throw new H264Exception("** AVC ERROR ** Unable to decode. Mediaformat not set. ParseHeader required.");

            }

            int inputBufferId = decoder.DequeueInputBuffer(0);

            if (inputBufferId >= 0) {

                ByteBuffer inputBuffer = decoder.GetInputBuffer(inputBufferId);

                inputBuffer.Put(nal);

                decoder.QueueInputBuffer(inputBufferId, 0, nal.Length, presentationTimeUs, 0);

                presentationTimeUs += presentationTimeInc;

            } 

            MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();

            int outputBufferId = 0; 

            while(outputBufferId >= 0) {

                outputBufferId = decoder.DequeueOutputBuffer(info, 0);

                if (outputBufferId >= 0) {

                    decoder.ReleaseOutputBuffer(outputBufferId, true);

                }

            }

            return true; 

        }

        public bool ParseHeader(int headerSize, byte[] pps, byte[] sps) {

            if (GotFormatDescription) { return true; }

            SPSParser spsParser = new SPSParser();
            spsParser.Parse(sps);
            width = spsParser.Width;
            height = spsParser.Height;
            fps = spsParser.Fps;

            presentationTimeInc = fps > 0 ? (1000000 / (int)fps) : 66666;
            presentationTimeUs = Java.Lang.JavaSystem.NanoTime() / 1000;

            mediaFormat = MediaFormat.CreateVideoFormat(MediaFormat.MimetypeVideoAvc, width, height);

            try {

                decoder.Configure(mediaFormat, surfaceView, null, 0);
                decoder.Start();
                Decode(headerSize, pps);
                Decode(headerSize, sps);

            } catch (Exception ex) {

                if (ex is Java.Lang.IllegalArgumentException ||
                    ex is Java.Lang.IllegalStateException ||
                    ex is MediaCodec.CryptoException) {

                    mediaFormat = null;
                    Console.WriteLine($"** AVC ERROR ** Got exception when parsing header: \n{ex}");
                    return false;

                }

                throw new H264Exception($"AVC ParseHeader Error: '{ex.Message}'", ex);

            }

            return true;

        }

        public bool GotFormatDescription { get { return mediaFormat != null; } }

    }

}
