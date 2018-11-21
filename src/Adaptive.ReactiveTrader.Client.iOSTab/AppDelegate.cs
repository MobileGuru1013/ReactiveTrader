using System;
using System.Collections.Generic;
using System.Linq; 
using System.Reactive.Linq;
using System.Security.Principal;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.iOSTab.Logging;
using Adaptive.ReactiveTrader.Client.iOSTab.View;
using Adaptive.ReactiveTrader.Client.Domain.Transport;
using System.Runtime.InteropServices;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		private IReactiveTrader _reactiveTrader;

		// class-level declarations

		UIWindow window;
		UITabBarController tabBarController;

		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Appearance
			UITableView.Appearance.BackgroundColor = Styles.RTDarkerBlue;
			UITableView.Appearance.SeparatorInset = UIEdgeInsets.Zero;

			UITabBar.Appearance.BarTintColor = Styles.RTDarkerBlue;


			// Black opaque status bar (which we request via Info.plist) not supported in iOS 7?
			// Xamarin 5.0.1 toolset out of date?

			UIApplication.SharedApplication.SetStatusBarStyle (UIStatusBarStyle.LightContent, false);
			UIApplication.SharedApplication.SetStatusBarHidden (false, true);

			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			var cs = new ConcurrencyService ();
			var logSource = new LogHub ();
			var logging = new LoggerFactory (logSource);

			#if DEBUG
			UIApplication.CheckForIllegalCrossThreadCalls = true;
			var logViewController = new LogViewController(cs, logSource);
			#endif

			_reactiveTrader = new Adaptive.ReactiveTrader.Client.Domain.ReactiveTrader ();

			_reactiveTrader.Initialize (UserModel.Instance.TraderId, new [] { "https://reactivetrader.azurewebsites.net/signalr" }, logging);
			//		_reactiveTrader.Initialize (UserModel.Instance.Id, new [] { "http://192.168.1.197:8080/signalr" }, logging);

			var tradesViewController = new TradesViewController (_reactiveTrader, cs);
			var pricesViewController = new PriceTilesViewController (_reactiveTrader, cs);
			var statusViewController = new StatusViewController (_reactiveTrader, cs);

			tabBarController = new UITabBarController ();

			tabBarController.ViewControllers = new UIViewController [] {
				pricesViewController,
				tradesViewController,
				statusViewController
				#if DEBUG
				, logViewController
				#endif
			};

			var startUpViewController = new StartUpView ();

			startUpViewController.DisplayMessages (true, "Connecting..");
			_reactiveTrader.ConnectionStatusStream
				.Where (ci => ci.ConnectionStatus == ConnectionStatus.Connected)
				.Timeout (TimeSpan.FromSeconds (15))
				.ObserveOn (cs.Dispatcher)
				.Subscribe (_ => startUpViewController.PresentViewController (tabBarController, false, null),
				ex => startUpViewController.DisplayMessages (false, "Disconnected", "Unable to connect. Please restart the app."));

			window.RootViewController = startUpViewController;

			// make the window visible
			window.MakeKeyAndVisible ();

			return true;
		}
	}
}

