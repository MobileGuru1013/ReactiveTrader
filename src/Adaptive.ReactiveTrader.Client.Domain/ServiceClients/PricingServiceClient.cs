using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Adaptive.ReactiveTrader.Client.Domain.Transport;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO.Pricing;
using Adaptive.ReactiveTrader.Shared.Logging;
using Microsoft.AspNet.SignalR.Client;

namespace Adaptive.ReactiveTrader.Client.Domain.ServiceClients
{
    internal class PricingServiceClient : ServiceClientBase, IPricingServiceClient
    {
        private readonly ILog _log;

        public PricingServiceClient(IConnectionProvider connectionProvider, ILoggerFactory loggerFactory) : base(connectionProvider)
        {
            _log = loggerFactory.Create(typeof (PricingServiceClient));
        }

        public IObservable<PriceDto> GetSpotStream(string currencyPair)
        {
            if (string.IsNullOrEmpty(currencyPair)) throw new ArgumentException("currencyPair");

            return GetResilientStream(connection => GetSpotStreamForConnection(currencyPair, connection.PricingHubProxy), TimeSpan.FromSeconds(5));
        }

        private IObservable<PriceDto> GetSpotStreamForConnection(string currencyPair, IHubProxy pricingHubProxy)
        {
            return Observable.Create<PriceDto>(observer =>
            {
                // subscribe to price feed first, otherwise there is a race condition 
                var priceSubscription = pricingHubProxy.On<PriceDto>(ServiceConstants.Client.OnNewPrice, p =>
                {
                    if (p.Symbol == currencyPair)
                    {
                        observer.OnNext(p);
                    } 
                });

                // send a subscription request
                _log.InfoFormat("Sending price subscription for currency pair {0}", currencyPair);
                var subscription = SendSubscription(currencyPair, pricingHubProxy)
                    .Subscribe(
                        _ => _log.InfoFormat("Subscribed to {0}", currencyPair),
                        observer.OnError);


                var unsubscriptionDisposable = Disposable.Create(() =>
                {
                    // send unsubscription when the observable gets disposed
                    _log.InfoFormat("Sending price unsubscription for currency pair {0}", currencyPair);
                    SendUnsubscription(currencyPair, pricingHubProxy)
                        .Subscribe(
                            _ => _log.InfoFormat("Unsubscribed from {0}", currencyPair),
                            ex =>
                                _log.WarnFormat("An error occurred while sending unsubscription request for {0}:{1}", currencyPair, ex.Message));
                });

                return new CompositeDisposable {priceSubscription, unsubscriptionDisposable, subscription};
            })
            .Publish()
            .RefCount();
        }

        private static IObservable<Unit> SendSubscription(string currencyPair, IHubProxy pricingHubProxy)
        {
            return Observable.FromAsync(
                () => pricingHubProxy.Invoke(ServiceConstants.Server.SubscribePriceStream,
                new PriceSubscriptionRequestDto {CurrencyPair = currencyPair}));
        }

        private static IObservable<Unit> SendUnsubscription(string currencyPair, IHubProxy pricingHubProxy)
        {
            return Observable.FromAsync(
                () => pricingHubProxy.Invoke(ServiceConstants.Server.UnsubscribePriceStream,
                new PriceSubscriptionRequestDto { CurrencyPair = currencyPair }));
        }
    }
}