class PricingServiceClient extends ServiceClientBase implements IPricingServiceClient {
     constructor(connectionProvider: IConnectionProvider) {
         super(connectionProvider);
     }

    public getSpotStream(currencyPair: string): Rx.Observable<PriceDto> {
        return super.getResilientStream(connection=> this.getSpotStreamForConnection(currencyPair, connection), 5000);
    }

    private getSpotStreamForConnection(currencyPair: string, connection: IConnection) : Rx.Observable<PriceDto> {
        return Rx.Observable.create<PriceDto>(observer=> {
            var pricesSubscription = connection
                .allPrices
                .subscribe(price=> {
                    if (price.s == currencyPair) {
                        observer.onNext(price);
                    }
                });
            
            console.log("Sending price subscription for currency pair " + currencyPair);

            var subscriptionRequest = new PriceSubscriptionRequestDto();
            subscriptionRequest.CurrencyPair = currencyPair;

            connection.pricingHubProxy.invoke("SubscribePriceStream", subscriptionRequest)
                .done(_ => console.log("Subscribed to " + currencyPair))
                .fail(ex => observer.onError(ex));

            var unsubsciptionDisposable =  Rx.Disposable.create(()=> {
                connection.pricingHubProxy.invoke("UnsubscribePriceStream", subscriptionRequest)
                    .done(_ => console.log("Unsubscribed from currency pair '" + currencyPair + "' stream."))
                    .fail(error => console.log("An error occured while sending unsubscription request for " + currencyPair + ":" + error));
            });

            return new Rx.CompositeDisposable([pricesSubscription, unsubsciptionDisposable]);
        })
        .publish()
        .refCount();
    }
}