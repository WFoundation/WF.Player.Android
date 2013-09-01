/// WF.Player.Android - A Wherigo Player User Interface for Android platform.
/// Copyright (C) 2012-2013  Dirk Weltz <web@weltz-online.de>
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

namespace WF.Player.Android
{
	public class LocListener :  Java.Lang.Object, ILocationListener
	{
		private double lat;
		private double lon;
		private double alt;
		private double accuracy;
		private bool valid;

		public event EventHandler<LocationChangedEventArgs> LocationChanged;

		public LocationManager locManager;

		public LocListener(LocationManager lm) : base()
		{
			// Get LocationManager
			locManager = lm;

			var locationCriteria = new Criteria ()
			{
				//				Accuracy = Accuracy.NoRequirement,
				AltitudeRequired = false,
				PowerRequirement = Power.NoRequirement
			};

			string locationProvider = lm.GetBestProvider (locationCriteria, true);

			if (locationProvider == null)
				throw new Exception("No location provider found");

			lm.RequestLocationUpdates (locationProvider, 1000, 2, this);

			var loc = lm.GetLastKnownLocation (locationProvider);

			if (loc != null)
			{
				lat = loc.Latitude;
				lon = loc.Longitude;
				alt = loc.Altitude;
				accuracy = loc.Accuracy;
			}

			valid = false;
		}

		public double Latitude {
			get { return lat; }
		}

		public double Longitude {
			get { return lon; }
		}

		public double Altitude {
			get { return alt; }
		}

		public double Accuracy {
			get { return accuracy; }
		}

		public bool IsValid {
			get { return valid; }
		}

		public void OnLocationChanged (Location location)
		{
			lat = location.Latitude;
			lon = location.Longitude;
			if (location.HasAltitude)
				alt = location.Altitude;
			if (location.HasAccuracy)
				accuracy = location.Accuracy;
			valid = true;
			if (LocationChanged != null)
				LocationChanged (this, new LocationChangedEventArgs (location));
		}

		public void OnProviderDisabled (string provider)
		{
			valid = false;
		}

		public void OnProviderEnabled (string provider)
		{
			valid = false;
		}

		public void OnStatusChanged (string provider, Availability status, Bundle extras)
		{

		}
	}
}

