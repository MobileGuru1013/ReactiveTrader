
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Concurrency;
using System.Linq;
using Adaptive.ReactiveTrader.Client.iOSTab.Tiles;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	//[Register("PriceTilesViewController")]
	public partial class PriceTilesViewController : UITableViewController
	{
		private readonly IReactiveTrader _reactiveTrader;
		private readonly IConcurrencyService _concurrencyService;
		private readonly PriceTilesModel _model;

		public PriceTilesViewController (IReactiveTrader reactiveTrader, IConcurrencyService concurrencyService) 
			: base(UITableViewStyle.Plain)
		{
			this._concurrencyService = concurrencyService;
			this._reactiveTrader = reactiveTrader;

			Title = "Prices";
			TabBarItem.Image = UIImage.FromBundle ("tab_prices");

			_model = new PriceTilesModel (_reactiveTrader, _concurrencyService);

			_model.ActiveCurrencyPairs.CollectionChanged += (sender, e) => {
				foreach (var model in e.NewItems.Cast<PriceTileModel>()) {
					model.OnChanged
						.Subscribe (OnItemChanged);
				}
				if (IsViewLoaded) {
					TableView.ReloadData ();
				}
			};
			_model.Initialise ();

		}

		private void OnItemChanged(PriceTileModel itemModel) {

			if (IsViewLoaded) {
				var indexOfItem = _model.ActiveCurrencyPairs.IndexOf (itemModel);

				NSIndexPath path = NSIndexPath.FromRowSection(indexOfItem, 0);
				IPriceTileCell cell = (IPriceTileCell)TableView.CellAt (path);

				if (cell == null) {
					//					System.Console.WriteLine ("Row {0} not found", indexOfItem);
					// There's no cell bound to that index in the data, so we can ignore the update.
				} else {
					//					System.Console.WriteLine ("Row {0} FOUND {1}", indexOfItem, cell.GetType ().ToString ());

					bool bAppropriateCell = false; // TODO: Refactor this elsewhere.

					switch (itemModel.Status) {
					case PriceTileStatus.Done:
					case PriceTileStatus.DoneStale:
						if (cell.GetType ().Equals (Type.GetType ("Adaptive.ReactiveTrader.Client.iOSTab.PriceTileTradeAffirmationViewCell", false))) {
							bAppropriateCell = true;
						}
						break;

					case PriceTileStatus.Streaming:
					case PriceTileStatus.Executing:
						if (cell.GetType ().Equals (Type.GetType ("Adaptive.ReactiveTrader.Client.iOSTab.PriceTileViewCell", false))) {
							bAppropriateCell = true;
						}
						break;

					case PriceTileStatus.Stale:
						if (cell.GetType ().Equals (Type.GetType ("Adaptive.ReactiveTrader.Client.iOSTab.PriceTileErrorViewCell", false))) {
							bAppropriateCell = true;
						}
						break;
					}

					// TODO: Batch the updates up, to only call ReloadRows once per main event loop loop?

					if (bAppropriateCell) {
						//						System.Console.WriteLine ("Cell is APPROPRIATE", indexOfItem);
						cell.UpdateFrom (itemModel);
					} else {
						// TODO: If the cell is of the wrong type, reload the row instead.

						TableView.ReloadRows (
							new [] {
								NSIndexPath.Create (0, indexOfItem)
							}, UITableViewRowAnimation.None);
					}
				}

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

			TableView.RegisterNibForHeaderFooterViewReuse (PricesHeaderCell.Nib, PricesHeaderCell.Key);

			TableView.Source = new PriceTilesViewSource (_model);

			Styles.ConfigureTable (TableView);
		}


		// Workaround: Prevent UI from incorrectly extending under tab bar.

		public override UIRectEdge EdgesForExtendedLayout {
			get {
				return (base.EdgesForExtendedLayout ^ UIRectEdge.Bottom);
			}
		}

	}
}

