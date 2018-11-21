interface IOneWayPriceViewModel {
    onExecute(): void;
    onPrice(executablePrice: IExecutablePrice): void;
    onStalePrice(): void;

    direction: string;
    bigFigures: KnockoutObservable<string>;
    pips: KnockoutObservable<string>;
    tenthOfPips: KnockoutObservable<string>;
} 