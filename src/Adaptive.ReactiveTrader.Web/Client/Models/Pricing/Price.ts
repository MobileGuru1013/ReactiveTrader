class Price implements IPrice, IPriceLatency {
    private _profiler: IProfiler;

    constructor(
        bid: ExecutablePrice,
        ask: ExecutablePrice,
        valueDate: Date,
        currencyPair: ICurrencyPair,
        profiler: IProfiler) {

        this.bid = bid;
        this.ask = ask;
        this.valueDate = valueDate;
        this.currencyPair = currencyPair;
        this.isStale = false;
        this._profiler = profiler;

        bid.parent = this;
        ask.parent = this;

        this.spread = (ask.rate - bid.rate) * Math.pow(10, currencyPair.pipsPosition);
    }

    bid: IExecutablePrice;
    ask: IExecutablePrice;
    currencyPair: ICurrencyPair;
    valueDate: Date;
    spread: number;
    isStale: boolean; 

    get mid(): number {
        return (this.bid.rate + this.ask.rate) / 2;
    }

    // IPriceLatency implementation

    private _receivedTimestamp: number;
    private _renderTimestamp: number;

    get uiProcessingTimeMs() {
        return this._renderTimestamp - this._receivedTimestamp;
    }

    displayedOnUi(): void {
        this._renderTimestamp = this._profiler.now();
    }

    receivedInGuiProcess(): void {
        this._receivedTimestamp = this._profiler.now();
    }
} 



