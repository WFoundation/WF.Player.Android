///
/// WF.Player.Android - A Wherigo Player for Android, which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014  Dirk Weltz <web@weltz-online.de>
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
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Vernacular;
using WF.Player.Core;
using WF.Player.Core.Live;
using WF.Player.Location;
using WF.Player.Preferences;

namespace WF.Player
{
	[Activity (Label = "WF.Player", Theme="@android:style/Theme.NoTitleBar")]			
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			Main.SetTheme(this);

			base.OnCreate (bundle);

			if (String.IsNullOrEmpty (Main.Path)) {
				Finish ();
				return;
			}

			Catalog.Implementation = new Vernacular.AndroidCatalog (Resources, typeof (Resource.String));

			// Create object for location listener
			Main.GPS = new GPSListener (GetSystemService (Context.LocationService) as LocationManager, GetSystemService (Context.SensorService) as SensorManager, this.WindowManager);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			ISharedPreferences pref = GetPreferences (FileCreationMode.WorldWriteable);

			Main.Prefs.GetString ("Username", GetString (Resource.String.main_unknown));

			var textUsername = FindViewById<TextView> (Resource.Id.textUsername);
			textUsername.Text = "charlenni";

			var textFounds = FindViewById<TextView> (Resource.Id.textFounds);
			textFounds.Text = String.Format (GetString (Resource.String.main_founds),0);

			var textCoordText = FindViewById<TextView> (Resource.Id.textCoordText);
			textCoordText.Text = GetString (Resource.String.main_last_known_location);

			var textCoords = FindViewById<TextView> (Resource.Id.textCoords);
			textCoords.Text = GetString (Resource.String.main_unknown_location);

			var textAccuracyText = FindViewById<TextView> (Resource.Id.textAccuracyText);
			textAccuracyText.Text = GetString (Resource.String.main_accuracy);

			var textAccuracy = FindViewById<TextView> (Resource.Id.textAccuracy);
			textAccuracy.Text = String.Format ("{0} m", Strings.Infinite);

			var buttonOffline = FindViewById<Button>(Resource.Id.buttonOffline);
			buttonOffline.Click += buttonOfflineClick;

			var buttonNearby = FindViewById<Button>(Resource.Id.buttonNearby);
			buttonNearby.Enabled = false;
			buttonNearby.Click += buttonNearbyClick;

			var buttonSearchParam = FindViewById<Button>(Resource.Id.buttonSearchParam);
			buttonSearchParam.Enabled = false;
			buttonSearchParam.Click += buttonSearchParamClick;

			var buttonSearchWGCode = FindViewById<Button>(Resource.Id.buttonSearchWGCode);
			buttonSearchWGCode.Enabled = false;
			buttonSearchWGCode.Click += buttonSearchWGCodeClick;

			var buttonSettings = FindViewById<Button>(Resource.Id.buttonSettings);
			buttonSettings.Click += buttonSettingsClick;
		}

		protected override void OnResume()
		{
			base.OnResume ();

			// Add to location listener
			Main.GPS.AddLocationListener(OnLocationChanged);
		}

		protected override void OnPause()
		{
			base.OnPause ();

			// Remove to location listener
			Main.GPS.RemoveLocationListener(OnLocationChanged);
		}

		public void buttonOfflineClick(object sender, EventArgs args)
		{ 
			Cartridges carts = new Cartridges();
			List<string> fileList = new List<string> ();
			FileInfo[] files = null;

			string path = Main.Path;

			try {
				// Read all GWC, GWZ, WFC and WFZ from default directory
				files = new DirectoryInfo(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Java.IO.File.Separator + "WF.Player").GetFiles("*");
			}
			catch(Exception e) {
				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetTitle (GetString (Resource.String.main_error));
				builder.SetMessage(e.Message);
				builder.SetCancelable (true);
				builder.SetNeutralButton(Resource.String.ok,(obj,arg) => { });
				builder.Show ();
				return;
			}

			foreach (FileInfo fi in files)
			{
				string ext = Path.GetExtension (fi.Name).ToUpper ();
				if (ext.Equals (".GWC") || ext.Equals (".GWZ") || ext.Equals ("WFC") || ext.Equals ("WFZ"))
					fileList.Add (fi.FullName);
			}

			if (fileList.Count == 0)
			{
				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetTitle (GetString (Resource.String.main_error));
				builder.SetMessage(String.Format(GetString(Resource.String.main_error_no_cartridges),path));
				builder.SetCancelable (true);
				builder.SetNeutralButton(Resource.String.ok,(obj,arg) => { });
				builder.Show ();
			}

			// Create CartridgesList
			carts.GetByFileList (fileList);

			((MainApp)this.Application).Cartridges = carts;

			Intent intent = new Intent (this, typeof(CartridgesActivity));
			StartActivity (intent);
		}

		public void buttonSearchWGCodeClick(object sender, EventArgs args)
		{ 
			// TODO: Insert token for user in constructor
			Cartridges carts = new Cartridges ();

			carts.BeginGetByName (FindViewById<EditText> (Resource.Id.editWGCode).Text);

			((MainApp)this.Application).Cartridges = carts;

			Intent intent = new Intent (this, typeof(CartridgesActivity));
			StartActivity (intent);
		}

		public void buttonSearchParamClick(object sender, EventArgs args)
		{ 
			Intent intent = new Intent (this, typeof(SearchActivity));
			intent.PutExtra ("Cartridges", "Test");
			StartActivity (intent);
		}

		void buttonNearbyClick (object sender, EventArgs e)
		{
		}

		void buttonSettingsClick (object sender, EventArgs e)
		{
			Intent intent = new Intent (this, typeof(PreferencesActivity));
			intent.PutExtra ("Cartridges", "Test");
			StartActivity (intent);
		}

		public void OnLocationChanged(object sender, WF.Player.Location.LocationChangedEventArgs e)
		{
			var textCoordText = FindViewById<TextView> (Resource.Id.textCoordText);
			textCoordText.Text = e.Location.HasAccuracy ? GetString (Resource.String.main_active_location) : GetString (Resource.String.main_last_known_location);

			var textCoords = FindViewById<TextView> (Resource.Id.textCoords);
			textCoords.Text = e.Location.ToString();

			var textAccuracy = FindViewById<TextView> (Resource.Id.textAccuracy);
			textAccuracy.Text = String.Format ("{0:0} m", e.Location.Accuracy);
		}
	}

}

