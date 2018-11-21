class ConfigViewModel implements IConfigViewModel {
    constructor() {
        this.subscriptionMode = ko.observable(SubscriptionMode.OnDispatcher);
        this.executionMode = ko.observable(ExecutionMode.Async);
    }

     standard(): void {
         this.subscriptionMode(SubscriptionMode.OnDispatcher);
     }

     dropFrame(): void {
         this.subscriptionMode(SubscriptionMode.ObserveLatestOnDispatcher);
     }

     conflate(): void {
         this.subscriptionMode(SubscriptionMode.Conflate);        
     }

     constantRate(): void {
         this.subscriptionMode(SubscriptionMode.ConstantRate);  
     }

     subscriptionMode: KnockoutObservable<SubscriptionMode>;

     async(): void {
         this.executionMode(ExecutionMode.Async);
     }

     sync(): void {
         this.executionMode(ExecutionMode.Sync);     
     }

     executionMode: KnockoutObservable<ExecutionMode>;
 }