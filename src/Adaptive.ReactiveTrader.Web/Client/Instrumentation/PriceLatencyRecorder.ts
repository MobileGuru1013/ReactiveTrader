class PriceLatencyRecorder implements IPriceLatencyRecorder {
    private _receivedCount: number;
    private _renderedCount: number;
    private _maxLatency: IPriceLatency;

    onRendered(price: IPrice) {
        var priceLatency = <IPriceLatency><any>price;
        if (priceLatency != null) {
            priceLatency.displayedOnUi();

            this._renderedCount++;
            if (this._maxLatency == null || priceLatency.uiProcessingTimeMs > this._maxLatency.uiProcessingTimeMs) {
                this._maxLatency = priceLatency;
            }
        }
    }

    onReceived(price: IPrice) {
        var priceLatency = <IPriceLatency><any>price;
        if (priceLatency != null) {
            priceLatency.receivedInGuiProcess();
            this._receivedCount++;
        }
    }

    calculateAndReset(): Statistics {
        if (!this._maxLatency) return null;

        var result = new Statistics(this._maxLatency.uiProcessingTimeMs, this._receivedCount, this._renderedCount);
        this._renderedCount = 0;
        this._receivedCount = 0;
        this._maxLatency = null;
        return result;
    }
} 