class Stale<T> implements IStale<T> {
    isStale: boolean;
    update: T;

    constructor(stale: boolean, update: T) {
        this.isStale = stale;
        this.update = update;
    }
}