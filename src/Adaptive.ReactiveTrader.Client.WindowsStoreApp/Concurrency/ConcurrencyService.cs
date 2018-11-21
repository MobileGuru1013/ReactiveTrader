using System.Reactive.Concurrency;
using Windows.ApplicationModel.Core;

namespace Adaptive.ReactiveTrader.Client.Concurrency
{
    public sealed class ConcurrencyService : IConcurrencyService
    {
        private readonly CoreDispatcherScheduler _dispatcherScheduler;
        public ConcurrencyService()
        {
            _dispatcherScheduler = new CoreDispatcherScheduler(CoreApplication.MainView.CoreWindow.Dispatcher);
        }
        public IScheduler Dispatcher
        {
            get { return _dispatcherScheduler; }
        }

        public IScheduler TaskPool
        {
            get { return TaskPoolScheduler.Default; }
        }
    }
}
