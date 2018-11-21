using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Client.Configuration;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Domain.Transport;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO.Control;
using Autofac;
using log4net;
using ILoggerFactory = Adaptive.ReactiveTrader.Shared.Logging.ILoggerFactory;

namespace Adaptive.ReactiveTrader.ControlClient.CLI
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Program));

        static void Main(string[] args)
        {
            InitializeLogging();

            var reactiveTraderApi = InitializeApi();

            Log.Info("API Connecting...");
            reactiveTraderApi.ConnectionStatusStream.Subscribe(Log.Info);
            reactiveTraderApi.ConnectionStatusStream.Where(ci => ci.ConnectionStatus == ConnectionStatus.Connected).Take(1).Wait();
            Log.Info("API Connected.");

            RunLoop(reactiveTraderApi).Wait();
        }

        private static IReactiveTrader InitializeApi()
        {
            string username;
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Username"]))
            {
                Console.WriteLine("Please enter your name (logging purposes only):");
                username = Console.ReadLine();
            }
            else
            {
                username = ConfigurationManager.AppSettings["Username"];
            }

            string authTokenKey;
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[AuthTokenProvider.AuthTokenKey]))
            {
                Console.WriteLine("Please enter your authorization token:");
                authTokenKey = Console.ReadLine();
            }
            else
            {
                authTokenKey = ConfigurationManager.AppSettings[AuthTokenProvider.AuthTokenKey];
            }

            var bootstrapper = new Bootstrapper(username);
            var container = bootstrapper.Build();

            Log.Info("Initializing reactive trader API...");
            var sw = Stopwatch.StartNew();
            var reactiveTraderApi = container.Resolve<IReactiveTrader>();

            reactiveTraderApi.Initialize(username, container.Resolve<IConfigurationProvider>().Servers, container.Resolve<ILoggerFactory>(), authTokenKey);
            Log.InfoFormat("Reactive trader API initialized in {0}ms", sw.ElapsedMilliseconds);
            
            return reactiveTraderApi;
        }

        private static async Task RunLoop(IReactiveTrader reactiveTraderApi)
        {
            Log.Info("Enter currency pair symbol and new state. Either E or D for enabled or disabled, or A or S for active or stale");
            while (true)
            {
                var states = await reactiveTraderApi.Control.GetCurrencyPairStates();
                var throughput = await reactiveTraderApi.Control.GetPriceFeedThroughput();
                Print(states, throughput);
                await SendCommand(states, reactiveTraderApi);
            }
        }

        private static void Print(IEnumerable<CurrencyPairStateDto> states, double throughput)
        {
            Log.Info("       | (E)nabled/ | (A)ctive/ |");
            Log.Info("Symbol | (D)isabled | (S)tale   |");
            Log.Info("---------------------------------");
            foreach (var state in states.OrderBy(st => st.Symbol))
            {
                Log.InfoFormat("{0, -6} | {1, 10} | {2, 9} |", state.Symbol, 
                    state.Enabled
                    ? "E"
                    : "D",
                    state.Stale
                    ? "S"
                    : "A");
            }
            Log.Info("---------------------------------");
            Log.InfoFormat("Target throughput: {0:G} ticks per second.", throughput);
        }

        private async static Task SendCommand(IEnumerable<CurrencyPairStateDto> states, IReactiveTrader reactiveTrader)
        {
            var commandLine = Console.ReadLine();
            var args = commandLine.Split(new [] { ' '}, StringSplitOptions.RemoveEmptyEntries);

            if (args.Length > 1)
            {
                await UpdateCurrencyPairState(states, reactiveTrader, args);
            }
            else
            {
                await UpdateThroughput(reactiveTrader, args[0]);
            }
            

        }

        private static async Task UpdateCurrencyPairState(IEnumerable<CurrencyPairStateDto> states, IReactiveTrader reactiveTrader, string[] args)
        {
            var ccyPair =
                states.FirstOrDefault(state => string.Equals(args[0], state.Symbol, StringComparison.InvariantCultureIgnoreCase));

            if (ccyPair == null)
            {
                Log.WarnFormat("Could not find symbol {0}", args[0]);
                return;
            }

            bool? setEnabled = null, setStale = null;

            foreach (var arg in args.Skip(1))
            {
                switch (arg.ToUpperInvariant())
                {
                    case "S":
                        setStale = true;
                        break;
                    case "A":
                        setStale = false;
                        break;
                    case "E":
                        setEnabled = true;
                        break;
                    case "D":
                        setEnabled = false;
                        break;
                }
            }

            Log.InfoFormat("From: {0}", ccyPair);
            if (setEnabled.HasValue)
            {
                ccyPair.Enabled = setEnabled.Value;
            }

            if (setStale.HasValue)
            {
                ccyPair.Stale = setStale.Value;
            }
            Log.InfoFormat("To:   {0}", ccyPair);
            Log.Info("Setting..");
            try
            {
                var result =
                    await reactiveTrader.Control.SetCurrencyPairState(ccyPair.Symbol, ccyPair.Enabled, ccyPair.Stale);
                Log.Info("Set!");
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to set.");
                Log.Warn(ex);
            }
        }

        private static async Task UpdateThroughput(IReactiveTrader reactiveTrader, string arg)
        {
            int throughput = 0;
            if (int.TryParse(arg, out throughput))
            {
                Log.InfoFormat("Setting through to {0:G}.", throughput);
                var result = await reactiveTrader.Control.SetPriceFeedThroughput(throughput);
                Log.Info("Set!");
            }
            else
            {
                Log.Warn("Could not parse throughput. Please specify an integer.");
            }
        }


        private static void InitializeLogging()
        {
            log4net.Config.XmlConfigurator.Configure();

            Log.Info(@"  ____  ____    __    ___  ____  ____  _  _  ____  ");
            Log.Info(@" (  _ \( ___)  /__\  / __)(_  _)(_  _)( \/ )( ___) ");
            Log.Info(@"  )   / )__)  /(__)\( (__   )(   _)(_  \  /  )__)  ");
            Log.Info(@" (_)\_)(____)(__)(__)\___) (__) (____)  \/  (____)");
            Log.Info(@"   ___  _____  _  _  ____  ____  _____  __  ");
            Log.Info(@"  / __)(  _  )( \( )(_  _)(  _ \(  _  )(  ) ");
            Log.Info(@" ( (__  )(_)(  )  (   )(   )   / )(_)(  )(__");
            Log.Info(@"  \___)(_____)(_)\_) (__) (_)\_)(_____)(____)"); 
        }
    }
}
