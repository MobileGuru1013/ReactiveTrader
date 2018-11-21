 class ReactiveTrader implements IReactiveTrader {
     private _connectionProvider: IConnectionProvider;

     initialize(username: string, servers: string[]): void {
         this._connectionProvider = new ConnectionProvider(username, servers);

         this.priceLatencyRecorder = new PriceLatencyRecorder();
         var referenceDataServiceClient = new ReferenceDataServiceClient(this._connectionProvider);
         var executionServiceClient = new ExecutionServiceClient(this._connectionProvider);
         var blotterServiceClient = new BlotterServiceClient(this._connectionProvider);
         var pricingServiceClient = new PricingServiceClient(this._connectionProvider);

         var tradeFactory = new TradeFactory();
         var executionRepository = new ExecutionRepository(executionServiceClient, tradeFactory);
         var profiler = new Profiler();
         var priceFactory = new PriceFactory(executionRepository, this.priceLatencyRecorder, profiler);
         var priceRepository = new PriceRepository(pricingServiceClient, priceFactory);
         var currencyPairUpdateFactory = new CurrencyPairUpdateFactory(priceRepository);

         this.tradeRepository = new TradeRepository(blotterServiceClient, tradeFactory);
         this.referenceDataRepository = new ReferenceDataRepository(referenceDataServiceClient, currencyPairUpdateFactory);
     }

     tradeRepository: ITradeRepository;
     priceLatencyRecorder: IPriceLatencyRecorder;
     referenceDataRepository: IReferenceDataRepository;

     get connectionStatusStream(): Rx.Observable<ConnectionInfo> {
         return this._connectionProvider.getActiveConnection()
             .select(c=>c.status)
             .switchLatest()
             .publish()
             .refCount();
     }

     dispose(): void {
         this._connectionProvider.dispose();
     }
 }

