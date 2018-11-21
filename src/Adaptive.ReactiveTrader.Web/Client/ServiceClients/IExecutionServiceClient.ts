interface IExecutionServiceClient {
    executeRequest(tradeRequest: TradeRequestDto): Rx.Observable<TradeDto>;
} 