using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using log4net;

namespace Adaptive.ReactiveTrader.ControlClient.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (App));
        public static IContainer Container;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeLogging();

            Start();
        }

        private void Start()
        {
            var bootstrapper = new Bootstrapper(ConfigurationManager.AppSettings["UserName"]);
            var container = bootstrapper.Build();

            // expose via static variable so SignalR can pick it up in Startup class 
            Container = container;

            var mainWindow = container.Resolve<MainWindow>();
            var vm = container.Resolve<IMainViewModel>();
            mainWindow.DataContext = vm;
            vm.Start();
            mainWindow.Show();
        }

        private void InitializeLogging()
        {
            Thread.CurrentThread.Name = "UI";

            log4net.Config.XmlConfigurator.Configure();

            Log.Info(@"   _____                 _   _            _____              ");
            Log.Info(@"  |  __ \               | | (_)          / ____|                         ");
            Log.Info(@"  | |__) |___  __ _  ___| |_ ___   _____| (___   ___ _ ____   _____ _ __ ");
            Log.Info(@"  |  _  // _ \/ _` |/ __| __| \ \ / / _ \\___ \ / _ \ '__\ \ / / _ \ '__|");
            Log.Info(@"  | | \ \  __/ (_| | (__| |_| |\ V /  __/____) |  __/ |   \ V /  __/ |   ");
            Log.Info(@"  |_|  \_\___|\__,_|\___|\__|_| \_/ \___|_____/ \___|_|    \_/ \___|_|   ");
        }
    }
}
