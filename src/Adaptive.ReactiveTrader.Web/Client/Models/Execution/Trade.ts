 class Trade implements ITrade {
    currencyPair: string;
    direction: Direction;
    notional: number;
    spotRate: number;
    tradeStatus: TradeStatus;
    tradeDate: Date;
    tradeId: number;
    traderName:string;
    valueDate:Date;
    dealtCurrency: string;
    
    constructor(
        currencyPair: string,
        direction: Direction,
        notional: number,
        spotRate: number,
        tradeStatus: TradeStatus,
        tradeDate: string,
        tradeId: number,
        traderName:string,
        valueDate:string,
        dealtCurrency: string){
    
        this.currencyPair = currencyPair;
        this.direction = direction;
        this.notional = notional;
        this.spotRate = spotRate;
        this.tradeStatus = tradeStatus;
        this.tradeDate = new Date(tradeDate);
        this.tradeId = tradeId;
        this.traderName = traderName;
        this.valueDate = new Date(valueDate);
        this.dealtCurrency = dealtCurrency;
    }    
}