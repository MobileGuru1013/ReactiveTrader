interface IConnectionProvider {
    getActiveConnection(): Rx.Observable<IConnection>;
    dispose(): void;
} 