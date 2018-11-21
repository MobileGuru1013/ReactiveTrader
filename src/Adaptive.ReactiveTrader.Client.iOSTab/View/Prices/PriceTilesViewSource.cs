

using System.Collections.Generic;
using Adaptive.ReactiveTrader.Client.Domain.Models;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Concurrency;
using System.IO;
using Adaptive.ReactiveTrader.Client.iOSTab.Tiles;
using Adaptive.ReactiveTrader.Shared.UI;


namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public class PriceTilesViewSource : UITableViewSource
	{
		private readonly PriceTilesModel priceTilesModel;

		public PriceTilesViewSource (PriceTilesModel priceTilesModel)
		{
			this.priceTilesModel = priceTilesModel;
		}


		public override int NumberOfSections (UITableView tableView)
		{
			return 1;
		}


		public override int RowsInSection (UITableView tableview, int section)
		{
			return priceTilesModel.ActiveCurrencyPairs.Count;
		}


		public override float GetHeightForHeader (UITableView tableView, int section)
		{
			return 60.0f;
		}


		public override UIView GetViewForHeader (UITableView tableView, int section)
		{
			// NOTE: Don't call the base implementation on a Model class
			// see http://docs.xamarin.com/guides/ios/application_fundamentals/delegates,_protocols,_and_events

			PricesHeaderCell dequeued = tableView.DequeueReusableHeaderFooterView (PricesHeaderCell.Key) as PricesHeaderCell;
			dequeued.UpdateFrom (UserModel.Instance);
			return dequeued;
		}


		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			PriceTileModel model = priceTilesModel [indexPath.Row];

			var cell = GetCell (tableView, model);

			cell.UpdateFrom (model);

			return ( UITableViewCell)cell;
		}


		private IPriceTileCell GetCell(UITableView tableView, PriceTileModel model) {
			IPriceTileCell priceTileCell = null;

			switch (model.Status) {
			case PriceTileStatus.Done:
			case PriceTileStatus.DoneStale:
				priceTileCell = tableView.DequeueReusableCell (PriceTileTradeAffirmationViewCell.Key) as PriceTileTradeAffirmationViewCell;
				if (priceTileCell == null) {
					priceTileCell = PriceTileTradeAffirmationViewCell.Create ();
				}
				break;

			case PriceTileStatus.Streaming:
			case PriceTileStatus.Executing:
				priceTileCell = tableView.DequeueReusableCell (PriceTileViewCell.Key) as PriceTileViewCell;
				if (priceTileCell == null) {
					priceTileCell = PriceTileViewCell.Create ();
				}
				break;

			case PriceTileStatus.Stale:
				priceTileCell = tableView.DequeueReusableCell (PriceTileErrorViewCell.Key) as PriceTileViewCell;
				if (priceTileCell == null) {
					priceTileCell = PriceTileErrorViewCell.Create ();
				}
				break;
			}

			return priceTileCell;
		}

		/*
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			// For now all rows are the same height, set via ConfigureTable.
		}*/
	}
}

