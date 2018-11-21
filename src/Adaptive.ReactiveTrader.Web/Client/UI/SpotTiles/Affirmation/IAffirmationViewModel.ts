interface IAffirmationViewModel {
    currencyPair: string;
    direction: string;
    notional: string;
    spotRate: string;
    tradeDate: string;
    tradeId: string;
    traderName: string;
    valueDate: string;
    dealtCurrency: string;
    rejected: string;
    otherCurrency: string;
    dismiss(): void;
}
