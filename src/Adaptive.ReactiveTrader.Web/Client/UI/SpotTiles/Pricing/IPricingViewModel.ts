interface IPricingViewModel {
    symbol: string;
    bid: IOneWayPriceViewModel;
    ask: IOneWayPriceViewModel;
    notional: KnockoutObservable<number>;
    spread: KnockoutObservable<string>;
    dealtCurrency: string;
    movement: KnockoutObservable<PriceMovement>;
    spotDate: KnockoutObservable<string>;
    isSubscribing: KnockoutObservable<boolean>;
    currencyPair: ICurrencyPair;

    onTrade(trade: ITrade): void;
    dispose(): void;
    onExecutionError(message: string): void;
    executing(value: boolean): void;
    stale(value: boolean): void;
} 