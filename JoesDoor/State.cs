using System;
using MonoTouch.UIKit;

namespace JoesDoor
{
	public class State
	{
		public State ()
		{
		}

		public UIImageView DisplayImageView {
			get;
			set;
		}

		public bool didKnock {
			get;
			set;
		}

		public int CountDown {
			get;
			set;
		}

		public UILabel BottomLabel{
			get;
			set;
		}

		public UILabel TopLabel{
			get;
			set;
		}

		public bool PublishToTwitter {
			get;
			set;
		}
	}
}

