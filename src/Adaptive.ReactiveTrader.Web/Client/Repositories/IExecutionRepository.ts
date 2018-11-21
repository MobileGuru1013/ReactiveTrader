interface IExecutionRepository {
    executeRequest(executablePrice: IExecutablePrice, notional: number, dealtCurrency: string)
        : Rx.Observable<IStale<ITrade>>;
} 