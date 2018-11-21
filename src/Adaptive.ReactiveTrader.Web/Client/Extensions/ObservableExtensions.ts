declare module Rx {
    // extend the interface (this is how one can implement extension methods in TypeScript)
    interface ConnectableObservable<T> extends Observable<T> {
        lazyConnect(futureDisposable: SingleAssignmentDisposable): Observable<T>;
    }

    interface Observable<T> {
        detectStale(stalenessPeriodMs: number, scheduler: IScheduler): Observable<IStale<T>>;
        cacheFirstResult(): Observable<T>;
        conflate(minimumUpdatePeriodMs: number, scheduler: IScheduler): Observable<T>; 
    }
}

Rx.ConnectableObservable.prototype.lazyConnect = function <T>(futureDisposable: Rx.SingleAssignmentDisposable): Rx.Observable<T> {
    var connected = false;
    return Rx.Observable.create<T>(observer=> {
        var subscription = this.subscribe(observer);
        if (!connected) {
            connected = true;
            if (!futureDisposable.isDisposed) {
                futureDisposable.setDisposable(this.connect());
            }
        }
        return subscription;
    }).asObservable();
};

Rx.Observable.prototype.cacheFirstResult = function <T>(): Rx.Observable<T> {
    return this.take(1).publishLast().lazyConnect(new Rx.SingleAssignmentDisposable());
};

// TODO take unit inclusive

// TODO ObserveLatestOn

Rx.Observable.prototype.conflate = function <T>(minimumUpdatePeriodMs: number, scheduler: Rx.IScheduler): Rx.Observable<T> {
    return Rx.Observable.create<T>(observer => {
        // indicate when the last update was published
        var lastUpdateTime = 0;
        // indicate if an update is currently scheduled
        var updateScheduled = new Rx.SerialDisposable();
        updateScheduled.setDisposable(null);

        // indicate if completion has been requested (we can't complete immediatly if an update is in flight)
        var completionRequested = false;

        var subscription = this.observeOn(scheduler)
            .subscribe(
                x => {
                    var currentUpdateTime = scheduler.now();
                    var scheduleRequired = currentUpdateTime - lastUpdateTime < minimumUpdatePeriodMs;

                    if (scheduleRequired) {
                        updateScheduled.setDisposable(
                            scheduler.scheduleWithRelative(
                                lastUpdateTime + minimumUpdatePeriodMs,
                                () => {
                                    observer.onNext(x);

                                    lastUpdateTime = scheduler.now();
                                    updateScheduled.setDisposable(null);

                                    if (completionRequested) {
                                        observer.onCompleted();
                                    }
                        }));
                    }
                    else {
                        observer.onNext(x);
                        lastUpdateTime = scheduler.now();
                    }
                },
                ex => observer.onError(ex),
            () => {
                // if we have scheduled an update we need to complete once the update has been published
                if (updateScheduled.getDisposable() != null) {
                     completionRequested = true;
                }
                else {
                    observer.onCompleted();
                }
            });

        return new Rx.CompositeDisposable(subscription, updateScheduled);
    });
};


Rx.Observable.prototype.detectStale = function <T>(stalenessPeriodMs: number, scheduler: Rx.IScheduler): Rx.Observable<IStale<T>> {
    return Rx.Observable.create<IStale<T>>(observer => {
        var timerSubscription = new Rx.SerialDisposable();

        var scheduleStale = () => {
            timerSubscription.setDisposable(Rx.Observable
                .timer(stalenessPeriodMs, scheduler)
                .subscribe(
                _ => {
                    observer.onNext(new Stale<T>(true, null));
                }));
        };

        var sourceSubscription = this.subscribe(
            x => {
                // cancel any scheduled stale update
                timerSubscription.getDisposable().dispose();

                observer.onNext(new Stale<T>(false, x));

                scheduleStale();
            },
            ex => observer.onError(ex),
            () => observer.onCompleted());

        scheduleStale();

        return new Rx.CompositeDisposable(sourceSubscription, timerSubscription);
    });
};




