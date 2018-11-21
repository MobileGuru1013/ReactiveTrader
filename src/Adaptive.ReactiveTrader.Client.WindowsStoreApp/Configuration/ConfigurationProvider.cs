namespace Adaptive.ReactiveTrader.Client.Configuration
{
    class ConfigurationProvider : IConfigurationProvider
    {
        public string[] Servers
        {
            //get { return new[] { "http://localhost:8080" }; }
            get { return new[] { "https://reactivetrader.azurewebsites.net/signalr" }; }
        }
    }
}
