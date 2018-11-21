interface ISpotTilesViewModel {
    spotTiles: KnockoutObservableArray<ISpotTileViewModel>;
    disconnect(): void;
} 