class ExecutablePrice implements IExecutablePrice {
    private _executionRepository: IExecutionRepository;

    constructor(direction: Direction, rate: number, executionRepository: IExecutionRepository) {
        this._executionRepository = executionRepository;
        this.direction = direction;
        this.rate = rate;
    }

    execute(notional: number, dealtCurrency: string): Rx.Observable<IStale<ITrade>> {
        return this._executionRepository.executeRequest(this, notional, dealtCurrency);
    }

    direction: Direction;
    rate: number;
    parent: IPrice;
}

