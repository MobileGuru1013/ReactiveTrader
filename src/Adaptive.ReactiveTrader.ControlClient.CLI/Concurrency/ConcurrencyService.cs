using System;
using System.Reactive.Concurrency;
using Adaptive.ReactiveTrader.Client.Concurrency;

namespace Adaptive.ReactiveTrader.ControlClient.CLI.Concurrency
{
    public sealed class ConcurrencyService : IConcurrencyService
    {
        public IScheduler Dispatcher
        {
            get { throw new InvalidOperationException("Console application - no dispatcher thread."); }
        }

        public IScheduler TaskPool
        {
            get { return ThreadPoolScheduler.Instance; }
        }

    }
}