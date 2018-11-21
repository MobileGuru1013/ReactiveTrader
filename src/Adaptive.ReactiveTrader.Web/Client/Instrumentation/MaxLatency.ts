class Statistics {
    uiLatencyMax: number;
    receivedCount: number;
    renderedCount: number;

    constructor(uiLatency: number, receivedCount: number, renderedCount: number) {
        this.receivedCount = receivedCount;
        this.uiLatencyMax = uiLatency;
        this.renderedCount = renderedCount;
    }
 }
 