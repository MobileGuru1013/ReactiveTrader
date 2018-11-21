class SpotTilesViewModel implements ISpotTilesViewModel, Rx.IDisposable {
    spotTiles: KnockoutObservableArray<ISpotTileViewModel>;
    private _referenceDataRepository: IReferenceDataRepository;
    private _pricingViewModelFactory: IPricingViewModelFactory;
    private _config: ISpotTileViewModel;
    private _subscriptionModeDisposable : KnockoutSubscription;
    private _executionModeDisposable: KnockoutSubscription;
    private _currencyPairsSubscription: Rx.Disposable;

    constructor(
        referenceDataRepository: IReferenceDataRepository,
        pricingViewModelFactory: IPricingViewModelFactory) {
        this._referenceDataRepository = referenceDataRepository;
        this._pricingViewModelFactory = pricingViewModelFactory;
        this.spotTiles = ko.observableArray([]);
        
        this._config = new SpotTileViewModel(null, SubscriptionMode.Conflate, this._pricingViewModelFactory);
        this._config.toConfig();

        // TODO this.spotTiles.push(this._config);

        this._subscriptionModeDisposable = this._config.config().subscriptionMode
            .subscribe(subscriptionMode => {
                // TODO
            });

        this._executionModeDisposable = this._config.config().executionMode
            .subscribe(executionMode => {
                // TODO
            });

        this.loadSpotTiles();
    }

    public dispose(): void {
        this._executionModeDisposable.dispose();
        this._subscriptionModeDisposable.dispose();
    }

    private loadSpotTiles(): void {
        this._currencyPairsSubscription =  this._referenceDataRepository.getCurrencyPairsStream()
            .subscribe(
                currencyPairs=> currencyPairs.forEach(cp=> this.handleCurrencyPairUpdate(cp)),
                ex=> console.error("Failed to get currencies", ex));
    }

    private handleCurrencyPairUpdate(update: ICurrencyPairUpdate) {

        var spotTileViewModel = ko.utils.arrayFirst(this.spotTiles(), stvm=> stvm.currencyPair == update.currencyPair.symbol);

        if (update.updateType == UpdateType.Add) {
            if (spotTileViewModel != null) {
                // we already have a tile for this ccy pair
                return;
            }

            var spotTile = new SpotTileViewModel(update.currencyPair, this._config.config().subscriptionMode(), this._pricingViewModelFactory);
            this.spotTiles.push(spotTile);
        } else {
            if (spotTileViewModel != null) {
                this.spotTiles.remove(spotTileViewModel);
                spotTileViewModel.dispose();
            }                
        }
    }

    public disconnect(): void {
        ko.utils.arrayForEach(this.spotTiles(), spotTile => spotTile.disconnect());
        this._currencyPairsSubscription.dispose();
    }
} 