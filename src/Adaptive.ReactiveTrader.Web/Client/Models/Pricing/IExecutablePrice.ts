interface IExecutablePrice
{
    execute(notional: number, dealtCurrency: string): Rx.Observable<IStale<ITrade>>;
    direction: Direction;
    parent: IPrice;
    rate: number;
} 

