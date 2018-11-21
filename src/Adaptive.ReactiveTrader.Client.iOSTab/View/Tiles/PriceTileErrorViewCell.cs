using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Adaptive.ReactiveTrader.Client.iOSTab.Tiles
{
	public partial class PriceTileErrorViewCell : UITableViewCell, IPriceTileCell
	{
		public static readonly UINib Nib = UINib.FromName ("PriceTileErrorViewCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("PriceTileErrorViewCell");

		private PriceTileModel _priceTileModel;

		public PriceTileErrorViewCell (IntPtr handle) : base (handle)
		{
		}

		public static PriceTileErrorViewCell Create ()
		{
			PriceTileErrorViewCell created = (PriceTileErrorViewCell)Nib.Instantiate (null, null) [0];
			created.ContentView.BackgroundColor = Styles.RTFailRed;
			return created;
		}

		public void UpdateFrom (PriceTileModel model)
		{
			_priceTileModel = model;

			this.CurrencyPair.Text = model.Symbol;
		}
	}
}

