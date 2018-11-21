interface IPriceLatency {
    uiProcessingTimeMs: number;
    displayedOnUi(): void;
    receivedInGuiProcess(): void;
} 