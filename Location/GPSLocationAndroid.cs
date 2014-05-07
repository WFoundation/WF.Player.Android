using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

namespace WF.Player.Location
{
	public partial class GPSLocation
	{
		// Base for time convertions from time in seconds since 1970-01-01 to DateTime
		DateTime _baseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		#region Constructor

		public GPSLocation(global::Android.Locations.Location loc)
		{
			// No valid location
			if (loc == null) {
				SetDefaults();
				return;
			}

			// Seems to be a valid location
			_isValid = true;

			// Copy position
			_provider = loc.Provider;
			_time = _baseTime.AddMilliseconds(loc.Time);
			_lat = loc.Latitude;
			_lon = loc.Longitude;

			// Copy Altitude only if exists
			if (loc.HasAltitude)
				_alt = loc.Altitude;
			else
				_alt = double.NaN;

			// Copy Accruracy only if exists
			if (loc.HasAccuracy) {
				_accuracy = loc.Accuracy;
				_hasAccuracy = true;
			} else {
				_accuracy = double.NaN;
				_hasAccuracy = false;
			}

			// Copy Speed only if exists
			if (loc.HasSpeed) {
				_speed = loc.Speed;
				_hasSpeed = true;
			} else {
				_speed = double.NaN;
				_hasSpeed = false;
			}

			// Copy Bearing only if exists
			if (loc.HasBearing) {
				_bearing = loc.Bearing;
				_hasBearing = true;
			} else {
				_bearing = double.NaN;
				_hasBearing = false;
			}

		}

		#endregion
	}
}

