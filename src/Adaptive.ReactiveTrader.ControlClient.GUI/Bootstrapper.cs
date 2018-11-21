using Adaptive.ReactiveTrader.Client;
using Adaptive.ReactiveTrader.Client.Concurrency;
using Adaptive.ReactiveTrader.Client.Configuration;
using Adaptive.ReactiveTrader.ControlClient.GUI.Concurrency;
using Adaptive.ReactiveTrader.ControlClient.GUI.Configuration;
using Autofac;

namespace Adaptive.ReactiveTrader.ControlClient.GUI
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly string _username;

        public Bootstrapper(string username)
        {
            _username = username;
        }

        protected override void RegisterTypes(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>();
            builder.RegisterInstance(new UserProvider(_username) as IUserProvider);
            builder.RegisterType<ConcurrencyService>().As<IConcurrencyService>();

            // UI
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<MainViewModel>().As<IMainViewModel>().SingleInstance();
            builder.RegisterType<CurrencyPairViewModel>().As<ICurrencyPairViewModel>();


        }

    }
}