﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Adaptive.ReactiveTrader.Client.Configuration;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.UI.Shell;
using Adaptive.ReactiveTrader.Client.UI.Splash;
using Adaptive.ReactiveTrader.Shared.Logging;
using Autofac;
using log4net;
using ILog = log4net.ILog;

namespace Adaptive.ReactiveTrader.Client
{
    public partial class App
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeLogging();

            Start();
        }

        private async void Start()
        {
            var splash = new SplashWindow();
            splash.Show();

            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Build();

            Log.Info("Initializing reactive trader API...");
            var sw = Stopwatch.StartNew();
            var reactiveTraderApi = container.Resolve<IReactiveTrader>();

            var username = container.Resolve<IUserProvider>().Username;
            reactiveTraderApi.Initialize(username, container.Resolve<IConfigurationProvider>().Servers, container.Resolve<ILoggerFactory>());
            Log.InfoFormat("Reactive trader API initialized in {0}ms", sw.ElapsedMilliseconds);

            MainWindow = new MainWindow();
            var shellViewModel = container.Resolve<IShellViewModel>();
            MainWindow.Content = new ShellView(shellViewModel);

            await Task.Delay(TimeSpan.FromSeconds(1.5));
            splash.Close();
            MainWindow.Show();
            Log.InfoFormat("Main UI displayed {0}ms after process start.", DateTime.Now - Process.GetCurrentProcess().StartTime);
        }

        private void InitializeLogging()
        {
            Thread.CurrentThread.Name = "UI";

            log4net.Config.XmlConfigurator.Configure();

            Log.Info(@"  _____                 _   _           ");
            Log.Info(@" |  __ \               | | (_)          ");
            Log.Info(@" | |__) |___  __ _  ___| |_ ___   _____ ");
            Log.Info(@" |  _  // _ \/ _` |/ __| __| \ \ / / _ \");
            Log.Info(@" | | \ \  __/ (_| | (__| |_| |\ V /  __/");
            Log.Info(@" |_|  \_\___|\__,_|\___|\__|_| \_/ \___|");
        }
    }
}
