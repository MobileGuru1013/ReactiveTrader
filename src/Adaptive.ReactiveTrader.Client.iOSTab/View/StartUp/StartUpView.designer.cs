// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	[Register ("StartUpView")]
	partial class StartUpView
	{
		[Outlet]
		MonoTouch.UIKit.UIActivityIndicatorView Activity { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel Connecting { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel ErrorLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Activity != null) {
				Activity.Dispose ();
				Activity = null;
			}

			if (Connecting != null) {
				Connecting.Dispose ();
				Connecting = null;
			}

			if (ErrorLabel != null) {
				ErrorLabel.Dispose ();
				ErrorLabel = null;
			}
		}
	}
}
