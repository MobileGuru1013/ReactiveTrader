class AffirmationViewModel implements IAffirmationViewModel {
    private _trade: ITrade;
    private _parent: ISpotTileViewModel;
    private _baseCurrency: string;
    private _counterCurrency: string;
    private _otherCurrency: string;

    constructor(trade: ITrade, parent: ISpotTileViewModel) {
        this._trade = trade;
        this._parent = parent;
        this._baseCurrency = trade.currencyPair.substring(0, 3);
        this._counterCurrency = trade.currencyPair.substring(3, 6);
        this._otherCurrency = trade.dealtCurrency = this._baseCurrency ? this._counterCurrency : this._baseCurrency;
    }

    get currencyPair(): string {
        return this._baseCurrency + " / " + this._counterCurrency;
    }

    get direction(): string {
        return this._trade.direction == Direction.Buy ? "Bought" : "Sold";
    }

    get notional(): string {
        return NumberFormatter.format(this._trade.notional);
    }

    get spotRate(): string {
        return this._trade.spotRate.toString();
    }

    get tradeDate(): string {
        return this._trade.tradeDate.toString();
    }

    get tradeId(): string {
        return this._trade.tradeId.toString();
    }

    get traderName(): string {
        return this._trade.traderName;
    }

    get valueDate(): string {
        return DateUtils.formatDateDayMonth(this._trade.valueDate);
    }

    get dealtCurrency(): string {
        return this._trade.dealtCurrency;
    }

    get rejected(): string {
        return this._trade.tradeStatus == TradeStatus.Done ? "" : "REJECTED";
    }

    get otherCurrency(): string {
        return this._otherCurrency;
    }

    dismiss(): void {
        this._parent.dismissAffirmation();
    }
} 