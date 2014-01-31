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
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace WF.Player.Android
{
	public class CheckLocation : global::Android.Support.V4.App.Fragment
	{
		ScreenController ctrl;
		Location activeLocation;
		TextView textDescription;
		TextView textCoordinates;
		TextView textAccuracy;
		Button button;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((ScreenController)this.Activity);

			if (container == null)
				return null;

			var view = inflater.Inflate(Resource.Layout.CheckLocation, container, false);

			textDescription = view.FindViewById<TextView> (Resource.Id.textDescription);
			textCoordinates = view.FindViewById<TextView> (Resource.Id.textCoords);
			textAccuracy = view.FindViewById<TextView> (Resource.Id.textAccuracy);
			button = view.FindViewById<Button> (Resource.Id.buttonStart);

			button.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light_red);
			button.Click += OnButtonClicked;

			return view;
		}

		public override void OnResume()
		{
			base.OnResume();

			// Add to GPS
			MainApp.Instance.GPS.LocationChanged += OnRefreshLocation;

			Refresh();
		}

		public override void OnPause()
		{
			base.OnPause ();

			// Remove from GPS
			MainApp.Instance.GPS.LocationChanged -= OnRefreshLocation;
		}

		void OnButtonClicked (object sender, EventArgs e)
		{
			ctrl.InitController(true);
		}

		void OnRefreshLocation (object sender, global::Android.Locations.LocationChangedEventArgs e)
		{
			Refresh();
		}

		void Refresh()
		{
			((ScreenController)Activity).SupportActionBar.Title = Strings.GetString("GPS Check");

			textDescription.Text = Strings.GetString("For much fun with the cartridge, you should wait for a good accuracy of your GPS signal.");
			if (MainApp.Instance.GPS.IsValid) {
				textCoordinates.Text = Strings.GetStringFmt("Current Coordinates\n{0}", coordinatesToString(MainApp.Instance.GPS.Latitude, MainApp.Instance.GPS.Longitude));
				textAccuracy.Text = Strings.GetStringFmt("Current Accuracy\n{0} m", (int)MainApp.Instance.GPS.Accuracy);
			} else {
				textCoordinates.Text = Strings.GetStringFmt("Current Coordinates\n{0}", Strings.GetString("unknown"));
				textAccuracy.Text = Strings.GetStringFmt("Current Accuracy\n{0} m", Strings.Infinite);
			}

			if (MainApp.Instance.GPS.IsValid &&MainApp.Instance.GPS.Accuracy < 30) {
				button.Text = Strings.GetString("Start");
				button.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light_green);
			} else {
				button.Text = Strings.GetString("Start anyway");
				button.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light_red);
			}
		}

		string coordinatesToString(double lat, double lon)
		{
			string latDirect;
			string lonDirect;
			int latDegrees;
			int lonDegrees;
			int latMin;
			int lonMin;
			int latSec;
			int lonSec;
			double latDecimalMin;
			double lonDecimalMin;

			latDirect = lat > 0 ? Strings.GetString("N") : Strings.GetString("S");
			lonDirect = lon > 0 ? Strings.GetString("E") : Strings.GetString("W");

			latDegrees = Convert.ToInt32 (Math.Floor(lat));
			lonDegrees = Convert.ToInt32 (Math.Floor(lon));

			latMin = Convert.ToInt32 (Math.Floor((lat - latDegrees) * 60.0));
			lonMin = Convert.ToInt32 (Math.Floor((lon - lonDegrees) * 60.0));

			latSec = Convert.ToInt32 (Math.Floor((((lat - latDegrees) * 60.0) - latMin) * 60.0));
			lonSec = Convert.ToInt32 (Math.Floor((((lon - lonDegrees) * 60.0) - lonMin) * 60.0));

			latDecimalMin = Math.Round((lat - latDegrees) * 60.0, 3);
			lonDecimalMin = Math.Round((lon - lonDegrees) * 60.0, 3);

			var format = 1;
			string result = "";

			switch (format) {
			case 0:
				result = String.Format ("{0} {1:0.00000}° {2} {3:0.00000}°", new object[] {
					latDirect,
					lat,
					lonDirect,
					lon
				});
				break;
			case 1:
				result = String.Format ("{0} {1:00}° {2:00.000}' {3} {4:000}° {5:00.000}'", new object[] {
					latDirect,
					latDegrees,
					latDecimalMin,
					lonDirect,
					lonDegrees,
					lonDecimalMin
				});
				break;
			case 2:
				result = String.Format ("{0} {1:00}° {2:00}' {3:00.0}\" {4} {5:000}° {6:00}' {7:00.0}\"", new object[] {
					latDirect,
					latDegrees,
					latMin,
					latSec,
					lonDirect,
					lonDegrees,
					lonMin,
					lonSec
				});
				break;
			}

			return result;
		}
	}
}

