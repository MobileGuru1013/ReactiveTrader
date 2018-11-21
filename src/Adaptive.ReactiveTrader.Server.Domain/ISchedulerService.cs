using System.Reactive.Concurrency;

namespace Adaptive.ReactiveTrader.Server
{
    public interface ISchedulerService
    {
        IScheduler ThreadPool { get; } 
    }
}