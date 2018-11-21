﻿using System;
using Adaptive.ReactiveTrader.Client.Domain;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Concurrency;
using System.Runtime.Remoting.Channels;
using System.Reactive.Disposables;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Client.Domain.Models.Execution;
using System.Collections.Generic;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;
using MonoTouch.Foundation;
using System.Collections.ObjectModel;

namespace Adaptive.ReactiveTrader.Client.iOSTab.Model
{
	public class TradeTilesModel : IDisposable
	{
		private readonly IReactiveTrader _reactiveTrader;
		private readonly IConcurrencyService _concurrencyService;
		private readonly CompositeDisposable _disposables = new CompositeDisposable();
		private readonly ObservableCollection<TradeDoneModel> _doneTrades = new ObservableCollection<TradeDoneModel> ();

		public TradeTilesModel (IReactiveTrader reactiveTrader, IConcurrencyService concurrencyService)
		{
			_reactiveTrader = reactiveTrader;
			_concurrencyService = concurrencyService;
		}

		public void Initialise() {
			_disposables.Add (
				_reactiveTrader.TradeRepository
					.GetTradesStream ()
					.Select((trades, i) => new { trades, isSotw = i == 0})
					.SubscribeOn(_concurrencyService.TaskPool)
					.ObserveOn(_concurrencyService.Dispatcher)
					.Subscribe(update => OnTradeUpdates(update.trades, update.isSotw))
			);
		}

		public ObservableCollection<TradeDoneModel> DoneTrades  { get { return _doneTrades; } }

		public int Count { 
			get {
				return _doneTrades.Count;
			}
		}

		public TradeDoneModel this[int index] {
			get { 
				return _doneTrades [index];
			}
		}

		public void Dispose ()
		{
			_disposables.Dispose();
		}

		private void OnTradeUpdates(IEnumerable<ITrade> trades, bool isSotw) {
			foreach (var trade in trades) {
				var td = new TradeDoneModel (trade);
				_doneTrades.Insert (0, td);
//				if (!isSotw) {
//					_table.InsertRows (new [] { NSIndexPath.Create (0, 0)}, UITableViewRowAnimation.Top);
//				}
			}

//			if (isSotw) {
//				_table.ReloadData ();
//			}

		}
	}
}

