interface IReferenceDataServiceClient
{
    getCurrencyPairUpdatesStream() : Rx.Observable<CurrencyPairUpdateDto[]>;
}
