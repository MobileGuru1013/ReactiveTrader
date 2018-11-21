class ExecutionRepository implements IExecutionRepository {
    private _executionServiceClient : IExecutionServiceClient;
    private _tradeFactory: ITradeFactory ;

    constructor(
        executionServiceClient: IExecutionServiceClient,
        tradeFactory: ITradeFactory) {
        this._tradeFactory = tradeFactory;
        this._executionServiceClient = executionServiceClient;
    }

    executeRequest(executablePrice: IExecutablePrice, notional: number, dealtCurrency: string): Rx.Observable<IStale<ITrade>> {
        var price = executablePrice.parent;

        var request = new TradeRequestDto();
        request.Direction = executablePrice.direction == Direction.Buy ? DirectionDto.Buy : DirectionDto.Sell;
        request.Notional = notional;
        request.SpotRate = executablePrice.rate;
        request.Symbol = price.currencyPair.symbol;
        request.ValueDate = price.valueDate.toISOString();
        request.DealtCurrency = dealtCurrency;

        return this._executionServiceClient.executeRequest(request)
            .select(tradeDto => this._tradeFactory.create(tradeDto))
            .detectStale(2000, Rx.Scheduler.timeout);
    }
} 