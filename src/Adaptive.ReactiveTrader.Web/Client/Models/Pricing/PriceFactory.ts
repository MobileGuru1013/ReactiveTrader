class PriceFactory implements IPriceFactory {
    private _executionRepository: IExecutionRepository;
    private _priceLatencyRecored: IPriceLatencyRecorder;
    private _profiler: IProfiler;

    constructor(executionRepository: IExecutionRepository, priceLatencyRecored: IPriceLatencyRecorder, profiler: IProfiler) {
        this._executionRepository = executionRepository;
        this._priceLatencyRecored = priceLatencyRecored;
        this._profiler = profiler;
    }

    create(priceDto: PriceDto, currencyPair: ICurrencyPair) {
        var bid = new ExecutablePrice(Direction.Sell, priceDto.b, this._executionRepository);
        var ask = new ExecutablePrice(Direction.Buy, priceDto.a, this._executionRepository);
        var valueDate = new Date(priceDto.d);
        var price = new Price(bid, ask, valueDate, currencyPair, this._profiler);

        this._priceLatencyRecored.onReceived(price);

        return price;
    }
} 

