
class Connection implements IConnection {
    private _status: Rx.Subject<ConnectionInfo>;
    private _hubConnection: HubConnection;
    private _initialized: boolean;
    private _address: string;
    private _referenceDataHubProxy: HubProxy;
    private _pricingHubProxy: HubProxy;
    private _executionHubProxy: HubProxy;
    private _blotterHubProxy: HubProxy;
    private _allPrices: Rx.Subject<PriceDto>;
    private _currencyPairUpdates: Rx.Subject<CurrencyPairUpdateDto>;
    private _allTrades: Rx.Subject<TradeDto[]>;

    constructor(address: string, username: string) {
        this._status = new Rx.BehaviorSubject(new ConnectionInfo(ConnectionStatus.Uninitialized, address, ConnectionType.None));
        this._address = address;

        if (address != "") {
            this._hubConnection = $.hubConnection(address);
        } else {
            this._hubConnection = $.hubConnection();
            this._address = window.location.protocol + "//" + window.location.host;
        }

        this._hubConnection.qs = { "User": username };

        this._hubConnection
            .disconnected(() => this.changeStatus(ConnectionStatus.Closed, ConnectionType.None))
            .connectionSlow(() => this.changeStatus(ConnectionStatus.ConnectionSlow, this.getConnectionType()))
            .reconnected(() => this.changeStatus(ConnectionStatus.Reconnected, this.getConnectionType()))
            .reconnecting(() => this.changeStatus(ConnectionStatus.Reconnecting, ConnectionType.None))
            .error(error => console.log(error));

        this._referenceDataHubProxy = this._hubConnection.createHubProxy("ReferenceDataHub");
        this._blotterHubProxy = this._hubConnection.createHubProxy("BlotterHub");
        this._executionHubProxy = this._hubConnection.createHubProxy("ExecutionHub");
        this._pricingHubProxy = this._hubConnection.createHubProxy("PricingHub");

        this.installListeners();
    }

    public initialize(): Rx.Observable<{}> {

        return Rx.Observable.create<{}>(observer=> {
            this.changeStatus(ConnectionStatus.Connecting, ConnectionType.None);

            console.log("Connecting to " + this._address + "...");
            this._hubConnection.start()
                .done(()=> {
                    this.changeStatus(ConnectionStatus.Connected, this.getConnectionType());
                    observer.onNext(true);
                    console.log("Connected to " + this._address + ".");
                })
                .fail(()=> {
                    this.changeStatus(ConnectionStatus.Closed, ConnectionType.None);
                    var error = "An error occured when starting SignalR connection.";
                    console.log(error);
                    observer.onError(error);
                });

            return Rx.Disposable.create(()=> {
                console.log("Stopping connection...");
                this._hubConnection.stop();
                console.log("Connection stopped.");
            });
        })
        .publish()
        .refCount();
    }

    private getConnectionType(): ConnectionType {
        switch(this._hubConnection.transport.name)
        {
            case "webSockets":
                return ConnectionType.WebScokets;
            case "foreverFrame":
                return ConnectionType.ForeverFrame;
            case "serverSentEvents":
                return ConnectionType.ServerSentEvents;
            case "longPolling":
                return ConnectionType.LongPolling;
            default:
                return ConnectionType.None;
        }
    }

    private changeStatus(newStatus: ConnectionStatus, connectionType: ConnectionType): void {
        this._status.onNext(new ConnectionInfo(newStatus, this.address, connectionType));
    }

    public get status(): Rx.Observable<ConnectionInfo> {
        return this._status;
    }

    public get address(): string {
        return this._address;
    }

    public get referenceDataHubProxy(): HubProxy {
        return this._referenceDataHubProxy;
    }

    public get pricingHubProxy(): HubProxy {
        return this._pricingHubProxy;
    }

    public get executionHubProxy(): HubProxy {
        return this._executionHubProxy;
    }

    public get blotterHubProxy(): HubProxy {
        return this._blotterHubProxy;
    }

    public get allPrices(): Rx.Observable<PriceDto> {
        return this._allPrices;
    }

    public get currencyPairUpdates(): Rx.Observable<CurrencyPairUpdateDto> {
        return this._currencyPairUpdates;
    }

    public get allTrades(): Rx.Observable<TradeDto[]> {
        return this._allTrades;
    }

    private installListeners() {
        this._allPrices = new Rx.Subject<PriceDto>();
        this._currencyPairUpdates = new Rx.Subject<CurrencyPairUpdateDto>();
        this._allTrades = new Rx.Subject<TradeDto[]>();

        this._pricingHubProxy.on("OnNewPrice", price=> this._allPrices.onNext(price));
        this._referenceDataHubProxy.on("OnCurrencyPairUpdate", currencyPairs=> this._currencyPairUpdates.onNext(currencyPairs));
        this._blotterHubProxy.on("OnNewTrade", trades=> this._allTrades.onNext(trades));
    }
}
