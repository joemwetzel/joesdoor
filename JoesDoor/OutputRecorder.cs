using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.AVFoundation;
using MonoTouch.CoreVideo;
using MonoTouch.CoreMedia;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace JoesDoor
{
	public class OutputRecorder : AVCaptureVideoDataOutputSampleBufferDelegate { 	
		State _CurrentState;
		int KnockCount = 0;

		public OutputRecorder (State CurrentState) 
		{
			_CurrentState = CurrentState;
		}

		public override void DidOutputSampleBuffer (AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
		{
			try {
				connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;

				using (var image = ImageFromSampleBuffer (sampleBuffer)){
					if(_CurrentState.didKnock){
						KnockCount++;

						if(KnockCount==1){
							_CurrentState.CountDown = 5;

							InvokeOnMainThread (delegate {  
								_CurrentState.TopLabel.Text = "Knock Again to Post!!";
								_CurrentState.BottomLabel.Text = "Knock to Post: 5 sec";
							});

						}else if(KnockCount==40){
							_CurrentState.CountDown = 4;
							InvokeOnMainThread (delegate {
								_CurrentState.BottomLabel.Text = "Knock to Post: 4 sec";
							});
						}else if(KnockCount==80){
							_CurrentState.CountDown = 3;
							InvokeOnMainThread (delegate {
								_CurrentState.BottomLabel.Text = "Knock to Post: 3 sec";
							});
						}else if(KnockCount==120){
							_CurrentState.CountDown = 2;
							InvokeOnMainThread (delegate {  
								_CurrentState.BottomLabel.Text = "Knock to Post: 2 sec";
							});
						}else if(KnockCount==160){
							_CurrentState.CountDown = 1;
							InvokeOnMainThread (delegate {  
								_CurrentState.BottomLabel.Text = "Knock to Post: 1 sec";
							});
						}else if(KnockCount>200){
							InvokeOnMainThread (delegate {  
								_CurrentState.TopLabel.Text = "Follow @JoesDoor on Twitter";
								_CurrentState.BottomLabel.Text = "Knock to take a photo";
							});
							KnockCount=0;
							_CurrentState.CountDown = 0;
							_CurrentState.didKnock=false;

						}
					}else{
						InvokeOnMainThread(delegate {
							using (var pool = new NSAutoreleasePool ()) {
								_CurrentState.DisplayImageView.Image = image;
							}
						});
					}
				}
				sampleBuffer.Dispose ();
			} catch (Exception e){
				Console.WriteLine (e);
			}
		}



		UIImage ImageFromSampleBuffer (CMSampleBuffer sampleBuffer)
		{
			// Get the CoreVideo image
			using (var pool = new NSAutoreleasePool ()) {
				using (var pixelBuffer = sampleBuffer.GetImageBuffer () as CVPixelBuffer) {
					// Lock the base address
					pixelBuffer.Lock (0);
					// Get the number of bytes per row for the pixel buffer
					var baseAddress = pixelBuffer.BaseAddress;
					int bytesPerRow = pixelBuffer.BytesPerRow;
					int width = pixelBuffer.Width;
					int height = pixelBuffer.Height;
					var flags = CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little;
					// Create a CGImage on the RGB colorspace from the configured parameter above
					using (var cs = CGColorSpace.CreateDeviceRGB ())
					using (var context = new CGBitmapContext (baseAddress, width, height, 8, bytesPerRow, cs, (CGImageAlphaInfo)flags))
					using (var cgImage = context.ToImage ()) {
						pixelBuffer.Unlock (0);
						return UIImage.FromImage (cgImage);
					}
				}
			}
		}
	}
}

