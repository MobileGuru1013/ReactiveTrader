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
	[Register ("PriceTileViewCell")]
	partial class PriceTileViewCell
	{
		[Outlet]
		MonoTouch.UIKit.UILabel CurrencyPair { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel Executing { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel LeftSideAction { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel LeftSideBigNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton LeftSideButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel LeftSideNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel LeftSidePips { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField Notional { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton NotionalCCY { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView PriceMovementDown { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView PriceMovementUp { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel RightSideAction { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel RightSideBigNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton RightSideButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel RightSideNumber { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel RightSidePips { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel SpotDate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel Spread { get; set; }

		[Action ("LeftSideButtonTouchUpInside:")]
		partial void LeftSideButtonTouchUpInside (MonoTouch.Foundation.NSObject sender);

		[Action ("NotionalCcyTouchUpInside:")]
		partial void NotionalCcyTouchUpInside (MonoTouch.Foundation.NSObject sender);

		[Action ("RightSideButtonTouchUpInside:")]
		partial void RightSideButtonTouchUpInside (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (LeftSideButton != null) {
				LeftSideButton.Dispose ();
				LeftSideButton = null;
			}

			if (RightSideButton != null) {
				RightSideButton.Dispose ();
				RightSideButton = null;
			}

			if (CurrencyPair != null) {
				CurrencyPair.Dispose ();
				CurrencyPair = null;
			}

			if (Executing != null) {
				Executing.Dispose ();
				Executing = null;
			}

			if (LeftSideAction != null) {
				LeftSideAction.Dispose ();
				LeftSideAction = null;
			}

			if (LeftSideBigNumber != null) {
				LeftSideBigNumber.Dispose ();
				LeftSideBigNumber = null;
			}

			if (LeftSideNumber != null) {
				LeftSideNumber.Dispose ();
				LeftSideNumber = null;
			}

			if (LeftSidePips != null) {
				LeftSidePips.Dispose ();
				LeftSidePips = null;
			}

			if (Notional != null) {
				Notional.Dispose ();
				Notional = null;
			}

			if (NotionalCCY != null) {
				NotionalCCY.Dispose ();
				NotionalCCY = null;
			}

			if (PriceMovementDown != null) {
				PriceMovementDown.Dispose ();
				PriceMovementDown = null;
			}

			if (PriceMovementUp != null) {
				PriceMovementUp.Dispose ();
				PriceMovementUp = null;
			}

			if (RightSideAction != null) {
				RightSideAction.Dispose ();
				RightSideAction = null;
			}

			if (RightSideBigNumber != null) {
				RightSideBigNumber.Dispose ();
				RightSideBigNumber = null;
			}

			if (RightSideNumber != null) {
				RightSideNumber.Dispose ();
				RightSideNumber = null;
			}

			if (RightSidePips != null) {
				RightSidePips.Dispose ();
				RightSidePips = null;
			}

			if (SpotDate != null) {
				SpotDate.Dispose ();
				SpotDate = null;
			}

			if (Spread != null) {
				Spread.Dispose ();
				Spread = null;
			}
		}
	}
}
