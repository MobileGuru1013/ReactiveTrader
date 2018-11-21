using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.iOSTab.Logging;
using System.Reactive.Linq;
using System.Linq;
using Adaptive.ReactiveTrader.Client.Concurrency;
using System.Collections;
using System.Collections.Generic;
using MonoTouch.Dialog;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public partial class LogViewController : UIViewController
	{
		private readonly ILogSource _logSource;
		private readonly IConcurrencyService _concurrencyService;
		private readonly IList<string> _messages = new List<string> ();
		Section _logs;
		private bool _isInitialised;

		public LogViewController (IConcurrencyService concurrencyService, ILogSource logSource)
		{
			_logSource = logSource;
			_concurrencyService = concurrencyService;

			Title = "Logs";
			TabBarItem.Image = UIImage.FromBundle ("adaptive");

			_logSource.LogSource
				.ObserveOn(_concurrencyService.Dispatcher)
				.Subscribe ((string message) => {
					if (!_isInitialised)
						_messages.Add (message);
					else
						OnMessage(message);
			});
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

			var root = new RootElement ("Logs");
			_logs = new Section ("Logs");
			root.Add (_logs);

			var dvc = new DialogViewController (root);
			this.AddChildViewController (dvc);
			this.Add (dvc.TableView);

			foreach (var message in _messages) {
				_logs.Add (GetElement (message));
			}

			_messages.Clear ();

			_isInitialised = true;
		}

		private void OnMessage(string message) {
			_logs.Add (GetElement (message));
		}

		static StyledStringElement GetElement (string message)
		{
			return new StyledStringElement (message, string.Empty, UITableViewCellStyle.Default) {
				LineBreakMode = UILineBreakMode.WordWrap,
				Alignment = UITextAlignment.Left,
				Font = UIFont.FromName ("Courier", 10)
			};
		}


		// Workaround: Prevent UI from incorrectly extending under tab bar.

		public override UIRectEdge EdgesForExtendedLayout {
			get {
				return (base.EdgesForExtendedLayout ^ UIRectEdge.Bottom);
			}
		}

	}
}

