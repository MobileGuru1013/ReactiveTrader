 interface IBlotterViewModel {
     trades: KnockoutObservableArray<ITradeViewModel>;
     disconnect(): void;
 }