using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using ActionbarSherlock.App;
using WF.Player.Core;

namespace WF.Player.Android
{
	[Activity (Label = "Screen", Theme = "@style/Theme.Wfplayer")]			
	public class ScreenActivity : SherlockFragmentActivity
	{
		private ScreenType activeScreen = ScreenType.MainScreen;
		private Cartridge cartridge;
		private Engine engine;
		private StreamWriter logFile;
		private LogLevel logLevel = LogLevel.LogCartridge;
		private LocListener locListener;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.ScreenActivity);
			var layoutMain = FindViewById<LinearLayout> (Resource.Id.layoutMain);

			// Create your application here
			Intent intent = this.Intent;

			string cartFilename = intent.GetStringExtra ("cartridge");
			bool cartRestore = intent.GetBooleanExtra ("restore", false);

			if (File.Exists (cartFilename)) {
				cartridge = new Cartridge(cartFilename);
			}

			if (cartridge == null)
				Finish ();

			// Create engine
			engine = createEngine (cartridge);

			// Show main screen
			var ft = this.SupportFragmentManager.BeginTransaction ();
			ft.SetBreadCrumbTitle (cartridge.Name);
			ft.SetTransition (FragmentTransaction.TransitNone);
			ft.Replace (Resource.Id.fragment, new ScreenMain (engine));
			ft.AddToBackStack (null);
			ft.Commit ();

			// Start cartridge
			if (cartRestore)
				Restore ();
			else
				Start ();
		}

		protected override void OnResume()
		{
			base.OnResume ();

			// Start LocListener
			if (locListener == null)
			{
				locListener = new LocListener (GetSystemService (Context.LocationService) as LocationManager);
				locListener.LocationChanged += OnRefreshLocation;
			}
		}

		protected override void OnPause()
		{
			base.OnPause ();
		}

		protected override void OnStart()
		{
			base.OnStart ();

			// Start LocListener
			if (locListener == null)
			{
				locListener = new LocListener (GetSystemService (Context.LocationService) as LocationManager);
				locListener.LocationChanged += OnRefreshLocation;
			}
		}

		protected override void OnStop()
		{
			base.OnStop ();

			// Stop LocListener
			if (locListener != null)
			{
				locListener.LocationChanged -= OnRefreshLocation;
				locListener = null;
			}
		}

		#region Properties

		public Cartridge Cartridge {
			get {
				return cartridge;
			}
		}

		#endregion

		#region Methods

		public void Start()
		{
			ShowScreen (ScreenType.MainScreen, -1);

			engine.Start ();

			// TODO: Show fragment of main
		}

		public void Restore()
		{
			ShowScreen (ScreenType.MainScreen, -1);

			engine.Restore(new FileStream(cartridge.SaveFilename,FileMode.Open));

			// TODO: Show fragment of main
		}

		public void ShowScreen(ScreenType screen, int objIndex)
		{
			var bar = this.SupportActionBar;
			var ft = this.SupportFragmentManager.BeginTransaction ();

			if (screen != activeScreen) {
				switch (screen) {
				case ScreenType.MainScreen:
					ft.SetBreadCrumbTitle (cartridge.Name);
					ft.SetTransition (FragmentTransaction.TransitNone);
					ft.Replace (Resource.Id.fragment, new ScreenMain (engine));
					ft.Commit ();
					break;
				case ScreenType.LocationScreen:
				case ScreenType.ItemScreen:
				case ScreenType.InventoryScreen:
				case ScreenType.TaskScreen:
					bar.SetDisplayHomeAsUpEnabled (true);
					ft.SetTransition (FragmentTransaction.TransitNone);
					ft.Replace (Resource.Id.fragment, new ScreenList (engine, screen));
					ft.AddToBackStack (null);
					ft.Commit ();
					break;
				case ScreenType.DetailScreen:
					bar.SetDisplayHomeAsUpEnabled (true);
					ft.SetTransition (FragmentTransaction.TransitNone);
					ft.Replace (Resource.Id.fragment, new ScreenDetail (engine, objIndex));
					ft.AddToBackStack (null);
					ft.Commit ();
					break;
				}
				activeScreen = screen;
			}
		}

		#endregion

		#region Events of Engine

		/// <summary>
		/// Get the input e.Input from the player.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnGetInput (Object sender, GetInputEventArgs e)
		{
		}

		/// <summary>
		/// Log the message e.MessageRaises the log message event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnLogMessage (Object sender, LogMessageEventArgs e)
		{
			if (e.Level <= logLevel)
			{
				if (logFile != null)
				{
					// Create log entry
					logFile.WriteLine (engine.CreateLogMessage (e.Message));
				}
			}
		}

		/// <summary>
		/// Execute special command specific for this player.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnNotifyOS (Object sender, NotifyOSEventArgs e)
		{
			switch (e.Command) {
			}
		}

		/// <summary>
		/// Play the media e.Media on device.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnPlayMedia (Object sender, PlayMediaEventArgs e)
		{
			if (e.Media.Data != null)
			{
				switch (e.Media.Type) {
					case MediaTypes.MP3:
					break;
					case MediaTypes.WAV:
					break;
					case MediaTypes.FDL:
					break;
				}
			}
		}

		/// <summary>
		/// Show message e.Text with media e.Media to player.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnShowMessage(Object sender, ShowMessageEventArgs e)
		{
			var bar = this.SupportActionBar;
			var ft = this.SupportFragmentManager.BeginTransaction ();

			bar.SetDisplayHomeAsUpEnabled (false);
			ft.SetTransition (FragmentTransaction.TransitNone);
			ft.Replace (Resource.Id.fragment, new ScreenDialog (engine, e.Text, e.Media, e.ButtonLabel1, e.ButtonLabel2, e.Callback));
			ft.Commit ();
		}

		/// <summary>
		/// Show screen e.Screen to player. If needed, there is an object index in e.IndexObject.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnShowScreen(Object sender, ShowScreenEventArgs e)
		{
			ShowScreen (e.Screen, e.IndexObject);
		}

		/// <summary>
		/// Show 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		public void OnShowStatusText (Object sender, ShowStatusTextEventArgs e)
		{
			Toast.MakeText (this, e.Text, ToastLength.Long);
		}

		public void OnSynchronize(Object sender, SynchronizeEventArgs e)
		{
			this.RunOnUiThread (() => e.Func(e.Source));
		}

		#endregion

		#region Events

		private void OnRefreshLocation(Object sender, LocationChangedEventArgs e)
		{
			if (engine != null)
				engine.RefreshLocation (locListener.Latitude, locListener.Longitude, locListener.Altitude, locListener.Accuracy);
		}

		#endregion

		#region Private functions

		private Engine createEngine (Cartridge cart)
		{
			Engine result;

			result = new Engine ();

			// Set all events for engine
			result.GetInputEvent += OnGetInput;
			result.LogMessageEvent += OnLogMessage;
			result.NotifyOSEvent += OnNotifyOS;
			result.PlayMediaEvent += OnPlayMedia;
			result.ShowMessageEvent += OnShowMessage;
			result.ShowScreenEvent += OnShowScreen;
			result.ShowStatusTextEvent += OnShowStatusText;
			result.SynchronizeEvent += OnSynchronize;

			// If there is a old logFile, close it
			if (logFile != null) {
				logFile.Flush ();
				logFile.Close ();
			}

			// Open logFile first time
			logFile = new StreamWriter(cart.LogFilename, true, Encoding.UTF8);
			logFile.AutoFlush = true;

			result.Init (new FileStream (cart.Filename,FileMode.Open), cart);

			return result;
		}

		private void closeEngine()
		{
			engine.GetInputEvent -= OnGetInput;
			engine.LogMessageEvent -= OnLogMessage;
			engine.NotifyOSEvent -= OnNotifyOS;
			engine.PlayMediaEvent -= OnPlayMedia;
			engine.ShowMessageEvent -= OnShowMessage;
			engine.ShowScreenEvent -= OnShowScreen;
			engine.ShowStatusTextEvent -= OnShowStatusText;
			engine.SynchronizeEvent -= OnSynchronize;

			engine = null;

			// If there is a old logFile, close it
			if (logFile != null) {
				logFile.Flush ();
				logFile.Close ();
			}
		}

		#endregion

	}
}

