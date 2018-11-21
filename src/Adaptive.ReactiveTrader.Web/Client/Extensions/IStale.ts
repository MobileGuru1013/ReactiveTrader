interface IStale<T> {
    isStale: boolean;
    update: T;
}