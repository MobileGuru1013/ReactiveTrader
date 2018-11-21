interface IPriceRepository {
    getPriceStream(currencyPair: ICurrencyPair) : Rx.Observable<IPrice>;
} 