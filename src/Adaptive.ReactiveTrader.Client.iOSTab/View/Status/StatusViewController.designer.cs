// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace Adaptive.ReactiveTrader.Client.iOSTab.View
{
	[Register ("StatusViewController")]
	partial class StatusViewController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel ConnectionDetail { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel ConnectionStatus { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel ServerUpdateRate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel TraderId { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel UILatency { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel UIUpdateRate { get; set; }

		[Action ("LinkTouchUpInside:")]
		partial void LinkTouchUpInside (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (ConnectionDetail != null) {
				ConnectionDetail.Dispose ();
				ConnectionDetail = null;
			}

			if (ConnectionStatus != null) {
				ConnectionStatus.Dispose ();
				ConnectionStatus = null;
			}

			if (ServerUpdateRate != null) {
				ServerUpdateRate.Dispose ();
				ServerUpdateRate = null;
			}

			if (TraderId != null) {
				TraderId.Dispose ();
				TraderId = null;
			}

			if (UILatency != null) {
				UILatency.Dispose ();
				UILatency = null;
			}

			if (UIUpdateRate != null) {
				UIUpdateRate.Dispose ();
				UIUpdateRate = null;
			}
		}
	}
}
