class TradeDto {
    TradeId:number;
    TraderName: string;
    CurrencyPair: string;
    Notional:number;
    DealtCurrency: string;
    Direction: DirectionDto;
    SpotRate: number;
    TradeDate: string;
    ValueDate: string;
    Status:TradeStatusDto;
}
