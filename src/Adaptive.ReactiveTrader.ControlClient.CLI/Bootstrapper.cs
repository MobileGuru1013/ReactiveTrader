using Adaptive.ReactiveTrader.Client;
using Adaptive.ReactiveTrader.Client.Concurrency;
using Adaptive.ReactiveTrader.Client.Configuration;
using Adaptive.ReactiveTrader.ControlClient.CLI.Concurrency;
using Adaptive.ReactiveTrader.ControlClient.CLI.Configuration;
using Autofac;

namespace Adaptive.ReactiveTrader.ControlClient.CLI
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
        }
    }
}