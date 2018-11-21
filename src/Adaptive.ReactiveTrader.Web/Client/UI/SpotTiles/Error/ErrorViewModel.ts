class ErrorViewModel implements IErrorViewModel{
    private _parent: ISpotTileViewModel;

    constructor(parent: ISpotTileViewModel, message: string) {
        this._parent = parent;
        this.errorMessage = ko.observable(message);
    }

    errorMessage: KnockoutObservable<string>;
    dismiss(): void {
        this._parent.dismissError();
    }
} 