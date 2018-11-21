class ReferenceDataServiceClient extends ServiceClientBase implements IReferenceDataServiceClient  {
     constructor(connectionProvider: IConnectionProvider) {
         super(connectionProvider);
     }

    getCurrencyPairUpdatesStream(): Rx.Observable<CurrencyPairUpdateDto[]> {
        return this.getResilientStream(connection => this.getCurrencyPairUpdatesForConnection(connection), 5000);
    }

    private getCurrencyPairUpdatesForConnection(connection: IConnection) : Rx.Observable<CurrencyPairUpdateDto[]> {
        return Rx.Observable.create<CurrencyPairUpdateDto[]>(observer => {
            var currencyPairUpdateSubscription = connection.currencyPairUpdates.subscribe(
                currencyPairUpdate=> observer.onNext([currencyPairUpdate]));

            console.log("Sending currency pair subscription...");

            connection.referenceDataHubProxy
                .invoke("GetCurrencyPairs")
                .done(currencyPairs => {
                    console.log("Subscribed to currency pairs and received " + currencyPairs.length + " currency pairs.");
                    observer.onNext(currencyPairs);
                })
                .fail(ex => observer.onError(ex));

            var unsubsciptionDisposable = Rx.Disposable.create(() => {
                console.log("Unsubscribed from currency pairs stream.");
            });

            return new Rx.CompositeDisposable(currencyPairUpdateSubscription, unsubsciptionDisposable);
        })
        .publish()
        .refCount();
    }
}

        
