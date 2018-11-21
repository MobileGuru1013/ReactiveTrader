 class BlotterServiceClient extends ServiceClientBase implements IBlotterServiceClient {
     constructor(connectionProvider: IConnectionProvider) {
         super(connectionProvider);
     }

     getTradesStream(): Rx.Observable<TradeDto[]> {
         return super.getResilientStream(connection=> this.getTradesStreamFromConnection(connection), 5000);
     }

     private getTradesStreamFromConnection(connection: IConnection): Rx.Observable<TradeDto[]> {
         return Rx.Observable.create<TradeDto[]>(observer=> {
             var tradesSubscription = connection.allTrades.subscribe(observer);

             console.log("Sending blotter subscription...");
             connection.blotterHubProxy.invoke("SubscribeTrades")
                 .done(_=> console.log("Subscribed to blotter"))
                 .fail(ex=> observer.onError(ex));

             var unsubscriptionDisposable = Rx.Disposable.create(()=> {
                 console.log("Sending blotter unsubscription...");

                 connection.blotterHubProxy.invoke("UnsubscribeTrades")
                     .done(_=> console.log("Unsubscribed from blotter stream."))
                     .fail(ex=> console.error("An error occured while unsubscribing from blotter:" + ex));
             });

             return new Rx.CompositeDisposable([tradesSubscription, unsubscriptionDisposable]);
         })
         .publish()
         .refCount();
     }
 }