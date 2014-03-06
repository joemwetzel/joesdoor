// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace JoesDoor
{
	[Register ("MainUI")]
	partial class MainUI
	{
		[Outlet]
		MonoTouch.UIKit.UIImageView imgDisplay { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView imgYouRock { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDisplay { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTop { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imgYouRock != null) {
				imgYouRock.Dispose ();
				imgYouRock = null;
			}

			if (imgDisplay != null) {
				imgDisplay.Dispose ();
				imgDisplay = null;
			}

			if (lblDisplay != null) {
				lblDisplay.Dispose ();
				lblDisplay = null;
			}

			if (lblTop != null) {
				lblTop.Dispose ();
				lblTop = null;
			}
		}
	}
}
