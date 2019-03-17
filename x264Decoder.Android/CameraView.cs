using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace x264Decoder
{
    public class CameraView : TextureView, TextureView.ISurfaceTextureListener, ICameraView
    {
        private I264Decoder decoder;
        private IH264Parser parser;
        private Surface surface;
        private H264Connection connection;

        /// <summary>
        /// Used to keep track of a "Start" call before the view is ready (OnSurfaceTextureAvailable has been called).
        /// </summary>
        private bool shouldStart;

        private string address;
        private int port;

        private WeakReference<ICameraViewDelegate> cameraDelegate;

        public ICameraViewDelegate CameraDelegate {

            get {

                ICameraViewDelegate camDelegate = null;
                if (cameraDelegate != null && cameraDelegate.TryGetTarget(out camDelegate)) { return camDelegate; }
                return null;

            }

            set { cameraDelegate = new WeakReference<ICameraViewDelegate>(value); }

        }

        public bool IsRunning { get { return connection != null ? connection.IsRunning : false; } }

        public string Address { get { return address; } set { address = value; } }
        public int Port { get { return port; } set { port = value; } }

        public CameraView(Context context) : base(context)  {  setup(context); }

        public CameraView(Context context, IAttributeSet attrs) : base(context, attrs) { setup(context); }

        public CameraView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) { setup(context); }

        private void setup(Context context) {

            this.SurfaceTextureListener = this;

        }

        //protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) : base(widthMeasureSpec, heightMeasureSpec) {

        //}


        public void Start() {

            if (parser == null) {

                shouldStart = true;
                return;

            }

            if (address == null || port== 0) { throw new InvalidOperationException("Address & Port must be set prior to Start."); }

            connection = new H264Connection(address, port, parser);

            connection.OnError += (ex) => {


                ((Activity)Context).RunOnUiThread(() => {

                    if (CameraDelegate != null) { CameraDelegate.OnError(ex); }
                   
                });

            };

            connection.Start();

        }

        public void Stop() {

            connection.Stop();

        }


        public void OnSurfaceTextureAvailable(SurfaceTexture surfaceTexture, int width, int height) {

            if (IsRunning) { connection.Stop(); }

            surface = new Surface(surfaceTexture);
            decoder = new H264Decoder(surface);

            parser = new H264Parser(decoder);

            parser.LoadingComplete += (decoder) => {

                ((Activity)Context).RunOnUiThread(() => {

                    if (CameraDelegate != null) { CameraDelegate.OnLoadingComplete(decoder); }

                });

            };

            if (shouldStart) {

                shouldStart = false;
                Start(); 
            
            }

        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface) {

            if (connection != null) { connection.Stop(); }

            return true;

        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {

        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
           
        }

    }

}
