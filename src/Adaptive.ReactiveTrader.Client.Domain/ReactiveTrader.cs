﻿using System;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Client.Domain.Authorization;
using Adaptive.ReactiveTrader.Client.Domain.Concurrency;
using Adaptive.ReactiveTrader.Client.Domain.Instrumentation;
using Adaptive.ReactiveTrader.Client.Domain.Models.Execution;
using Adaptive.ReactiveTrader.Client.Domain.Models.Pricing;
using Adaptive.ReactiveTrader.Client.Domain.Models.ReferenceData;
using Adaptive.ReactiveTrader.Client.Domain.Repositories;
using Adaptive.ReactiveTrader.Client.Domain.ServiceClients;
using Adaptive.ReactiveTrader.Client.Domain.Transport;
using Adaptive.ReactiveTrader.Shared.Logging;

namespace Adaptive.ReactiveTrader.Client.Domain
{
    public class ReactiveTrader : IReactiveTrader, IDisposable
    {
        private ConnectionProvider _connectionProvider;
        private ILoggerFactory _loggerFactory;
        private ILog _log;
        private IControlRepository _controlRepository;

        public void Initialize(string username, string[] servers, ILoggerFactory loggerFactory = null, string authToken = null) 
        {
            _loggerFactory = loggerFactory ?? new DebugLoggerFactory();
            _log = _loggerFactory.Create(typeof(ReactiveTrader));
            _connectionProvider = new ConnectionProvider(username, servers, _loggerFactory);

            var referenceDataServiceClient = new ReferenceDataServiceClient(_connectionProvider, _loggerFactory);
            var executionServiceClient = new ExecutionServiceClient(_connectionProvider);
            var blotterServiceClient = new BlotterServiceClient(_connectionProvider, _loggerFactory);
            var pricingServiceClient = new PricingServiceClient(_connectionProvider, _loggerFactory);

            if (authToken != null)
            {
                var controlServiceClient = new ControlServiceClient(new AuthTokenProvider(authToken), _connectionProvider, _loggerFactory);
                _controlRepository = new ControlRepository(controlServiceClient);
            }

            PriceLatencyRecorder = new PriceLatencyRecorder();
            var concurrencyService = new ConcurrencyService();

            var tradeFactory = new TradeFactory();
            var executionRepository = new ExecutionRepository(executionServiceClient, tradeFactory, concurrencyService);
            var priceFactory = new PriceFactory(executionRepository, PriceLatencyRecorder);
            var priceRepository = new PriceRepository(pricingServiceClient, priceFactory, _loggerFactory);
            var currencyPairUpdateFactory = new CurrencyPairUpdateFactory(priceRepository);
            TradeRepository = new TradeRepository(blotterServiceClient, tradeFactory);
            ReferenceData = new ReferenceDataRepository(referenceDataServiceClient, currencyPairUpdateFactory);
        }

        public IReferenceDataRepository ReferenceData { get; private set; }
        public ITradeRepository TradeRepository { get; private set; }
        public IPriceLatencyRecorder PriceLatencyRecorder { get; private set; }

        public IControlRepository Control
        {
            get
            {
                if (_controlRepository == null)
                    throw new InvalidOperationException("You must supply an authentication token when initializing to use the control API.");
                return _controlRepository;
            }
        }

        public IObservable<ConnectionInfo> ConnectionStatusStream
        {
            get
            {
                return _connectionProvider.GetActiveConnection()
                    .Do(_ => _log.Info("New connection created by connection provider"))
                    .Select(c => c.StatusStream)
                    .Switch()
                    .Publish()
                    .RefCount();
            }
        }

        public void Dispose()
        {
            _connectionProvider.Dispose();
        }
    }
}
