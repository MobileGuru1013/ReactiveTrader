 class SpotTileViewModel implements ISpotTileViewModel {
     pricing: KnockoutObservable<IPricingViewModel> = ko.observable(null);
     affirmation: KnockoutObservable<IAffirmationViewModel> = ko.observable(null);
     error: KnockoutObservable<IErrorViewModel> = ko.observable(null);
     config: KnockoutObservable<IConfigViewModel> = ko.observable(null);
     state: KnockoutObservable<TileState> = ko.observable(TileState.Pricing);
     currencyPair: string;

     private _disposed: boolean = false;

     constructor(currencyPair: ICurrencyPair,
         subscriptionMode: SubscriptionMode,
         pricingFactory: IPricingViewModelFactory) {

         if (currencyPair != null) {
             this.pricing(pricingFactory.create(currencyPair, this));
             this.currencyPair = currencyPair.symbol;
         }
     }

     dispose(): void {
         if (!this._disposed) {
             this.pricing().dispose();
             this._disposed = true;
         }
     }

     onTrade(trade: ITrade) {
         this.affirmation(new AffirmationViewModel(trade, this));
         this.state(TileState.Affirmation);
         this.error = null;

         Rx.Observable.timer(3000, Rx.Scheduler.timeout)
             .subscribe(
                 _ => this.dismissAffirmation());
     }
      
     onExecutionError(message: string): void {
         this.error(new ErrorViewModel(this, message));
         this.state(TileState.Error);
     }

     dismissAffirmation(): void {
         this.state(TileState.Pricing);
         this.affirmation(null);
     }

     dismissError(): void {
         this.state(TileState.Pricing);
         this.error(null);
     }

     toConfig(): void {
         this.state(TileState.Config);
         this.config(new ConfigViewModel());
     }

     disconnect(): void {
         this.pricing().dispose();
     }
 }