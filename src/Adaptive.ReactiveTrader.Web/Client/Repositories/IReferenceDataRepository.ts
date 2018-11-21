interface IReferenceDataRepository {
    getCurrencyPairsStream(): Rx.Observable<ICurrencyPairUpdate[]>;
} 

