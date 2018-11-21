class ExecutionServiceClient extends ServiceClientBase implements IExecutionServiceClient {
     constructor(connectionProvider: IConnectionProvider) {
         super(connectionProvider);
     }

    public executeRequest(tradeRequest: TradeRequestDto): Rx.Observable<TradeDto> {
        return this.requestUponConnection(connection => this.executeForConnection(tradeRequest, connection.executionHubProxy), 500);
    }

    public executeForConnection(tradeRequest: TradeRequestDto, executionHub: HubProxy): Rx.Observable<TradeDto> {
        return Rx.Observable.create<TradeDto>(observer => {
            executionHub.invoke("Execute", tradeRequest)
                .done(trade => observer.onNext(trade))
                .fail(error => observer.onError(error));

            return Rx.Disposable.empty;
        });
    }
}