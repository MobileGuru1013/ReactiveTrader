
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public partial class StartUpView : UIViewController
	{
		private bool _isActive;
		private string _title;
		private string _error;

		public StartUpView () : base ("StartUpView", null)
		{
		}

		public void DisplayMessages(bool isActive, string title, string error = null) {
			_isActive = isActive;
			_title = title;
			_error = error;
			ShowMessages ();
		}

		private void ShowMessages() {
			if (IsViewLoaded) {
				if (_isActive) {
					this.Activity.StartAnimating ();
				} else {
					this.Activity.StopAnimating ();
				}
				this.Connecting.Text = _title;
				ErrorLabel.Text = _error;
			}
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.Activity.HidesWhenStopped = true;

			ShowMessages ();
			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}

