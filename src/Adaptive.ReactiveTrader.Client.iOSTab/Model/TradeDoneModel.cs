using System;
using System.Linq;
using System.Reactive.Linq;
using MonoTouch.Foundation;
using Adaptive.ReactiveTrader.Client.Domain.Models.ReferenceData;
using Adaptive.ReactiveTrader.Client.Domain.Models.Pricing;
using Adaptive.ReactiveTrader.Client.Concurrency;
using Adaptive.ReactiveTrader.Shared.DTO.Pricing;
using Adaptive.ReactiveTrader.Shared.Extensions;
using Adaptive.ReactiveTrader.Client.Domain.Models.Execution;
using MonoTouch.SystemConfiguration;
using System.Dynamic;
using Adaptive.ReactiveTrader.Client.iOSTab.Tiles;
using System.IO;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;
using Adaptive.ReactiveTrader.Client.Domain.Repositories;

namespace Adaptive.ReactiveTrader.Client.iOSTab.Model
{
	public class TradeDoneModel
	{
		public ITrade Trade { get; private set; }

		public TradeDoneModel (ITrade trade)
		{
			Trade = trade;
		}
	}

}

