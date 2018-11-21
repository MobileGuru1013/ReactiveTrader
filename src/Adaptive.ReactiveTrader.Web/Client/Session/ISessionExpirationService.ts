interface ISessionExpirationService {
    getSessionExpiredStream(): Rx.Observable<{}>;
} 