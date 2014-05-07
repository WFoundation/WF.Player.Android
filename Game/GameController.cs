///
/// WF.Player.Android - A Wherigo Player for Android, which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014  Dirk Weltz <mail@wfplayer.com>
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Lesser General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Lesser General Public License for more details.
/// 
/// You should have received a copy of the GNU Lesser General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
///

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;
using Android.Support.V4.App;
using Android.Support.V7.App;
//using SharpGpx;
using WF.Player.Core;
using WF.Player.Core.Engines;
using WF.Player.Location;
using WF.Player.Types;

namespace WF.Player.Game
{
	/// <summary>
	/// Screen activity for player.
	/// </summary>
	[Activity (Label = "Screen", ConfigurationChanges = ConfigChanges.KeyboardHidden|ConfigChanges.Orientation|ConfigChanges.ScreenSize, Theme = "@style/Theme")]
	public class GameController : ActionBarActivity
	{
		ScreenTypes activeScreen = ScreenTypes.Main;
		UIObject activeObject;
		Cartridge cartridge;
		Engine engine;
		StreamWriter logFile;
//		private GpxClass gpxFile;
		LogLevel logLevel = LogLevel.Cartridge;
		MediaPlayer mediaPlayer = new MediaPlayer();
		AudioManager audioManager;
		Vibrator vibrator;
		System.Timers.Timer removeTimer;
//		ScreenType removeType;
//		global::Android.Support.V4.App.Fragment removeVisibleScreen;
		bool cartRestore;
		Paint light = new Paint (PaintFlags.AntiAlias);
		Paint dark = new Paint (PaintFlags.AntiAlias);
		Paint black = new Paint (PaintFlags.AntiAlias);

		#region Constructor

		public GameController()
		{
			light.Color = new Color (128, 0, 0, 255);
			light.StrokeWidth = 0f;
			light.SetStyle (Paint.Style.FillAndStroke);

			dark.Color = new Color (255, 0, 0, 255);
			dark.StrokeWidth = 0f;
			dark.SetStyle (Paint.Style.FillAndStroke);

			black.Color = new Color (0, 0, 0, 255);
			black.StrokeWidth = 0f;
			black.SetStyle (Paint.Style.Stroke);
		}

		#endregion

		#region Android Event Handlers

		/// <summary>
		/// Raises the back pressed event, if the back button on phone is pressed.
		/// </summary>
		public override void OnBackPressed()
		{
			if (SupportFragmentManager.Fragments [0] is GameListScreen || SupportFragmentManager.Fragments [0] is GameDetailScreen 
				|| SupportFragmentManager.Fragments [0] is GameMapScreen || SupportFragmentManager.Fragments[0] is GameDialogScreen)
				RemoveScreen (ActiveScreenType());
			else if (SupportFragmentManager.Fragments [0] is GameMainScreen)
				Quit();
			else
				base.OnBackPressed ();
		}

		/// <summary>
		/// Raised, when the activity is created.
		/// </summary>
		/// <param name="bundle">Bundle with cartridge and restore flag.</param>
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Load content of activity
			SetContentView (Resource.Layout.ScreenActivity);

			// Get main layout to replace with fragments
			var layoutMain = FindViewById<LinearLayout> (Resource.Id.layoutMain);

			// Get data from intent
			Intent intent = this.Intent;

			string cartFilename = intent.GetStringExtra ("cartridge");
			cartRestore = intent.GetBooleanExtra ("restore", false);

			// Check, if cartridge files exists, and if yes, create a new cartridge object
			if (File.Exists (cartFilename)) {
				cartridge = new Cartridge(cartFilename);
			}

			// If cartridge object don't exist, than close activity
			if (cartridge == null)
				Finish ();

			audioManager = (AudioManager)GetSystemService(Context.AudioService);
			vibrator = (Vibrator)GetSystemService(Context.VibratorService);

			// Create CheckLocation
			GameCheckLocation checkLocation = new GameCheckLocation();

			// Show CheckLocation
			var ft = SupportFragmentManager.BeginTransaction ();
			ft.SetBreadCrumbTitle (cartridge.Name);
			ft.SetTransition (global::Android.Support.V4.App.FragmentTransaction.TransitNone);
			ft.Replace (Resource.Id.fragment, checkLocation);
			ft.Commit ();
		}

		public void InitController(bool start)
		{
			// Create engine
			CreateEngine (cartridge);

			// If cartridge contains an icon, than show this as home button
			if (cartridge.Icon != null) {
				using (Bitmap bm = BitmapFactory.DecodeByteArray (cartridge.Icon.Data, 0, cartridge.Icon.Data.Length)) {
					SupportActionBar.SetIcon(new BitmapDrawable(this.Resources, bm));
				}
			}

			// Show main screen
			var ft = this.SupportFragmentManager.BeginTransaction ();
			ft.SetBreadCrumbTitle (cartridge.Name);
			ft.SetTransition (global::Android.Support.V4.App.FragmentTransaction.TransitNone);
			ft.Replace (Resource.Id.fragment, new GameMainScreen (engine));
			ft.Commit ();

			// Start cartridge
			if (cartRestore)
				Restore ();
			else
				Start ();
		}

		/// <summary>
		/// Raised, when an option item is selected.
		/// </summary>
		/// <param name="item">Item, which is selected.</param>
		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			// TODO: Why is Resource.Id.Home not working?
			if (item.ItemId == 16908332) {
				OnBackPressed ();
				return false;
			}

			return base.OnOptionsItemSelected(item);
		}

		/// <summary>
		/// Raised, when the activity lost the focus.
		/// </summary>
		protected override void OnPause()
		{
			base.OnPause ();

			// Pause engine
			if (engine != null)
				engine.Pause();

			// Remove from GPS
			Main.GPS.RemoveLocationListener(OnRefreshLocation);
		}

		/// <summary>
		/// Raised, when the activity gets focus.
		/// </summary>
		protected override void OnResume()
		{
			base.OnResume ();

			SupportActionBar.SetDisplayHomeAsUpEnabled (true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);

			// Add to GPS
			Main.GPS.AddLocationListener(OnRefreshLocation);

			// Restart engine
			if (engine != null && engine.GameState == EngineGameState.Paused)
				engine.Resume();
		}

		/// <summary>
		/// Raised, when the activity is started.
		/// </summary>
		protected override void OnStart()
		{
			base.OnStart ();
		}

		/// <summary>
		/// Raised, when the activity stops.
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop ();

			// TODO: If engine is running, create an AutoSave file.
		}

		#endregion

		#region Properties

		/// <summary>
		/// Cartridge belonging to this activity.
		/// </summary>
		/// <value>The cartridge.</value>
		public Cartridge Cartridge 
		{
			get 
			{
				return cartridge;
			}
		}

		/// <summary>
		/// Engine belonging to this activity.
		/// </summary>
		/// <value>The engine.</value>
		public Engine Engine
		{
			get
			{
				return engine;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Quit the cartridge.
		/// </summary>
		public void Quit()
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetTitle(Resource.String.menu_screen_main_quit);
			builder.SetMessage(Resource.String.screen_save_before_quit);
			builder.SetCancelable(true);
			builder.SetPositiveButton(Resource.String.screen_save_before_quit_yes, delegate { engine.Save(new FileStream(cartridge.SaveFilename,FileMode.Create)); DestroyEngine (); Finish(); });
			// TODO: Works this also on devices with API < 14 (Pre 4.0)
			// var test = Build.VERSION.SdkInt;
			// builder.SetNeutralButton(Resource.String.screen_save_before_quit_cancel, delegate { });
			builder.SetNegativeButton(Resource.String.screen_save_before_quit_no, delegate { DestroyEngine (); Finish(); });
			builder.Show();
		}

		/// <summary>
		/// Restore the cartridge.
		/// </summary>
		public void Restore()
		{
			if (engine != null) {
				engine.Restore (new FileStream (cartridge.SaveFilename, FileMode.Open));
				engine.RefreshLocation(Main.GPS.Location.Latitude, Main.GPS.Location.Longitude, Main.GPS.Location.Altitude, Main.GPS.Location.Accuracy);
			}
		}

		/// <summary>
		/// Save the cartridge.
		/// </summary>
		public void Save()
		{
			engine.Save (new FileStream(cartridge.SaveFilename,FileMode.Create));
		}

		/// <summary>
		/// Start the cartridge.
		/// </summary>
		public void Start()
		{
			if (engine != null) {
				engine.Start ();
				engine.RefreshLocation(Main.GPS.Location.Latitude, Main.GPS.Location.Longitude, Main.GPS.Location.Altitude, Main.GPS.Location.Accuracy);
			}
		}

		/// <summary>
		/// Removes the active screen and show screen before.
		/// </summary>
		/// <param name="last">Last screen active.</param>
		public void RemoveScreen(ScreenTypes type)
		{
			// Save active screen and wait 100 ms. If the active screen is the same as the saved, than remove it
			global::Android.Support.V4.App.Fragment removeVisibleScreen = SupportFragmentManager.Fragments [0];
			ScreenTypes removeType = type;

			if (removeTimer != null) {
				removeTimer.Stop();
				removeTimer = null;
			}

			removeTimer = new System.Timers.Timer();
			removeTimer.Interval = 100;
			removeTimer.Elapsed += (sender, e) => RunOnUiThread( () => InternalRemoveScreen(sender, e, removeType, removeVisibleScreen) ); //InvokeRemoveScreen(sender, e, removeType, removeVisibleScreen);
			removeTimer.Start();
		}

		void InternalRemoveScreen(object sender, System.Timers.ElapsedEventArgs e, ScreenTypes removeType, global::Android.Support.V4.App.Fragment removeVisibleScreen) 
		{
			if (removeTimer != null) {
				// Stop timer, we don't need it anymore
				removeTimer.Stop();
				removeTimer = null;
			}

			// Is there still the same screen visible? If not, than leave
			if (SupportFragmentManager.Fragments [0] != removeVisibleScreen) {
				return;
			}

			bool remove = true;
			ScreenTypes activeType = ActiveScreenType();

			// Check if screen to remove is active screen, instead leave
			if (removeType != null) {
				if (SupportFragmentManager.Fragments [0] is GameListScreen)
					remove &= ((GameListScreen)SupportFragmentManager.Fragments [0]).Type == removeType;
				if (SupportFragmentManager.Fragments [0] is GameDetailScreen)
					remove &= removeType == ScreenTypes.Details;
				if (SupportFragmentManager.Fragments [0] is GameDialogScreen)
					remove &= removeType == ScreenTypes.Dialog;
				if (SupportFragmentManager.Fragments [0] is GameMapScreen)
					remove &= removeType == ScreenTypes.Map;
			}

			if (!remove)
				return;


			switch (activeType) {
				case ScreenTypes.Main:
					// Don't remove the main screen
					break;
				case ScreenTypes.Locations:
					ShowScreen (ScreenTypes.Main, null);
					break;
				case ScreenTypes.Items:
					ShowScreen (ScreenTypes.Main, null);
					break;
				case ScreenTypes.Inventory:
					ShowScreen (ScreenTypes.Main, null);
					break;
				case ScreenTypes.Tasks:
					ShowScreen (ScreenTypes.Main, null);
					break;
				case ScreenTypes.Details:
					// Show correct list for this zone/item/character/task
					if (((GameDetailScreen)SupportFragmentManager.Fragments [0]).ActiveObject != null) {
						// Select the correct list to show
						UIObject obj = ((GameDetailScreen)SupportFragmentManager.Fragments [0]).ActiveObject;
						activeObject = null;
						if (obj is Zone)
							ShowScreen (ScreenTypes.Locations, null);
						if (obj is Task)
							ShowScreen (ScreenTypes.Tasks, null);
						if (obj is Item || obj is Character) {
							if (engine.VisibleInventory.Contains ((Thing)obj))
								ShowScreen (ScreenTypes.Inventory, null);
							else
								ShowScreen (ScreenTypes.Items, null);
						}
					} else
						ShowScreen (ScreenTypes.Main, null);
					break;
				case ScreenTypes.Dialog:
				case ScreenTypes.Map:
					if (activeScreen == ScreenTypes.Details && activeObject != null && !activeObject.Visible) {
						// Object for detail screen is no longer visible, so show correct list
						// Select the correct list to show
						UIObject obj = activeObject;
						activeObject = null;
						if (obj is Zone)
							ShowScreen (ScreenTypes.Locations, null);
						if (obj is Task)
							ShowScreen (ScreenTypes.Tasks, null);
						if (obj is Item || obj is Character) {
							if (engine.VisibleInventory.Contains ((Thing)obj))
								ShowScreen (ScreenTypes.Inventory, null);
							else
								ShowScreen (ScreenTypes.Items, null);
						}
					} else {
						ShowScreen (activeScreen, activeObject);
					}
					break;
				}
		}

		/// <summary>
		/// Shows the screen.
		/// </summary>
		/// <param name="screen">Screen to show.</param>
		/// <param name="obj">Object to show if screen is ScreenType.Details.</param>
		public void ShowScreen(ScreenTypes screen, UIObject obj)
		{
			var bar = SupportActionBar;
			var ft = this.SupportFragmentManager.BeginTransaction ();
			var activeFragment = this.SupportFragmentManager.FindFragmentByTag("active");

			// If there is an active remove timer, stop it, because we bring the next screen on the device
			if (removeTimer != null) {
				removeTimer.Stop();
				removeTimer = null;
			}

			switch (screen) 
			{
				case ScreenTypes.Main:
					ft.SetBreadCrumbTitle (cartridge.Name);
					ft.SetTransition (global::Android.Support.V4.App.FragmentTransaction.TransitNone);
					ft.Replace (Resource.Id.fragment, new GameMainScreen (engine), "active");
					ft.Commit ();
				//					SupportFragmentManager.Fragments [0] = new ScreenMain (engine);
					break;
				case ScreenTypes.Locations:
				case ScreenTypes.Items:
				case ScreenTypes.Inventory:
				case ScreenTypes.Tasks:
					bar.SetDisplayHomeAsUpEnabled (true);
					ft.SetTransition (global::Android.Support.V4.App.FragmentTransaction.TransitNone);
					ft.Replace (Resource.Id.fragment, new GameListScreen (engine, screen), "active");
					ft.Commit ();
				//					SupportFragmentManager.Fragments [0] = new ScreenList (engine, screen);
					break;
				case ScreenTypes.Details:
					bar.SetDisplayHomeAsUpEnabled (true);
					ft.SetTransition (global::Android.Support.V4.App.FragmentTransaction.TransitNone);
					ft.Replace (Resource.Id.fragment, new GameDetailScreen (this, obj), "active");
					ft.Commit ();
				//					SupportFragmentManager.Fragments [0] = new ScreenDetail (this, obj);
					break;
				case ScreenTypes.Map:
					bar.SetDisplayHomeAsUpEnabled (true);
					ft.SetTransition (global::Android.Support.V4.App.FragmentTransaction.TransitNone);
					ft.Replace (Resource.Id.fragment, new GameMapScreen (this, obj), "active");
					ft.Commit ();
				//					SupportFragmentManager.Fragments [0] = new ScreenMap (this, obj);
					break;
			}

			// Save actuall values for later use
			if (screen != ScreenTypes.Dialog && screen != ScreenTypes.Map) {
				activeScreen = screen;
				activeObject = obj;
			}
		}

		public Bitmap ConvertMediaToBitmap(Media media, int maxWidth = -1)
		{
			Bitmap result = null;

			// First get dimensions of the image
			BitmapFactory.Options options = new BitmapFactory.Options();   

			// First decode with InJustDecodeBounds=true to check dimensions
			options.InJustDecodeBounds = true;
			BitmapFactory.DecodeByteArray(media.Data, 0, media.Data.Length, options);

			// Calculate inSampleSize

			// We need to adjust the height if the width of the bitmap is
			// smaller than the view width, otherwise the image will be boxed.
			if (options.OutWidth > 0) {

				var metrics = Resources.DisplayMetrics;
				int width = (int)(options.OutWidth * metrics.Density);
				int height = (int)(options.OutHeight * metrics.Density);
							
				maxWidth = maxWidth < 0 ? (int)(metrics.WidthPixels - 2 * Resources.GetDimension(Resource.Dimension.screen_frame)) : maxWidth;
				int maxHeight = (int)(0.5 * metrics.HeightPixels);
							
				if (width > maxWidth && (Main.Prefs.ImageResize == ImageResize.ResizeWidth || Main.Prefs.ImageResize == ImageResize.ShrinkWidth)) {
					double factor = (double)maxWidth / (double)width;
					width = maxWidth;
					height = (int)(height * factor);
					}

				if (width < maxWidth && Main.Prefs.ImageResize == ImageResize.ResizeWidth) {
					double factor = (double)maxWidth / (double)width;
					width = maxWidth;
					height = (int)(height * factor);
				}

				if (height != maxHeight && Main.Prefs.ImageResize == ImageResize.ResizeHeight) {
					double factor = (double)maxHeight / (double)height;
					height = maxHeight;
					width = (int)(width * factor);
				}

				//	result = Bitmap.CreateScaledBitmap(bitmap, width, height, true);
				if (options.OutWidth > width || options.OutHeight > height) {
					// Calculate ratios of height and width to requested height and width
					int heightRatio = Convert.ToInt32(Math.Round((double) options.OutHeight / (double) height));
					int widthRatio = Convert.ToInt32(Math.Round((double) options.OutWidth / (double) width));

					// Choose the smallest ratio as inSampleSize value, this will guarantee
					// a final image with both dimensions larger than or equal to the
					// requested height and width.
					options.InSampleSize = heightRatio < widthRatio ? heightRatio : widthRatio;
				} else {
					options.InSampleSize = 1;
				}

				// Decode bitmap with InSampleSize set
				options.InJustDecodeBounds = false;        


				if (options.OutWidth == width && options.OutHeight == height) {
					// Cave: If width and height is the same, CreateScaledBitmap returns the same bitmap, not a new one :(
					result = BitmapFactory.DecodeByteArray (media.Data, 0, media.Data.Length, options); 
				} else {
					using (Bitmap bm = BitmapFactory.DecodeByteArray (media.Data, 0, media.Data.Length, options)) { 
						result = Bitmap.CreateScaledBitmap(bm, width, height, true);
					}
				}
			}

			return result;
		}

		public void Feedback()
		{
			if (Main.Prefs.FeedbackSound) {
				// This will be half of the default system sound
				float vol = 1.0f; 
				audioManager.PlaySoundEffect(SoundEffect.KeyClick, vol);
			}
			if (Main.Prefs.FeedbackVibration) {
				vibrator.Vibrate(120);
			}
		}

		#endregion

		#region Events of Engine

		/// <summary>
		/// Is called, when an attribute changed event of an objecty occures.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnAttributeChanged(Object sender, AttributeChangedEventArgs e)
		{
			// The easiest way is to redraw all screens
			if (engine != null)
				Refresh ();
		}

		/// <summary>
		/// Raised, if the cartridge is complete.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		void OnCartridgeComplete (object sender, WherigoEventArgs args)
		{
			// TODO: Implementation
//			throw new NotImplementedException ();
		}

		/// <summary>
		/// Get the input e.Input from the player.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnGetInput (Object sender, ObjectEventArgs<Input> args)
		{
			var bar = SupportActionBar;
			var ft = this.SupportFragmentManager.BeginTransaction ();
			var activeFragment = this.SupportFragmentManager.FindFragmentByTag("active");

			bar.SetDisplayHomeAsUpEnabled (false);
			ft.SetTransition (global::Android.Support.V4.App.FragmentTransaction.TransitNone);
			ft.Replace (Resource.Id.fragment, new GameDialogScreen (args.Object), "active");
			ft.Commit ();

			// Delete last fragment
//			if (activeFragment != null)
//				activeFragment.Dispose();
		}

		/// <summary>
		/// Is called, when an inventory changed event occures.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnInventoryChanged(Object sender, InventoryChangedEventArgs e)
		{
			// The easiest way is to redraw all screens
			if (engine != null)
				Refresh ();
		}

		/// <summary>
		/// Log the message e.MessageRaises the log message event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnLogMessage (Object sender, LogMessageEventArgs args)
		{
			if (args.Level <= logLevel)
			{
				// TODO: Remove
				Console.WriteLine (engine.CreateLogMessage (args.Message));

				if (logFile != null)
				{
					// Create log entry
					logFile.WriteLine (engine.CreateLogMessage (args.Message));
				}
			}
		}

		/// <summary>
		/// Raises the play alert event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		public void OnPlayAlert(Object sender, WherigoEventArgs args)
		{
			// TODO: Implement
		}

		/// <summary>
		/// Play the media e.Media on device.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnPlayMedia (Object sender, ObjectEventArgs<Media> args)
		{
			if (args.Object.Data != null)
			{
				switch (args.Object.Type) {
					case MediaType.MP3:
						PlayMedia (args.Object);
						break;
					case MediaType.WAV:
						PlayMedia (args.Object);
						break;
				case MediaType.OGG:
						PlayMedia (args.Object);
						break;
					case MediaType.FDL:
						break;
				}
			}
		}

		/// <summary>
		/// Is called, when an location changed event occures.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnRefreshLocation(Object sender, WF.Player.Location.LocationChangedEventArgs e)
		{
			GPSLocation loc = Main.GPS.Location;

			if (engine != null && !loc.Equals(engine.Latitude, engine.Longitude, engine.Altitude, engine.Accuracy))
				engine.RefreshLocation (loc.Latitude, loc.Longitude, loc.Altitude, loc.Accuracy);
		}

		/// <summary>
		/// Save the cartridge.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Event arguments.</param>
		public void OnSaveCartridge (object sender, WherigoEventArgs args)
		{
			engine.Save (new FileStream (args.Cartridge.SaveFilename, FileMode.Create));
		}

		/// <summary>
		/// Show message e.Text with media e.Media to player.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnShowMessageBox(Object sender, MessageBoxEventArgs args)
		{
			var bar = SupportActionBar;
			var ft = this.SupportFragmentManager.BeginTransaction ();
			var activeFragment = this.SupportFragmentManager.FindFragmentByTag("active");

			bar.SetDisplayHomeAsUpEnabled (false);
			ft.SetTransition (global::Android.Support.V4.App.FragmentTransaction.TransitNone);
			ft.Replace (Resource.Id.fragment, new GameDialogScreen (args.Descriptor), "active");
			ft.Commit ();

			// Delete last fragment
//			if (activeFragment != null)
//				activeFragment.Dispose();
		}

		/// <summary>
		/// Show screen e.Screen to player. If needed, there is an object index in e.IndexObject.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnShowScreen(Object sender, ScreenEventArgs e)
		{
			ShowScreen ((ScreenTypes)e.Screen, e.Object);
		}

		/// <summary>
		/// Show 
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		public void OnShowStatusText (Object sender, StatusTextEventArgs e)
		{
			Toast.MakeText (this, e.Text, ToastLength.Long);
		}

		/// <summary>
		/// Stop sound.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		public void OnStopSound(Object sender, WherigoEventArgs args)
		{
			mediaPlayer.Stop ();
		}

		/// <summary>
		/// Is called, when a zone state changed event occures.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnZoneStateChanged(Object sender, ZoneStateChangedEventArgs e)
		{
			// The easiest way is to redraw all screens
			if (engine != null)
				Refresh ();
		}

		#endregion

		#region Private functions

		/// <summary>
		/// Draws image for inside of a zone.
		/// </summary>
		/// <returns>Bitmap of inside.</returns>
		internal Bitmap DrawCenter ()
		{
			int w = 48;
			int h = 48;

			Bitmap b = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);

			using(Canvas c = new Canvas(b)) {
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
			}

			return b;
		}

		/// <summary>
		/// Draws image of arrow with given direction.
		/// </summary>
		/// <returns>Bitmap of arrow.</returns>
		/// <param name="direction">Direction of the arrow.</param>
		internal Bitmap DrawArrow (double direction)
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
			using(Canvas c = new Canvas(b)) {
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
			}

			return b;
		}

		/// <summary>
		/// Creates the engine and sets all event handlers.
		/// </summary>
		/// <returns>The engine.</returns>
		/// <param name="cart">Cart.</param>
		public void CreateEngine (Cartridge cart)
		{
			var helper = new AndroidPlatformHelper(ApplicationContext);
			helper.Ctrl = this;

			engine = new Engine (helper);

			// Set all events for engine
			engine.AttributeChanged += OnAttributeChanged;
			engine.InventoryChanged += OnInventoryChanged;
			engine.ZoneStateChanged += OnZoneStateChanged;
			engine.CartridgeCompleted += OnCartridgeComplete;
			engine.InputRequested += OnGetInput;
			engine.LogMessageRequested += OnLogMessage;
			engine.PlayAlertRequested += OnPlayAlert;
			engine.PlayMediaRequested += OnPlayMedia;
			engine.SaveRequested += OnSaveCartridge;
			engine.ShowMessageBoxRequested += OnShowMessageBox;
			engine.ShowScreenRequested += OnShowScreen;
			engine.ShowStatusTextRequested += OnShowStatusText;
			engine.StopSoundsRequested += OnStopSound;

			// If there is a old logFile, close it
			if (logFile != null) {
				logFile.Flush ();
				logFile.Close ();
			}

			// Open logFile first time
			logFile = new StreamWriter(cart.LogFilename, true, System.Text.Encoding.UTF8);
			logFile.AutoFlush = true;

			// Open GPX file for the first time
//			if (!String.IsNullOrEmpty(cartridge.GpxFilename)) {
//				// Create new Gpx object
//				gpxFile = new GpxClass ();
//
//				if (File.Exists (cartridge.SaveFilename)) {
//					// Get existing data
//					gpxFile.FromFile (cartridge.SaveFilename);
//					if (gpxFile.trk == null)
//						gpxFile.trk = new trkTypeCollection ();
//				} else {
//					// Create new Gpx file
//					gpxFile.metadata = new metadataType () {
//						author=new personType(){name=WindowsIdentity.GetCurrent().Name},
//						link=new linkTypeCollection().AddLink(new linkType(){ href="www.BlueToque.ca",  text="Blue Toque Software" })
//					};
//					gpxFile.trk = new trkTypeCollection ();
//				}

				// Create new track segment
//				gpxFile.trk.trksgt = new trksegTypeCollection ();
//				gpxFile.trk.trksgt
//			}

			engine.Init (new FileStream (cart.Filename,FileMode.Open), cart);
		}

		private void DestroyEngine()
		{
			if (engine != null) {
				engine.Stop();
				engine.Reset();

				engine.AttributeChanged -= OnAttributeChanged;
				engine.InventoryChanged -= OnInventoryChanged;
				engine.ZoneStateChanged -= OnZoneStateChanged;
				engine.CartridgeCompleted -= OnCartridgeComplete;
				engine.InputRequested -= OnGetInput;
				engine.LogMessageRequested -= OnLogMessage;
				engine.PlayAlertRequested -= OnPlayAlert;
				engine.PlayMediaRequested -= OnPlayMedia;
				engine.SaveRequested -= OnSaveCartridge;
				engine.ShowMessageBoxRequested -= OnShowMessageBox;
				engine.ShowScreenRequested -= OnShowScreen;
				engine.ShowStatusTextRequested -= OnShowStatusText;
				engine.StopSoundsRequested -= OnStopSound;

				engine.Dispose();

				engine = null;
			}

			// TODO: If there is a AusoSave file, delete it.

			// If there is a old logFile, close it
			if (logFile != null) {
				logFile.Flush ();
				logFile.Close ();
				logFile = null;
			}
		}

		/// <summary>
		/// Plaies a media sound file.
		/// </summary>
		/// <param name="media">Media.</param>
		private async void PlayMedia (Media media)
		{
			try {
				// Reset MediaPlayer to be ready for the next sound
				mediaPlayer.Reset();

				// Open file and read from FileOffset FileSize bytes for the media
				using (Java.IO.RandomAccessFile file = new Java.IO.RandomAccessFile(media.FileName,"r")) {
					await mediaPlayer.SetDataSourceAsync(file.FD,media.FileOffset,media.FileSize);
					file.Close();
				}

				// Start media
				mediaPlayer.Prepare();
				mediaPlayer.Start();
			} catch (Exception ex) {
				String s = ex.ToString();
			}
		}

		/// <summary>
		/// Refresh screen, if something changes.
		/// </summary>
		void Refresh()
		{
			var view = this.FindViewById(Resource.Id.fragment);
			view.Invalidate ();
		}

		ScreenTypes ActiveScreenType()
		{
			ScreenTypes result = ScreenTypes.Main;

			// Get active screen type
			if (SupportFragmentManager.Fragments [0] is GameMainScreen)
				result = ScreenTypes.Main;
			if (SupportFragmentManager.Fragments [0] is GameListScreen)
				result = ((GameListScreen)SupportFragmentManager.Fragments [0]).Type;
			if (SupportFragmentManager.Fragments [0] is GameDetailScreen)
				result = ScreenTypes.Details;
			if (SupportFragmentManager.Fragments [0] is GameDialogScreen)
				result = ScreenTypes.Dialog;
			if (SupportFragmentManager.Fragments [0] is GameMapScreen)
				result = ScreenTypes.Map;

			return result;
		}

		#endregion

	}
}

