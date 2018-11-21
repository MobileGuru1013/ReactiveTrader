interface ISpotTileViewModel extends  Rx.IDisposable
{
    pricing: KnockoutObservable<IPricingViewModel>;
    affirmation: KnockoutObservable<IAffirmationViewModel>;
    error: KnockoutObservable<IErrorViewModel>;
    config: KnockoutObservable<IConfigViewModel>;
    state: KnockoutObservable<TileState>;
    currencyPair: string;
    onTrade(trade: ITrade): void;
    onExecutionError(message: string);
    toConfig(): void;
    dismissAffirmation(): void;
    dismissError(): void;
    disconnect(): void;
} 