 interface IReactiveTrader {
     tradeRepository: ITradeRepository;
     referenceDataRepository: IReferenceDataRepository;
     priceLatencyRecorder: IPriceLatencyRecorder;
     connectionStatusStream: Rx.Observable<ConnectionInfo>;
     initialize(username: string, servers: string[]): void;
     dispose(): void;
 } 