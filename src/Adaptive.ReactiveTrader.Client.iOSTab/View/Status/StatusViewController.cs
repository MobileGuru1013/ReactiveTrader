
using System;
using System.Drawing;
using System.Collections.Generic; // For List<T>

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace Adaptive.ReactiveTrader.Client.iOSTab.View
{
	public partial class StatusViewController : UIViewController
	{
		private readonly IReactiveTrader _reactiveTrader;
		private readonly IConcurrencyService _concurrencyService;
		private readonly CompositeDisposable _disposables = new CompositeDisposable();

		private ConnectionInfo _lastConnectionInfo;
		private UIWebView _webView;

		public StatusViewController (IReactiveTrader reactiveTrader, IConcurrencyService concurrencyService) : base ("StatusViewController", null)
		{
			_reactiveTrader = reactiveTrader;
			_concurrencyService = concurrencyService;

			Title = "Status";
			TabBarItem.Image = UIImage.FromBundle ("tab_status");

			_disposables.Add (
				_reactiveTrader.ConnectionStatusStream
					.SubscribeOn (_concurrencyService.TaskPool)
					.ObserveOn (_concurrencyService.Dispatcher)
					.Subscribe (OnStatusChange));

			_disposables.Add (
				Observable.Interval (TimeSpan.FromSeconds(1), _concurrencyService.TaskPool)
					.ObserveOn (_concurrencyService.Dispatcher)
					.Subscribe(_ => OnTimer()));

		}

		protected override void Dispose (bool disposing)
		{	
			base.Dispose (disposing);

			if (disposing) {
				_disposables.Dispose();
			}
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.

			// The connection detail text area wraps and is resizable.
			// Note that this requires us to turn off Auto-layout for this view.

			this.ConnectionDetail.LineBreakMode = UILineBreakMode.CharacterWrap;

			this.TraderId.Text = UserModel.Instance.TraderId;

			if (_lastConnectionInfo != null) {
				OnStatusChange (_lastConnectionInfo);
				_lastConnectionInfo = null;
			}
		}

		private void OnStatusChange(ConnectionInfo connectionInfo)
		{
			if (this.IsViewLoaded) {
				this.ConnectionDetail.Text = connectionInfo.Server;
				this.ConnectionDetail.SizeToFit (); // Multi-line, with initially small height.
				this.ConnectionStatus.Text = connectionInfo.ConnectionStatus.ToString ();
			} else {
				_lastConnectionInfo = connectionInfo;
			}
		}

		private void OnTimer()
		{
			var statistics = _reactiveTrader.PriceLatencyRecorder.CalculateAndReset ();

			if (this.IsViewLoaded) {
				this.ServerUpdateRate.Text = string.Format ("{0} / sec", statistics.ReceivedCount);
				this.UIUpdateRate.Text = string.Format ("{0} / sec", statistics.RenderedCount);
				this.UILatency.Text = string.Format ("{0} ms", statistics.RenderedCount);
			}
		}

		partial void LinkTouchUpInside (MonoTouch.Foundation.NSObject sender)
		{
			UITabBarController parentTabBar = this.ParentViewController as UITabBarController;

			if (_webView == null) {

				_webView = new UIWebView (this.View.Bounds);

				UIViewController webViewController = new UIViewController();
				webViewController.View = _webView;
				webViewController.Title = "Adaptive";
				webViewController.TabBarItem.Image = UIImage.FromBundle ("adaptive");

				List<UIViewController> tabBarViewControllers = new List<UIViewController>(parentTabBar.ViewControllers);
				tabBarViewControllers.Add(webViewController);
				parentTabBar.SetViewControllers(tabBarViewControllers.ToArray(), false);
			}

			string url = "http://www.weareadaptive.com";
			_webView.LoadRequest(new NSUrlRequest(new NSUrl(url)));

			parentTabBar.SelectedIndex = parentTabBar.ViewControllers.Length - 1;
		}
	}
}

