class Profiler implements IProfiler {
    private _userPerformanceApi: boolean;

    constructor() {
        this._userPerformanceApi = typeof window.performance != "undefined";
    }

    now(): number {
        if (this._userPerformanceApi) {
            return window.performance.now();
        } else {
            return new Date().getTime();
        }
    }
} 