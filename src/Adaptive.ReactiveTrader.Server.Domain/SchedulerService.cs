using System.Reactive.Concurrency;

namespace Adaptive.ReactiveTrader.Server
{
    public class SchedulerService : ISchedulerService
    {
        public IScheduler ThreadPool { get { return ThreadPoolScheduler.Instance; } }
    }
}