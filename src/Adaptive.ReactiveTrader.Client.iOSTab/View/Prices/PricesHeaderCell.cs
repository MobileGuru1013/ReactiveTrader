using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public partial class PricesHeaderCell : UITableViewHeaderFooterView
	{
		public static readonly UINib Nib = UINib.FromName ("PricesHeaderCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("PricesHeaderCell");

		public PricesHeaderCell (IntPtr handle) : base (handle)
		{
		}

		public static PricesHeaderCell Create ()
		{
			return (PricesHeaderCell)Nib.Instantiate (null, null) [0];
		}

		private void DecorateWithEnabledness(Boolean isEnabled)
		{
			if (this.StatusSwitch.On != isEnabled) {
				this.StatusSwitch.On = isEnabled;
			}

			if (isEnabled) {
				this.ContainerView.BackgroundColor = Styles.RTTradeEnabled;
				this.StatusLabel.Text = "Trading is enabled";
			} else {
				this.ContainerView.BackgroundColor = Styles.RTTradeDisabled;
				this.StatusLabel.Text = "Trading is disabled";
			}
		}

		public void UpdateFrom (UserModel userModel)
		{
			userModel.OnChanged
				.Subscribe (OnItemChanged);
			this.DecorateWithEnabledness (userModel.OneTouchTradingEnabled);
		}

		private void OnItemChanged(UserModel item) {
			DecorateWithEnabledness(item.OneTouchTradingEnabled);
		}

		partial void SwitchValueChanged (MonoTouch.Foundation.NSObject sender)
		{
			UISwitch asSwitch = (UISwitch)sender;
			UserModel.Instance.OneTouchTradingEnabled = asSwitch.On;
		}
	}
}

