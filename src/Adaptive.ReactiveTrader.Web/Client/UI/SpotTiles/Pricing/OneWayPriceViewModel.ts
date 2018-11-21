class OneWayPriceViewModel implements IOneWayPriceViewModel {
    private _parent: IPricingViewModel;
    private _executablePrice: IExecutablePrice;

    constructor(parent: IPricingViewModel, direction: Direction) {
        this._parent = parent;
        this.direction = direction == Direction.Buy ? "BUY" : "SELL";

        this.bigFigures = ko.observable("");
        this.pips = ko.observable("");
        this.tenthOfPips = ko.observable("");
    }

    onExecute(): void {
        this._parent.executing(true);

        this._executablePrice.execute(this._parent.notional(), this._parent.dealtCurrency)
            .subscribe(
                trade => this.onExecuted(trade),
                ex => this.onExecutionError(ex));
    }

    onPrice(executablePrice: IExecutablePrice): void {
        this._executablePrice = executablePrice;

        var formattedPrice = PriceFormatter.getFormattedPrice(executablePrice.rate,
            executablePrice.parent.currencyPair.ratePrecision, executablePrice.parent.currencyPair.pipsPosition);

        this.bigFigures(formattedPrice.bigFigures);
        this.pips(formattedPrice.pips);
        this.tenthOfPips(formattedPrice.tenthOfPips);
        this._parent.stale(false);
    }

    onStalePrice(): void {
        this._executablePrice = null;
        this.bigFigures("");
        this.pips("");
        this.tenthOfPips("");
        this._parent.stale(true);
    }

    private onExecuted(trade: IStale<ITrade>) {
        if (trade.isStale) {
            this.onExecutionTimeout();
        }
        else {
            console.log("Trade executed.");
            this._parent.onTrade(trade.update);    
        }
        this._parent.executing(false);
    }

    private onExecutionTimeout() {
        console.error("Trade execution request timed out.");
        this._parent.onExecutionError("No response was received from the server, the execution status is unknown. Please contact your sales representative.");
    }

    private onExecutionError(ex: Error) {
        this._parent.onExecutionError(ex.message);
    }

    direction: string;
    bigFigures: KnockoutObservable<string>;
    pips: KnockoutObservable<string>;
    tenthOfPips: KnockoutObservable<string>;
} 