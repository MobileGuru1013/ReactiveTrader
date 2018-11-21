using System;
using System.Reactive.Concurrency;
using Adaptive.ReactiveTrader.Server.Analytics;
using Adaptive.ReactiveTrader.Server.Blotter;
using Adaptive.ReactiveTrader.Server.Execution;
using Adaptive.ReactiveTrader.Server.Pricing;
using NSubstitute;
using NUnit.Framework;

namespace Adaptive.ReactiveTrader.Server.Domain.Tests
{
    [TestFixture]
    public class CleanerTests
    {
        private Cleaner _target;
        private ITradeRepository _tradeRepo;
        private IAnalyticsService _analyticsService;
        private IExecutionService _executionService;
        private IPriceLastValueCache _lastValueCache;
        private HistoricalScheduler _scheduler;
        private ISchedulerService _schedulerService;

        [SetUp]
        public void SetUp()
        {
            _tradeRepo = Substitute.For<ITradeRepository>();
            _analyticsService = Substitute.For<IAnalyticsService>();
            _executionService = Substitute.For<IExecutionService>();
            _lastValueCache = Substitute.For<IPriceLastValueCache>();

            _scheduler = new HistoricalScheduler();
            _scheduler.AdvanceTo(DateTimeOffset.Now);

            _schedulerService = Substitute.For<ISchedulerService>();
            _schedulerService.ThreadPool.Returns(_scheduler);
        }

        [Test]
        public void Ctor_correctly_schedules()
        {
            _target = new Cleaner(_tradeRepo, _analyticsService, _executionService, _lastValueCache, _schedulerService);


        }


    }
}