<Query Kind="Statements">
  <Reference Relative="Adaptive.ReactiveTrader.Client.Domain\bin\Debug\Adaptive.ReactiveTrader.Client.DomainPortable.dll">&lt;MyDocuments&gt;\GitHub\ReactiveTrader\src\Adaptive.ReactiveTrader.Client.Domain\bin\Debug\Adaptive.ReactiveTrader.Client.DomainPortable.dll</Reference>
  <Reference Relative="Adaptive.ReactiveTrader.Client.Domain\bin\Debug\Adaptive.ReactiveTrader.Shared.dll">&lt;MyDocuments&gt;\GitHub\ReactiveTrader\src\Adaptive.ReactiveTrader.Client.Domain\bin\Debug\Adaptive.ReactiveTrader.Shared.dll</Reference>
  <Reference Relative="Adaptive.ReactiveTrader.Client.Domain\bin\Debug\Microsoft.AspNet.SignalR.Client.dll">&lt;MyDocuments&gt;\GitHub\ReactiveTrader\src\Adaptive.ReactiveTrader.Client.Domain\bin\Debug\Microsoft.AspNet.SignalR.Client.dll</Reference>
  <Reference Relative="Adaptive.ReactiveTrader.Client.Domain\bin\Debug\Newtonsoft.Json.dll">&lt;MyDocuments&gt;\GitHub\ReactiveTrader\src\Adaptive.ReactiveTrader.Client.Domain\bin\Debug\Newtonsoft.Json.dll</Reference>
  <Reference Relative="Adaptive.ReactiveTrader.Client.Domain\bin\Debug\System.Reactive.Core.dll">&lt;MyDocuments&gt;\GitHub\ReactiveTrader\src\Adaptive.ReactiveTrader.Client.Domain\bin\Debug\System.Reactive.Core.dll</Reference>
  <Reference Relative="Adaptive.ReactiveTrader.Client.Domain\bin\Debug\System.Reactive.Interfaces.dll">&lt;MyDocuments&gt;\GitHub\ReactiveTrader\src\Adaptive.ReactiveTrader.Client.Domain\bin\Debug\System.Reactive.Interfaces.dll</Reference>
  <Reference Relative="Adaptive.ReactiveTrader.Client.Domain\bin\Debug\System.Reactive.Linq.dll">&lt;MyDocuments&gt;\GitHub\ReactiveTrader\src\Adaptive.ReactiveTrader.Client.Domain\bin\Debug\System.Reactive.Linq.dll</Reference>
  <Reference Relative="Adaptive.ReactiveTrader.Client.Domain\bin\Debug\System.Reactive.PlatformServices.dll">&lt;MyDocuments&gt;\GitHub\ReactiveTrader\src\Adaptive.ReactiveTrader.Client.Domain\bin\Debug\System.Reactive.PlatformServices.dll</Reference>
  <Namespace>Adaptive.ReactiveTrader.Client.Domain</Namespace>
  <Namespace>Adaptive.ReactiveTrader.Client.Domain.Models</Namespace>
  <Namespace>Adaptive.ReactiveTrader.Client.Domain.Models.Execution</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

var api = new ReactiveTrader();
api.Initialize("Trader1", new []{"http://localhost:8080"});

api.ConnectionStatusStream.DumpLatest("Connection");

var prices = api.ReferenceData.GetCurrencyPairsStream()
    .SelectMany(_ => _)
    .SelectMany(p => p.CurrencyPair.PriceStream)
    .Where(o => !o.IsStale)
    .Select(p => new { CurrencyPair = p.CurrencyPair.Symbol, p.Mid })
    .Scan(new ConcurrentDictionary<string, decimal>(), (acc, cur)=> { acc.AddOrUpdate(cur.CurrencyPair, cur.Mid, (k, u)=>cur.Mid); return acc; });

var tradesSets = api.TradeRepository.GetTradesStream()
		.Select(trades=>trades.Where(t=>t.TradeStatus==TradeStatus.Done))
		.Scan((acc, cur) => acc.Concat(cur));
		
var pnlByTrade = tradesSets.CombineLatest(prices, (t, p) => {
	var markedTrades = from trade in t 
		let netPosition = (trade.Direction == Direction.SELL ? -1 : 1) * trade.Notional
		let lastPrice = p.ContainsKey(trade.CurrencyPair) ? p[trade.CurrencyPair] : trade.SpotRate
		select new { 
		TradeId = trade.TradeId,
		CCYPair = trade.CurrencyPair,
		TradeDate = trade.TradeDate,
		TraderName = trade.TraderName,
		LastPrice = lastPrice,
		Cost = trade.SpotRate, 
		NetPosition =  netPosition,
		PNL = netPosition * (lastPrice - trade.SpotRate)/trade.SpotRate,
		CCY = trade.DealtCurrency};
	return markedTrades;
	});

var pnlByCurrency = pnlByTrade.Select(tradeSet=>tradeSet.GroupBy(trade=>trade.CCY)
	.Select(byCCY => new { CCY = byCCY.Key, PNL = byCCY.Sum(t=>t.PNL) }));

//prices.Select(i=>i.Select(item=>new {Symbol = item.Key, Price = item.Value})).DumpLatest("Prices");
pnlByCurrency.DumpLatest("Profit & Loss by Currency");
pnlByTrade.DumpLatest("Profit & Loss by Trade");
