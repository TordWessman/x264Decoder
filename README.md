# C# / Xamarin x264Decoder

Library for playing a raw AVC (H.264) encoded byte-stream (tested on a Raspberry Pi) in Android & iOS.

Contains the following projects:
- x264Decoder.Shared: _Cross-platform shared code_
- x264Decoder.Android: _H.264 Decoding och playback for Android_
- x264Decoder.iOS: _H.264 Decoding och playback for iOS_


## iOS Usage

```         
	CameraView cameraView = new CameraView(new CoreGraphics.CGRect(0, 60, 320, 240));
    this.View.AddSubview(cameraView);
	cameraView.Address = "192.168.0.199";
	cameraView.Port = 4444;
	cameraView.Start(); 
```

## Android Usage

#### 1. Insert a CameraView layout
```
    <x264Decoder.CameraView
        android:layout_width="match_parent"
        android:layout_height="320dp"
        android:id="@+id/cameraView"/>
```

#### 2. Start the playback
```
	CameraView cameraView = FindViewById<CameraView>(Resource.Id.cameraView);
    cameraView.Address = "192.168.0.199";
    cameraView.Port = 4444;
    cameraView.Start();
```

## Server side on a Raspberry Pi (Raspbian GNU/Linux 9) on port 4444

#### Using raspivid

```bash
	$ raspivid -n -ih -t 0 -rot 0 -w 640 -h 480 -fps 15 -b 1000000 -o - | nc -lkv4 4444
```

#### Using Gstreamer

In order to use the gstreamer pipeline below, the following packages are required:
- gstreamer
- libgstreamer-plugins-base
- rpicamsrc

```bash
	$ sudo apt-get install gstreamer1.0-plugins-base gstreamer1.0-x

	$ git clone https://github.com/thaytan/gst-rpicamsrc.git
	$ cd gst-rpicamsrc
	$ ./autogen.sh --prefix=/usr --libdir=/usr/lib/arm-linux-gnueabihf/
	# if running on a 64-bit machine: $ ./autogen.sh --prefix=/usr --libdir=/usr/lib/x86_64-linux-gnu/
	$ make
	$ sudo make install
```

Launch the camera:

```
	$ gst-launch-1.0 rpicamsrc inline-headers=true preview=false bitrate=524288 keyframe-interval=30 ! video/x-h264, parsed=false, stream-format=\"byte-stream\", level=\"4\", profile=\"high\", framerate=30/1,width=640,height=480 ! queue leaky=2 ! tcpserversink host=0.0.0.0 port=4444
```

Disclaimer: This library has never been used in any application so far. Expect the unexpected.  