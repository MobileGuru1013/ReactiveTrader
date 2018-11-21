using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Client.Domain.Transport;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;
using Adaptive.ReactiveTrader.Shared.Logging;
using Microsoft.AspNet.SignalR.Client;

namespace Adaptive.ReactiveTrader.Client.Domain.ServiceClients
{
    internal class BlotterServiceClient : ServiceClientBase, IBlotterServiceClient
    {
        private readonly ILog _log;

        public BlotterServiceClient(IConnectionProvider connectionProvider, ILoggerFactory loggerFactory) : base(connectionProvider)
        {
            _log = loggerFactory.Create(typeof (BlotterServiceClient));
        }

        public IObservable<IEnumerable<TradeDto>> GetTradesStream()
        {
            return GetResilientStream(connection => GetTradesForConnection(connection.BlotterHubProxy), TimeSpan.FromSeconds(5));
        }

        private IObservable<IEnumerable<TradeDto>> GetTradesForConnection(IHubProxy blotterHubProxy)
        {
            return Observable.Create<IEnumerable<TradeDto>>(observer =>
            {
                // subscribe to trade feed first, otherwise there is a race condition 
                var spotTradeSubscription = blotterHubProxy.On<IEnumerable<TradeDto>>(ServiceConstants.Client.OnNewTrade, observer.OnNext);

                _log.Info("Sending blotter subscription...");
                var sendSubscriptionDisposable = SendSubscription(blotterHubProxy)
                    .Subscribe(
                        _ => _log.InfoFormat("Subscribed to blotter."),
                        observer.OnError);

                var unsubscriptionDisposable = Disposable.Create(() =>
                {
                    // send unsubscription when the observable gets disposed
                    _log.Info("Sending trades unsubscription...");
                    SendUnsubscription(blotterHubProxy)
                        .Subscribe(
                            _ => _log.InfoFormat("Unsubscribed from blotter."),
                            ex => _log.WarnFormat("An error occurred while unsubscribing from blotter: {0}", ex.Message));
                });
                return new CompositeDisposable { spotTradeSubscription, unsubscriptionDisposable, sendSubscriptionDisposable };
            })
                .Publish()
                .RefCount();
        }

        private static IObservable<Unit> SendSubscription(IHubProxy blotterHubProxy)
        {
            return Observable.FromAsync(() => blotterHubProxy.Invoke(ServiceConstants.Server.SubscribeTrades));
        }

        private static IObservable<Unit> SendUnsubscription(IHubProxy blotterHubProxy)
        {
            return Observable.FromAsync(() => blotterHubProxy.Invoke(ServiceConstants.Server.UnsubscribeTrades));
        } 
    }
}