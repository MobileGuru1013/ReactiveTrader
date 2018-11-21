interface IPrice
{
    bid: IExecutablePrice;
    ask: IExecutablePrice;
    mid: number; 
    currencyPair: ICurrencyPair;
    valueDate: Date;
    spread: number;
    isStale: boolean; 
}

