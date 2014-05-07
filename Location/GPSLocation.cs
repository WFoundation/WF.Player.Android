///
/// WF.Player - A Wherigo Player, which use the Wherigo Foundation Core.
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
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Vernacular;

namespace WF.Player.Location
{
	public enum GPSFormat {
		Decimal,
		DecimalMinutes,
		DecimalMinutesSeconds
	}

	public partial class GPSLocation
	{
		#region Constructor

		public GPSLocation()
		{
			SetDefaults();
		}

		public GPSLocation(GPSLocation loc)
		{
			SetDefaults();

			// Copy position
			_lat = loc.Latitude;
			_lon = loc.Longitude;
			_time = loc.Time;
			_provider = loc.Provider;
			_isValid = loc.IsValid;

			// Copy Altitude only if exists
			if (loc.HasAltitude)
				_alt = loc.Altitude;
			else
				_alt = double.NaN;

			// Copy Accruracy only if exists
			if (loc.HasAccuracy) {
				_accuracy = loc.Accuracy;
				_hasAccuracy = true;
			} 

			// Copy Speed only if exists
			if (loc.HasSpeed) {
				_speed = loc.Speed;
				_hasSpeed = true;
			}

			// Copy Bearing only if exists
			if (loc.HasBearing) {
				_bearing = loc.Bearing;
				_hasBearing = true;
			}
		}

		#endregion

		#region Members

		string _provider;

		/// <summary>
		/// Provider of this location.
		/// </summary>
		/// <value>The provider.</value>
		public string Provider
		{
			get { return _provider; }
			set { _provider = value; }
		}

		bool _isValid;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is valid.
		/// </summary>
		/// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
		public bool IsValid
		{
			get { return _isValid; }
			internal set { _isValid = value; }
		}


		double _lat;

		/// <summary>
		/// Gets or sets the latitude.
		/// </summary>
		/// <value>The latitude.</value>
		public double Latitude
		{
			get { return _lat; }
			set { _lat = value; }
		}

		double _lon;

		/// <summary>
		/// Gets or sets the longitude.
		/// </summary>
		/// <value>The longitude.</value>
		public double Longitude
		{
			get { return _lon; }
			set { _lon = value; }
		}

		double _alt;
		bool _hasAltitude;

		/// <summary>
		/// Gets or sets the altitude.
		/// </summary>
		/// <value>The altitude.</value>
		public double Altitude
		{
			get { return _alt; }
			set { 
				_alt = value;
				_hasAltitude = true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has altitude.
		/// </summary>
		/// <value><c>true</c> if this instance has altitude; otherwise, <c>false</c>.</value>
		public bool HasAltitude
		{
			get { return _hasAltitude; }
		}

		double _speed;
		bool _hasSpeed;

		/// <summary>
		/// Gets or sets the speed.
		/// </summary>
		/// <value>The speed.</value>
		public double Speed
		{
			get { return _speed; }
			set { 
				_speed = value;
				_hasSpeed = true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has speed.
		/// </summary>
		/// <value><c>true</c> if this instance has speed; otherwise, <c>false</c>.</value>
		public bool HasSpeed
		{
			get { return _hasSpeed; }
		}

		double _accuracy;
		bool _hasAccuracy;

		/// <summary>
		/// Gets or sets the accuracy.
		/// </summary>
		/// <value>The accuracy.</value>
		public double Accuracy
		{
			get { return _accuracy; }
			set { 
				_accuracy = value;
				_hasAccuracy = true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has accuracy.
		/// </summary>
		/// <value><c>true</c> if this instance has accuracy; otherwise, <c>false</c>.</value>
		public bool HasAccuracy
		{
			get { return _hasAccuracy; }
		}

		double _bearing;
		bool _hasBearing;

		/// <summary>
		/// Gets or sets the bearing.
		/// </summary>
		/// <value>The bearing.</value>
		public double Bearing
		{
			get { return _bearing; }
			set { 
				_bearing = value;
				_hasBearing = true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has bearing.
		/// </summary>
		/// <value><c>true</c> if this instance has bearing; otherwise, <c>false</c>.</value>
		public bool HasBearing
		{
			get { return _hasBearing; }
		}

		DateTime _time;

		/// <summary>
		/// Gets or sets the time.
		/// </summary>
		/// <value>The time.</value>
		public DateTime Time
		{
			get { return _time; }
			set { _time = value; }
		}

		#endregion

		#region Methods

		public bool Equals(double lat, double lon, double alt, double accuracy)
		{
			return (lat == _lat && lon == _lon && alt == _alt && accuracy == _accuracy);
		}

		#endregion

		#region Display

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="WF.Player.Location.GPSLocation" in default format./>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="WF.Player.Location.GPSLocation"/>.</returns>
		public override string ToString ()
		{
			return ToString(GPSFormat.DecimalMinutes);
		}

		/// <summary>
		/// Convert this location in a readable format.
		/// </summary>
		/// <returns>Location in readable format.</returns>
		/// <param name="format">Format.</param>
		public string ToString(GPSFormat format)
		{
			string latDirect;
			string lonDirect;
			int latDegrees;
			int lonDegrees;
			int latMin = 0;
			int lonMin = 0;
			int latSec = 0;
			int lonSec = 0;
			double latDecimalMin = 0;
			double lonDecimalMin = 0;

			latDirect = _lat > 0 ? Catalog.GetString("N", comment: "Direction North") : Catalog.GetString("S", comment: "Direction South");
			lonDirect = _lon > 0 ? Catalog.GetString("E", comment: "Direction East") : Catalog.GetString("W", comment: "Direction West");

			latDegrees = Convert.ToInt32 (Math.Floor(_lat));
			lonDegrees = Convert.ToInt32 (Math.Floor(_lon));

			if (format == GPSFormat.DecimalMinutes || format == GPSFormat.DecimalMinutesSeconds) {
				latDecimalMin = Math.Round((_lat - latDegrees) * 60.0, 3);
				lonDecimalMin = Math.Round((_lon - lonDegrees) * 60.0, 3);

				latMin = Convert.ToInt32 (Math.Floor((_lat - latDegrees) * 60.0));
				lonMin = Convert.ToInt32 (Math.Floor((_lon - lonDegrees) * 60.0));
			}

			if (format == GPSFormat.DecimalMinutesSeconds) {
				latSec = Convert.ToInt32 (Math.Floor((((_lat - latDegrees) * 60.0) - latMin) * 60.0));
				lonSec = Convert.ToInt32 (Math.Floor((((_lon - lonDegrees) * 60.0) - lonMin) * 60.0));
			}

			string result = "";

			switch (format) {
				case GPSFormat.Decimal:
					result = String.Format ("{0} {1:0.00000}° {2} {3:0.00000}°", latDirect, _lat, lonDirect, _lon);
					break;
				case GPSFormat.DecimalMinutes:
					result = String.Format ("{0} {1:00}° {2:00.000}' {3} {4:000}° {5:00.000}'", latDirect, latDegrees, latDecimalMin, lonDirect, lonDegrees, lonDecimalMin);
					break;
				case GPSFormat.DecimalMinutesSeconds:
					result = String.Format ("{0} {1:00}° {2:00}' {3:00.0}\" {4} {5:000}° {6:00}' {7:00.0}\"", latDirect, latDegrees, latMin, latSec, lonDirect, lonDegrees,	lonMin,	lonSec);
					break;
			}

			return result;
		}

		public string ToAccuracyString()
		{
			if (_hasAccuracy)
				return String.Format("{0} m", (int)_accuracy);
			else
				return String.Format("{0} m", Strings.Infinite);
		}

		#endregion

		#region Private Functions

		void SetDefaults()
		{
			_provider = "";
			_time = DateTime.Now;
			_lat = double.NaN;
			_lon = double.NaN;
			_alt = double.NaN;
			_hasAltitude = false;
			_accuracy = double.NaN;
			_hasAccuracy = false;
			_speed = double.NaN;
			_hasSpeed = false;
			_bearing = double.NaN;
			_hasBearing = false;
			_isValid = false;
		}

		#endregion
	}
}

