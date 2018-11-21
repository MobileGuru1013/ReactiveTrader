using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Adaptive.ReactiveTrader.Client.Configuration;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.UI.Shell;
using Adaptive.ReactiveTrader.Shared.Logging;
using Autofac;

namespace Adaptive.ReactiveTrader.Client
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            var shellView = Window.Current.Content as ShellView;
            if (shellView == null)
            {
                var bootstrapper = new Bootstrapper();
                var container = bootstrapper.Build();

                var reactiveTraderApi = container.Resolve<IReactiveTrader>();

                var username = container.Resolve<IUserProvider>().Username;
                reactiveTraderApi.Initialize(username, container.Resolve<IConfigurationProvider>().Servers, container.Resolve<ILoggerFactory>());

                shellView = new ShellView {DataContext = container.Resolve<IShellViewModel>()};

                Window.Current.Content = shellView;
            }

            Window.Current.Activate();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
