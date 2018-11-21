interface IErrorViewModel {
    errorMessage: KnockoutObservable<string>;
    dismiss(): void;
}