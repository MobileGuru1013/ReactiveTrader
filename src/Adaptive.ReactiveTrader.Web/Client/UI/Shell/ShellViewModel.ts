class ShellViewModel {
    private _reactiveTrader: IReactiveTrader;

    spotTiles: ISpotTilesViewModel;
    blotter: IBlotterViewModel;
    connectivityStatus: IConnectivityStatusViewModel;
    sessionExpired: KnockoutObservable<boolean>;

    constructor(
        spotTiles: ISpotTilesViewModel,
        blotter: IBlotterViewModel,
        connectivityStatus: IConnectivityStatusViewModel,
        sessionExpirationService: ISessionExpirationService,
        reactiveTrader: IReactiveTrader) {
        this.spotTiles = spotTiles;
        this.blotter = blotter;
        this.connectivityStatus = connectivityStatus;
        this.sessionExpired = ko.observable(false);
        this._reactiveTrader = reactiveTrader;

        sessionExpirationService.getSessionExpiredStream()
            .subscribe(() => this.onSessionExpired());
    }

    private onSessionExpired(): void {
        console.info("Expiring session...");

        this.spotTiles.disconnect();
        this.blotter.disconnect();
        this.connectivityStatus.disconnect();
        this.sessionExpired(true);
        this._reactiveTrader.dispose();

        console.info("session expired");
    }

    reconnect(): void {
        location.reload();
    }
} 