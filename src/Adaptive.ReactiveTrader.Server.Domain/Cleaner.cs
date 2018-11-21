using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Server.Analytics;
using Adaptive.ReactiveTrader.Server.Execution;
using Adaptive.ReactiveTrader.Server.Pricing;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;
using Adaptive.ReactiveTrader.Shared.Extensions;
using ITradeRepository = Adaptive.ReactiveTrader.Server.Blotter.ITradeRepository;

namespace Adaptive.ReactiveTrader.Server
{
    public class Cleaner : IDisposable
    {
        private static readonly TimeSpan ResetTime = new TimeSpan(6, 0, 0);

        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        private readonly ITradeRepository _tradeRepository;
        private readonly IAnalyticsService _analyticsService;
        private readonly IExecutionService _executionService;
        private readonly IPriceLastValueCache _priceLastValueCache;
        private readonly ISchedulerService _scheduler;
        

        public Cleaner(ITradeRepository tradeRepository, IAnalyticsService analyticsService,
            IExecutionService executionService, IPriceLastValueCache priceLastValueCache,
            ISchedulerService scheduler)
        {
            _tradeRepository = tradeRepository;
            _analyticsService = analyticsService;
            _executionService = executionService;
            _priceLastValueCache = priceLastValueCache;
            _scheduler = scheduler;
        }

        public void Start()
        {
            _disposable.Add(StartTimer());
        }

        private IDisposable StartTimer()
        {
            var scheduleFor = _scheduler.ThreadPool.Now.ToUniversalTime()
                                .UtcDateTime
                                .Date
                                .AddDays(1)
                                .Add(ResetTime);

            var timer = Observable.Timer(scheduleFor, TimeSpan.FromDays(1), _scheduler.ThreadPool);

            return timer.Subscribe(Reset);
        }

        private void Reset(long _)
        {
            _analyticsService.Reset();
            _tradeRepository.Reset();
            
            // make 3 trades

            // eurusd
            // gbpusd
            // nzdusd
            foreach (var ccyPair in new [] { "EURUSD", "GBPUSD", "NZDUSD"})
            {
                try
                {
                    var price = _priceLastValueCache.GetLastValue(ccyPair);

                    var trade = new TradeRequestDto()
                    {
                        DealtCurrency = "EUR",
                        Direction = DirectionDto.Buy,
                        Notional = 500000,
                        SpotRate = price.Bid,
                        Symbol = ccyPair,
                        ValueDate = DateTime.Today.AddDays(2).ToWeekday()
                    };

                    _executionService.Execute(trade, "CPU-007").Wait(TimeSpan.FromSeconds(10));
                }
                catch
                {
                    // swallow exception
                }
            }
        }


        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}