 interface IPriceLatencyRecorder {
     onRendered(price: IPrice);
     onReceived(price: IPrice);
     calculateAndReset(): Statistics;
 }

 