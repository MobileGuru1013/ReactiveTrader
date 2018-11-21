interface IConfigViewModel {
    standard(): void;
    dropFrame(): void;
    conflate(): void;
    constantRate(): void;
    async(): void;
    sync(): void;
    subscriptionMode: KnockoutObservable<SubscriptionMode>;
    executionMode: KnockoutObservable<ExecutionMode>;
}