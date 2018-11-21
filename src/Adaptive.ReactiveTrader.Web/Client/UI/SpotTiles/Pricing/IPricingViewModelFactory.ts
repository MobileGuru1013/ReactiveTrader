interface IPricingViewModelFactory {
    create(currencyPair: ICurrencyPair, parent: ISpotTileViewModel): IPricingViewModel;
}  