interface IConnectivityStatusViewModel {
    status: KnockoutObservable<string>;
    uiUpdates: KnockoutObservable<number>;
    ticksReceived: KnockoutObservable<number>;
    uiLatency: KnockoutObservable<string>;
    disconnected: KnockoutObservable<boolean>;
    disconnect(): void;
} 