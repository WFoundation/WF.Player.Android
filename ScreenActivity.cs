using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
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
		private int activeObject;
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

			// Set correct icon
			if (cartridge.Icon != null) {
				Bitmap bm = BitmapFactory.DecodeByteArray (cartridge.Icon.Data, 0, cartridge.Icon.Data.Length);
				this.SupportActionBar.SetIcon(new BitmapDrawable(this.Resources, bm));
			}

			// Show main screen
			var ft = this.SupportFragmentManager.BeginTransaction ();
			ft.SetBreadCrumbTitle (cartridge.Name);
			ft.SetTransition (FragmentTransaction.TransitNone);
			ft.AddToBackStack (null);
			ft.Replace (Resource.Id.fragment, new ScreenMain (engine));
			ft.Commit ();

			layoutMain.Invalidate ();

			// Start cartridge
			if (cartRestore)
				Restore ();
			else
				Start ();
		}

		public override void OnBackPressed()
		{
			if (activeScreen == ScreenType.LocationScreen || activeScreen == ScreenType.ItemScreen 
				|| activeScreen == ScreenType.InventoryScreen 
				|| activeScreen == ScreenType.TaskScreen 
				|| activeScreen == ScreenType.DetailScreen)
				RemoveScreen (activeScreen);
			else if (activeScreen == ScreenType.MainScreen)
				;
			else
				base.OnBackPressed ();
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
//			ShowScreen (ScreenType.MainScreen, -1);

			engine.Start ();
		}

		public void Restore()
		{
//			ShowScreen (ScreenType.MainScreen, -1);

			engine.Restore(new FileStream(cartridge.SaveFilename,FileMode.Open));
		}

		public void RemoveScreen(ScreenType last)
		{
			switch (last) {
				case ScreenType.MainScreen:
					// ToDo: Main screen is the last screen to show, so stop the cartridge
					ShowScreen (ScreenType.MainScreen, -1);
					break;
				case ScreenType.LocationScreen:
				case ScreenType.ItemScreen:
				case ScreenType.InventoryScreen:
				case ScreenType.TaskScreen:
					ShowScreen (ScreenType.MainScreen, -1);
					break;
				case ScreenType.DetailScreen:
					// Show correct list for this zone/item/character/task
					if (engine.IsUIObject (engine.GetObject (activeObject))) {
						UIObject obj = (UIObject)engine.GetObject (activeObject);
						activeObject = -1;
						if (obj is Zone)
							ShowScreen (ScreenType.LocationScreen, -1);
						if (obj is Task)
							ShowScreen (ScreenType.TaskScreen, -1);
						if (obj is Item || obj is Character) {
							if (engine.IsInInventory (obj))
								ShowScreen (ScreenType.InventoryScreen, -1);
							else
								ShowScreen (ScreenType.ItemScreen, -1);						
						}
					}
					break;
				case ScreenType.DialogScreen:
					// Which screen to show
					if (activeScreen == ScreenType.DetailScreen && activeObject != -1 && !((UIObject)engine.GetObject (activeObject)).Visible)
						RemoveScreen (ScreenType.DetailScreen);
					else
						ShowScreen (activeScreen, activeObject);
					break;
			}
		}

		public void ShowScreen(ScreenType screen, int objIndex)
		{
			var bar = this.SupportActionBar;
			var ft = this.SupportFragmentManager.BeginTransaction ();

//			if (screen != activeScreen) {
				// If the old screen is unknown, than use the last active screen
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
//					ft.AddToBackStack (null);
					ft.Commit ();
					break;
				case ScreenType.DetailScreen:
					bar.SetDisplayHomeAsUpEnabled (true);
					ft.SetTransition (FragmentTransaction.TransitNone);
					ft.Replace (Resource.Id.fragment, new ScreenDetail (engine, objIndex));
//					ft.AddToBackStack (null);
					ft.Commit ();
					break;
				}
				activeScreen = screen;
				activeObject = objIndex;
//			}
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
			var bar = this.SupportActionBar;
			var ft = this.SupportFragmentManager.BeginTransaction ();

			bar.SetDisplayHomeAsUpEnabled (false);
			ft.SetTransition (FragmentTransaction.TransitNone);
			ft.Replace (Resource.Id.fragment, new ScreenDialog (engine, e.Input));
			ft.Commit ();
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

		#region Drawing Directions

		public Bitmap DrawCenter ()
		{
			int w = 48;
			int h = 48;

			Bitmap b = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
			Canvas c = new Canvas(b);
			Paint light = new Paint (PaintFlags.AntiAlias);
			Paint dark = new Paint (PaintFlags.AntiAlias);

			light.Color = new Color (128, 0, 0, 255);
			light.StrokeWidth = 0f;
			light.SetStyle (Paint.Style.Stroke);

			dark.Color = new Color (255, 0, 0, 255);
			dark.StrokeWidth = 0f;
			dark.SetStyle (Paint.Style.FillAndStroke);

			int cx = c.Width / 2 - 1;
			int cy = c.Height / 2 - 1;

			c.DrawCircle (cx, cy, w / 8, light);
			c.DrawCircle (cx, cy, w / 32 * 3, dark);
//			UIGraphics.BeginImageContext (new SizeF(64, 64));
//
//			using (CGContext cont = UIGraphics.GetCurrentContext()) {
//
//				using (CGPath path = new CGPath()) {
//
//					cont.SetLineWidth (1f);
//					cont.SetRGBStrokeColor (1f, 0, 0, 1);
//					cont.SetRGBFillColor (0.5f, 0, 0, 1);
//					path.AddElipseInRect(new RectangleF(24,24,16,16));
//					path.CloseSubpath ();
//
//					cont.AddPath (path);
//					cont.DrawPath (CGPathDrawingMode.FillStroke);
//
//				}
//
//				using (CGPath path = new CGPath()) {
//
//					cont.SetRGBStrokeColor (1f, 0, 0, 1);
//					cont.SetLineWidth(3f);
//					//					cont.SetRGBFillColor (.5f, 0, 0, 1);
//					path.AddElipseInRect(new RectangleF(16,16,32,32));
//					path.CloseSubpath ();
//
//					cont.AddPath (path);
//					cont.DrawPath (CGPathDrawingMode.Stroke);
//
//				}
//
//				return UIGraphics.GetImageFromCurrentImageContext ();
//
//			}
//
//			UIGraphics.EndImageContext ();
			return b;
		}

		public Bitmap DrawArrow (double direction)
		{
			int w = 48;
			int h = 48;
			int w2 = w / 2;
			int h2 = w / 2;

			double rad1 = direction / 180.0 * Math.PI;
			double rad2 = (direction + 180.0 + 30.0) / 180.0 * Math.PI;
			double rad3 = (direction + 180.0 - 30.0) / 180.0 * Math.PI; 
			double rad4 = (direction + 180.0) / 180.0 * Math.PI; 

			PointF p1 = new PointF((float) (w2 + w2 * Math.Sin (rad1)), (float) (h2 + h2 * Math.Cos (rad1)));
			PointF p2 = new PointF((float) (w2 + w2 * Math.Sin (rad2)), (float) (h2 + h2 * Math.Cos (rad2)));
			PointF p3 = new PointF((float) (w2 + w2 * Math.Sin (rad3)), (float) (h2 + h2 * Math.Cos (rad3)));
			PointF p4 = new PointF((float) (w2 + w / 3 * Math.Sin (rad4)), (float) (h2 + h / 3 * Math.Cos (rad4)));

			Bitmap b = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
			Canvas c = new Canvas(b);
			Paint light = new Paint (PaintFlags.AntiAlias);
			Paint dark = new Paint (PaintFlags.AntiAlias);
			Paint black = new Paint (PaintFlags.AntiAlias);

			light.Color = new Color (128, 0, 0, 255);
			light.StrokeWidth = 0f;
			light.SetStyle (Paint.Style.FillAndStroke);

			dark.Color = new Color (255, 0, 0, 255);
			dark.StrokeWidth = 0f;
			dark.SetStyle (Paint.Style.FillAndStroke);

			black.Color = new Color (0, 0, 0, 255);
			black.StrokeWidth = 0f;
			black.SetStyle (Paint.Style.Stroke);

			int cx = c.Width / 2 - 1;
			int cy = c.Height / 2 - 1;

			global::Android.Graphics.Path path1 = new global::Android.Graphics.Path ();
			global::Android.Graphics.Path path2 = new global::Android.Graphics.Path ();
			global::Android.Graphics.Path path3 = new global::Android.Graphics.Path ();

			path1.MoveTo (p1.X,p1.Y);
			path1.LineTo (p2.X,p2.Y);
			path1.LineTo (p4.X,p4.Y);
			path1.LineTo (p1.X,p1.Y);

			path2.MoveTo (p1.X,p1.Y);
			path2.LineTo (p3.X,p3.Y);
			path2.LineTo (p4.X,p4.Y);
			path2.LineTo (p1.X,p1.Y);

			path3.MoveTo (p1.X,p1.Y);
			path3.LineTo (p2.X,p2.Y);
			path3.LineTo (p4.X,p4.Y);
			path3.LineTo (p1.X,p1.Y);
			path3.LineTo (p3.X,p3.Y);
			path3.LineTo (p4.X,p4.Y);

			c.DrawPath (path1, light);
			c.DrawPath (path2, dark);
			c.DrawPath (path3, black);

//			UIGraphics.BeginImageContext (new SizeF(64, 64));
//
//			using (CGContext cont = UIGraphics.GetCurrentContext()) {
//
//				using (CGPath path = new CGPath()) {
//
//					cont.SetLineWidth (1f);
//					cont.SetRGBStrokeColor (0f, 0, 0, 1);
//					cont.SetRGBFillColor (1f, 0, 0, 1);
//					path.AddLines (new PointF[] { p1, p2, p4 });
//					path.CloseSubpath ();
//
//					cont.AddPath (path);
//					cont.DrawPath (CGPathDrawingMode.FillStroke);
//
//				}
//
//				using (CGPath path = new CGPath()) {
//
//					cont.SetRGBStrokeColor (0f, 0, 0, 1);
//					cont.SetRGBFillColor (.5f, 0, 0, 1);
//					path.AddLines (new PointF[] { p1, p3, p4 });
//					path.CloseSubpath ();
//
//					cont.AddPath (path);
//					cont.DrawPath (CGPathDrawingMode.FillStroke);
//
//				}
//
//				return UIGraphics.GetImageFromCurrentImageContext ();
//
//			}
//
//			UIGraphics.EndImageContext ();
			return b;
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

