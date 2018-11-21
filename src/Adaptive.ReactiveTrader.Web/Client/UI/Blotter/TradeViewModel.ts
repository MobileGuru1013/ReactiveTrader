 class TradeViewModel implements ITradeViewModel {
     spotRate: number;
     notional: string;
     direction: string;
     currencyPair: string;
     tradeId: string;
     tradeDate: string;
     tradeStatus: string;
     traderName: string;
     valueDate: string;
     dealtCurrency: string;

     constructor(trade: ITrade) {
         this.spotRate = trade.spotRate;
         this.notional = NumberFormatter.format(trade.notional) + " " + trade.dealtCurrency;
         this.direction = trade.direction == Direction.Buy ? "Buy" : "Sell";
         this.currencyPair = trade.currencyPair.substring(0, 3) + " / " + trade.currencyPair.substring(3, 6);
         this.tradeId = trade.tradeId.toFixed(0);
         this.tradeDate = DateUtils.formatDateDayMonthYearHour(trade.tradeDate);
         this.tradeStatus = trade.tradeStatus == TradeStatus.Done ? "Done" : "REJECTED";
         this.traderName = trade.traderName;
         this.valueDate = "SP. " + DateUtils.formatDateDayMonthYear(trade.tradeDate);
         this.dealtCurrency = trade.dealtCurrency;
     }
 }