using System;

//gst-launch-1.0 -v rpicamsrc inline-headers=true preview=false bitrate=524288 keyframe-interval=30 ! video/x-h264, parsed=false, stream-format=\"byte-stream\", level=\"4\", profile=\"high\", framerate=30/1,width=640,height=480 ! queue leaky=2 ! tcpserversink host=0.0.0.0 port=4444
namespace x264Decoder
{
    public interface ICameraViewDelegate {

        /// <summary>
        /// Called whenever the sub system encountered any non-recoverable error.
        /// </summary>
        /// <param name="ex">Ex.</param>
        void OnError(Exception ex);

        /// <summary>
        /// Called once just after the playback has buffered & received header information.
        /// </summary>
        /// <param name="decoder">Decoder.</param>
        void OnLoadingComplete(I264Decoder decoder);

    }

    public interface ICameraView {

        /// <summary>
        /// Delegate for camera events
        /// </summary>
        /// <value>The camera delegate.</value>
        ICameraViewDelegate CameraDelegate { get; set; }

        /// <summary>
        /// If true, the camera is currently playing
        /// </summary>
        /// <value><c>true</c> if is running; otherwise, <c>false</c>.</value>
        bool IsRunning { get; }

        /// <summary>
        /// The camera host address
        /// </summary>
        /// <value>The address.</value>
        string Address { get; set; }

        /// <summary>
        /// The camera host port
        /// </summary>
        /// <value>The port.</value>
        int Port { get; set; }

        /// <summary>
        /// Starts the playback by connecting to the camera using `Address` & `Port`
        /// </summary>
        void Start();

        /// <summary>
        /// Disconnect the camera
        /// </summary>
        void Stop();


    }

}
