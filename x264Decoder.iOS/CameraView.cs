using System;
using UIKit;
using AVFoundation;
using Foundation;
using System.ComponentModel;
using CoreGraphics;

namespace x264Decoder {

    [Register("CameraView"), DesignTimeVisible(true)]
    public class CameraView: UIView, ICameraView {

        private I264Decoder decoder;
        private IH264Parser parser;
        private H264Connection connection;
        private AVSampleBufferDisplayLayer displayLayer;
        private string address;
        private int port;

        private WeakReference<ICameraViewDelegate> cameraDelegate;

        public ICameraViewDelegate CameraDelegate {

            get  {

                ICameraViewDelegate camDelegate = null;
                if (cameraDelegate != null && cameraDelegate.TryGetTarget(out camDelegate)) { return camDelegate; }
                return null;

            }

            set { cameraDelegate = new WeakReference<ICameraViewDelegate>(value); }

        }

        public bool IsRunning { get { return connection != null ? connection.IsRunning : false; } }
        public string Address { get { return address; } set { address = value; } }
        public int Port { get { return port; } set { port = value; } }

        public CameraView(IntPtr p) : base(p) { Setup(); }
        public CameraView() : base() { Setup(); }
        public CameraView(CGRect frame) : base(frame) { Setup(); }

        ~CameraView() {

            connection.Stop();

        }

        private void Setup() {

            if (displayLayer == null) {

                displayLayer = new AVSampleBufferDisplayLayer();
                displayLayer.VideoGravity = "AVLayerVideoGravityResize";
                displayLayer.Frame = Bounds;
                Layer.AddSublayer(displayLayer);

            }

            connection = null;
            decoder = new H264Decoder(displayLayer);
            parser = new H264Parser(decoder);

            parser.LoadingComplete += (dec) => {

                InvokeOnMainThread(() => {

                    if (CameraDelegate != null) { CameraDelegate.OnLoadingComplete(decoder); }

                });

            };

            SetNeedsDisplay();

        }

        public void Start() {

           if (IsRunning) { Stop(); }

            if (address == null || port == 0) { throw new InvalidOperationException("Address & Port must be set prior to Start."); }

            connection = new H264Connection(address, port, parser);

            connection.OnError += (ex) => {

                InvokeOnMainThread(() => {

                    if (CameraDelegate != null) { CameraDelegate.OnError(ex); }

                });

            };

            connection.Start();

        }

        public void Stop() {

            connection.Stop();

        }

        public override void WillMoveToSuperview(UIView newsuper) {

            base.WillMoveToSuperview(newsuper);

            displayLayer.Frame = this.Bounds;

        }

    }

}
