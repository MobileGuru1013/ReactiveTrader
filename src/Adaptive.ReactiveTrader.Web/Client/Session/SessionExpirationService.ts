class SessionExpirationService implements ISessionExpirationService {
    private _sessionDurationSeconds: number;

    constructor(sessionDurationSeconds: number) {
        this._sessionDurationSeconds = sessionDurationSeconds;
    }

    public getSessionExpiredStream(): Rx.Observable<{}> {
        return Rx.Observable.timer(this._sessionDurationSeconds * 1000, Rx.Scheduler.timeout)
            .publish()
            .refCount();
    }
} 