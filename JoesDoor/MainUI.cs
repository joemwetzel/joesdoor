using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.AVFoundation;
using MonoTouch.CoreVideo;
using MonoTouch.CoreMedia;
using MonoTouch.CoreFoundation;
using RestSharp;
using System.Threading;

namespace JoesDoor
{
	public partial class MainUI : UIViewController
	{
		State _state;
		AVAudioRecorder recorder;
		NSUrl audioFilePath = null;
		NSTimer timer;
		NSTimer AudioLoop;
		bool isRecording = false;
		AVCaptureSession session;
		OutputRecorder outputRecorder;
		DispatchQueue queue;


		public MainUI (State ImageView) : base ("MainUI", null)
		{
			_state = ImageView;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		void PostToTwitter ()
		{
			//Goto https://apps.twitter.com/ and create a new app to post to.
			using (var pool = new NSAutoreleasePool ()) {
				using (NSData imageData = _state.DisplayImageView.Image.AsPNG ()) {
					Byte[] myByteArray = new Byte[imageData.Length];
					System.Runtime.InteropServices.Marshal.Copy (imageData.Bytes, myByteArray, 0, Convert.ToInt32 (imageData.Length));
					var client = new RestClient ("https://api.twitter.com");
					//Get all 4 token and secret vallues from your twitter add @ https://apps.twitter.com/
					client.Authenticator = RestSharp.Authenticators.OAuth1Authenticator.ForProtectedResource ("CustomerKey","CustomerSecret", "Token", "TokenSecret");
					var request = new RestRequest ("/1.1/statuses/update_with_media.json", Method.POST);
					request.AddFile ("media[]", myByteArray, "share.jpg");
					client.Execute (request);
				}
			}
		}

		void UserKnocked ()
		{
			_state.didKnock = true;
			if (_state.CountDown > 0 && _state.CountDown < 5 && _state.PublishToTwitter == false) {
				_state.PublishToTwitter = true;
				session.StopRunning ();
				session.Dispose ();
				session = null;
				outputRecorder.Dispose ();
				outputRecorder = null;
				queue.Dispose ();
				queue = null;

				//Stop Both Timers
				AudioLoop.Invalidate ();
				timer.Invalidate ();

				//Stop Recording
				StopRecording ();



				//Start up Resording
				NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (10), AudioReset);
				NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (1), PostToTwitter);
				_state.PublishToTwitter = true;
				_state.TopLabel.Text = "";
				_state.BottomLabel.Text = "Posted to Twitter!";
				imgYouRock.Hidden = false;

			}
		}

		void StartRecording ()
		{
			var session = AVAudioSession.SharedInstance ();
			NSError error = null;
			session.SetCategory (AVAudioSession.CategoryRecord, out error);
			if (error != null) {
				return;
			}
			session.SetActive (true, out error);
			if (error != null) {
				return;
			}
			if (!PrepareAudioRecording ()) {
				return;
			}
			if (!recorder.Record ()) {
				return;
			}

			timer = NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromSeconds (.1), UpdateMeter);
		}


		void StopRecording ()
		{
			recorder.Stop ();
			timer.Invalidate ();
		}

		void AudioReset ()
		{
			SetupCaptureSession ();
			StartRecording ();

			imgYouRock.Hidden = true;
			_state.PublishToTwitter = false;
			_state.didKnock = false;
			_state.CountDown = 0;
			_state.TopLabel.Text = "Follow @JoesDoor on Twitter";
			_state.BottomLabel.Text = "Knock to take a photo";

			AudioLoop = NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromSeconds (10), AudioUpdater);
			timer = NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromSeconds (.1), UpdateMeter);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			_state.DisplayImageView = imgDisplay;
			_state.TopLabel = lblTop;
			_state.BottomLabel = lblDisplay;

			using (var image = UIImage.FromBundle ("Images/yourock")){
				imgYouRock.Image = image;
			}

			imgYouRock.Hidden = true;

			AudioLoop = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(10), AudioUpdater);

			StartRecording ();
			SetupCaptureSession ();
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return false;
		}

		public void UpdateMeter(){
			try{
				recorder.UpdateMeters ();
				//lblDisplay.Text = recorder.PeakPower(0).ToString ();

				if(recorder.PeakPower(0) == 0){
					UserKnocked ();
				}
			}catch{
			}
		}

		public void AudioUpdater()
		{
			StopRecording ();
			//lblDisplay.Text = "";
			NSTimer.CreateScheduledTimer(TimeSpan.FromMilliseconds(100), StartRecording);

		}

		bool PrepareAudioRecording()
		{
			//Declare string for application temp path and tack on the file extension
			//string fileName = string.Format("Myfile{0}.aac", DateTime.Now.ToString("yyyyMMddHHmmss"));
			string fileName = "temp.aac";
			string tempRecording = NSBundle.MainBundle.BundlePath + "/../tmp/" + fileName;

			this.audioFilePath = NSUrl.FromFilename(tempRecording);

			var audioSettings = new AudioSettings() {
				SampleRate = 44100.0f, 
				Format = MonoTouch.AudioToolbox.AudioFormatType.MPEG4AAC,
				NumberChannels = 1,
				AudioQuality = AVAudioQuality.High
			};

			//Set recorder parameters
			NSError error;
			recorder = AVAudioRecorder.Create(this.audioFilePath, audioSettings, out error);
			if((recorder == null) || (error != null))
			{
				return false;
			}

			//Set Recorder to Prepare To Record
			if(!recorder.PrepareToRecord())
			{
				recorder.Dispose();
				recorder = null;
				return false;
			}

			recorder.FinishedRecording += delegate (object sender, AVStatusEventArgs e) {
				recorder.Dispose();
				recorder = null;
			};
			recorder.MeteringEnabled = true;
			return true;
		}

			bool SetupCaptureSession ()
			{
				// configure the capture session for low resolution, change this if your code
				// can cope with more data or volume
				session = new AVCaptureSession () {
					SessionPreset = AVCaptureSession.PresetMedium
				};


				AVCaptureDevice captureDevice = null;
				var videoDevices = AVCaptureDevice.DevicesWithMediaType (AVMediaType.Video);
				foreach (AVCaptureDevice Device in videoDevices) {
					if (Device.Position == AVCaptureDevicePosition.Front)
					{
						captureDevice = Device;
						break;
					}
				}



				// create a device input and attach it to the session
				if(captureDevice==null){
					captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType (AVMediaType.Video);
				}
				if (captureDevice == null){
					return false;
				}
				//Configure for 15 FPS. Note use of LockForConigfuration()/UnlockForConfiguration()
				NSError error = null;
				captureDevice.LockForConfiguration(out error);

				if(error != null)
				{
					captureDevice.UnlockForConfiguration();
					return false;
				}
				if(UIDevice.CurrentDevice.CheckSystemVersion(7,0))
					captureDevice.ActiveVideoMinFrameDuration = new CMTime (1,15);

			captureDevice.UnlockForConfiguration();

			var input = AVCaptureDeviceInput.FromDevice (captureDevice);

				if (input == null){
					return false;
				}

				session.AddInput (input);

				// create a VideoDataOutput and add it to the sesion
				var output = new AVCaptureVideoDataOutput () {
					VideoSettings = new AVVideoSettings (CVPixelFormatType.CV32BGRA),
				};


				// configure the output
				queue = new MonoTouch.CoreFoundation.DispatchQueue ("myQueue");
				outputRecorder = new OutputRecorder (_state);
				output.SetSampleBufferDelegate (outputRecorder, queue);
				session.AddOutput (output);

				session.StartRunning ();
				return true;
			}
		}
}

