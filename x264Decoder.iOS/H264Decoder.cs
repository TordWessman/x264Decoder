using System;
using System.Collections.Generic;
using System.Linq;
using AVFoundation;
using CoreMedia;
using Foundation;

namespace x264Decoder {

    public class H264Decoder : I264Decoder {

        private CMVideoFormatDescription formatDescriptor = null;
        private WeakReference<AVSampleBufferDisplayLayer> displayLayerRef;
        private int width;
        private int height;

        private const int IDRFrameDescriptor = 0x5;
        private const int NonIDRFrameDescriptor = 0x1;

        private int[] imageFrameTypes = new int[] { IDRFrameDescriptor, NonIDRFrameDescriptor };

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public H264Decoder(AVSampleBufferDisplayLayer displayLayer) {

            this.displayLayerRef = new WeakReference<AVSampleBufferDisplayLayer>(displayLayer);

            NSNotificationCenter.DefaultCenter.AddObserver(
                    AVSampleBufferDisplayLayer.FailedToDecodeNotification, (notification) =>
                    {
                        Console.WriteLine($"** AVC ERROR ** Received the notification AVSampleBufferDisplayLayer {notification}");

                    });

        }

        public bool GotFormatDescription { get { return formatDescriptor != null;  } }

        public unsafe bool Decode(int headerSize, byte[] nal) {

            if (formatDescriptor == null) {

                throw new InvalidOperationException("** AVC ERROR ** Unable to decode. Mediaformat not set. ParseHeader required.");

            }

            if (!imageFrameTypes.Contains(H264Parser.GetNalType(nal, headerSize))) {

                return true;  // Ignore non-image units.

            }

            int len = nal.Length - headerSize;
            byte[] bigLen = BitConverter.GetBytes((UInt32)len).Reverse().ToArray();
            Array.Copy(bigLen, nal, 4);

            CMBlockBufferError blockBufferError;
            CMBlockBuffer blockBuffer;

            fixed (byte* p = nal) {

                IntPtr ptr = (IntPtr)p;
                blockBuffer = CMBlockBuffer.FromMemoryBlock(ptr, (nuint)nal.Length, null, 0, (nuint)nal.Length, 0x0, out blockBufferError);
               
            }

            if (blockBufferError != CMBlockBufferError.None) {

                Console.WriteLine($"** AVC ERROR ** Unable to retrieve CMBlockBuffer. Error: {blockBufferError}");
                return false;

            }

            CMSampleBufferError sampleBufferError;
            CMSampleBuffer sampleBuffer;

            sampleBuffer = CMSampleBuffer.CreateReady(blockBuffer, formatDescriptor, 1, null, new nuint[] { (nuint)nal.Length }, out sampleBufferError);

            if (sampleBufferError != CMSampleBufferError.None) {

                Console.WriteLine($"** AVC ERROR ** Unable to retrieve CMBlockBuffer. Error: {blockBufferError}");
                return false;
            }

            CMSampleBufferAttachmentSettings[] settings = sampleBuffer.GetSampleAttachments(true);

            if (settings != null && settings.Length > 0) {

                settings[0].DisplayImmediately = true;

            }

            if (sampleBuffer != null && sampleBuffer.NumSamples > 0) {

                return DisplaySample(sampleBuffer);

            }

            return true;

        }

        public bool ParseHeader(int headerSize, byte[] pps, byte[] sps) {

            formatDescriptor = null;

            byte[] ppsWithoutHeader = new byte[pps.Length - headerSize];
            byte[] spsWithoutHeader = new byte[sps.Length - headerSize];
            Array.Copy(pps, headerSize, ppsWithoutHeader, 0, ppsWithoutHeader.Length);
            Array.Copy(sps, headerSize, spsWithoutHeader, 0, spsWithoutHeader.Length);

            List<byte[]> parameters = new List<byte[]>() { ppsWithoutHeader, spsWithoutHeader };

            int[] sizes = new int[] { ppsWithoutHeader.Length, spsWithoutHeader.Length };
            CMFormatDescriptionError formatDescriptionError;

            formatDescriptor = CMVideoFormatDescription.FromH264ParameterSets(parameters, headerSize, out formatDescriptionError);

            if (formatDescriptionError != CMFormatDescriptionError.None) {

                formatDescriptor = null;
                Console.WriteLine($"** AVC ERROR ** Unable to retrieve CMBlockBuffer. Error: {formatDescriptionError}");
                return false;

            }

            width = formatDescriptor.Dimensions.Width;
            height = formatDescriptor.Dimensions.Height;

            return true;

        }

        private bool DisplaySample(CMSampleBuffer sampleBuffer)  {

            AVSampleBufferDisplayLayer displayLayer = null;

            if (displayLayerRef.TryGetTarget(out displayLayer)) {

                displayLayer.InvokeOnMainThread(() => {

                    if (displayLayer.ReadyForMoreMediaData == true)
                    {

                        displayLayer.Enqueue(sampleBuffer);

                    }

                });

            } else {

                Console.WriteLine("** AVC ERROR ** Display layer has been deallocated");
                return false;

            }

            return true;

        }

    }

}
