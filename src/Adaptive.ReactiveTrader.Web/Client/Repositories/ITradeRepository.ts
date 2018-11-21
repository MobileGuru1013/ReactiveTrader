interface ITradeRepository {
    getTradesStream() : Rx.Observable<ITrade[]>;
}