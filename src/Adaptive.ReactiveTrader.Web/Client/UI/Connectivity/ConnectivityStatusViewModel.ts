class ConnectivityStatusViewModel implements IConnectivityStatusViewModel {
    private _priceLatencyRecorder: IPriceLatencyRecorder;
    private _connectionStatusSubscription: Rx.Disposable;
    private _timerSubscription: Rx.Disposable;

    status: KnockoutObservable<string>;
    uiUpdates: KnockoutObservable<number>;
    ticksReceived: KnockoutObservable<number>;
    uiLatency: KnockoutObservable<string>;
    disconnected: KnockoutObservable<boolean>;

    constructor(reactiveTrader: IReactiveTrader, priceLatencyRecorder: IPriceLatencyRecorder) {
        this._priceLatencyRecorder = priceLatencyRecorder;

        this.uiLatency = ko.observable("-");
        this.uiUpdates = ko.observable(0);
        this.ticksReceived = ko.observable(0);
        this.disconnected = ko.observable(false);
        this.status = ko.observable("Disconnected");

        this._connectionStatusSubscription = reactiveTrader.connectionStatusStream
            .subscribe(
                status=> this.onStatusChanged(status),
                ex=> console.error("An error occured within the connection status stream", ex));

        this._timerSubscription = Rx.Observable
            .timer(1000, Rx.Scheduler.timeout)
            .repeat()
            .subscribe(_=> this.onTimerTick());
    }

    private onTimerTick() {
        var stats = this._priceLatencyRecorder.calculateAndReset();
        if (stats == null) return;

        this.uiLatency(stats.uiLatencyMax.toFixed(2));
        this.uiUpdates(stats.renderedCount);
        this.ticksReceived(stats.receivedCount);
    }

    private onStatusChanged(connectionInfo: ConnectionInfo) {
        switch(connectionInfo.connectionStatus) {
            case ConnectionStatus.Uninitialized:
            case ConnectionStatus.Connecting:
                this.status("Connecting to " + connectionInfo.server + "...");
                this.disconnected(true);
                break;
            case ConnectionStatus.Reconnected:
            case ConnectionStatus.Connected:
                this.status("Connected to " + connectionInfo.server + " (" + this.formatConnectionType(connectionInfo.connectionType) + ")");
                this.disconnected(false);
                break;
            case ConnectionStatus.ConnectionSlow:
                this.status("Slow connection detected with " + connectionInfo.server + " (" + this.formatConnectionType(connectionInfo.connectionType) + ")");
                this.disconnected(false);
                break;
            case ConnectionStatus.Reconnecting:
                this.status("Reconnecting to " + connectionInfo.server + "...");
                this.disconnected(true);
                this.clearStatistics();
                break;
            case ConnectionStatus.Closed:
                this.status("Disconnected from " + connectionInfo.server);
                this.disconnected(true);
                this.clearStatistics();
                break;
        }
    }

    private clearStatistics(): void {
        this.uiLatency("-");
        this.uiUpdates(0);
        this.ticksReceived(0);
    }

    public disconnect(): void {
        this._connectionStatusSubscription.dispose();
        this._timerSubscription.dispose();

        this.status("Session expired, disconnected.");
        this.disconnected(true);
        this.clearStatistics();
    }

    private formatConnectionType(connectionType: ConnectionType): string {
        switch (connectionType) {
            case ConnectionType.WebScokets:
                return "ws";
            case ConnectionType.ServerSentEvents:
                return "sse";
            case ConnectionType.LongPolling:
                return "lp";
            case ConnectionType.ForeverFrame:
                return "ff";
            case ConnectionType.None:
                return "?";
        }
    }
} 