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
using System.Timers;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

namespace WF.Player.Android
{
	public class LocListener : Java.Lang.Object, ILocationListener, ISensorEventListener
	{
		LocationManager locManager;
		SensorManager sensorManager;
		Timer timer;
		Sensor accelerometer;
		Sensor magnetometer;
		string locationProvider;
		Location location;
		double lat;
		double lon;
		double alt;
		double accuracy;
		bool valid;
		bool hasAltitude;
		bool hasAccuracy;

		public event EventHandler<LocationChangedEventArgs> LocationChanged;
		public event EventHandler<EventArgs> HeadingChanged;

		public LocListener(LocationManager lm, SensorManager sm) : base()
		{
			// Get LocationManager
			locManager = lm;

			var locationCriteria = new Criteria ()
			{
				Accuracy = global::Android.Locations.Accuracy.Fine,
				AltitudeRequired = true,
				PowerRequirement = Power.Low
			};

			locationProvider = locManager.GetBestProvider(locationCriteria, true);

			if (locationProvider == null)
				throw new Exception("No location provider found");

			List<String> providers = locManager.GetProviders(true) as List<String>;

			// Loop over the array backwards, and if you get an accurate location, then break out the loop
			Location loc = null;

			if (providers != null) {
				for (int i = providers.Count - 1; i >= 0; i--) {
					loc = locManager.GetLastKnownLocation(providers[i]);
					if (loc != null) 
						break;
				}
			}

			if (loc != null)
			{
				lat = loc.Latitude;
				lon = loc.Longitude;
				alt = loc.Altitude;
				accuracy = loc.Accuracy;
			}

			sensorManager = sm;
			accelerometer = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
			magnetometer = sensorManager.GetDefaultSensor(SensorType.MagneticField);
		}

		#region Members

		public Location Location {
			get { return location; }
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

		public bool HasAltitude {
			get { return hasAltitude; }
		}

		public bool HasAccuracy {
			get { return hasAccuracy; }
		}

		#endregion

		#region Methods

		public void Start()
		{
			// If there is a timer for stop, than remove it
			if (timer != null) {
				timer.Stop();
				timer = null;
			}

			// Now activate location updates
			locManager.RequestLocationUpdates(locationProvider, 500, 1, this);

			sensorManager.RegisterListener(this, accelerometer, SensorDelay.Ui);
			sensorManager.RegisterListener(this, magnetometer, SensorDelay.Ui);

			valid = false;
		}

		public void Stop()
		{
			if (timer != null) {
				timer.Stop();
			} else {
				timer = new Timer();
			}

			// Set interval to 3 seconds. This is the time while running GPS in background
			timer.Interval = 3000;
			timer.Elapsed += OnTimerTick;
			timer.Start();
		}

		public string CoordinatesToString(double lat, double lon)
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

			var format = 1; // NSUserDefaults.StandardUserDefaults.IntForKey("CoordFormat");
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

		#endregion

		#region Events

		void OnTimerTick (object sender, ElapsedEventArgs e)
		{
			locManager.RemoveUpdates(this);
			sensorManager.UnregisterListener(this);

			valid = false;

			timer.Stop();
			timer.Elapsed -= OnTimerTick;
			timer = null;
		}

		#endregion

		#region LocationListener Events

		/// <summary>
		/// Called when the location has changed.
		/// </summary>
		/// <param name="location">The new location, as a Location object.</param>
		public void OnLocationChanged (Location l)
		{
			location = l;
			lat = location.Latitude;
			lon = location.Longitude;
			hasAltitude = location.HasAltitude;
			if (location.HasAltitude)
				alt = location.Altitude;
			hasAccuracy = location.HasAccuracy;
			if (location.HasAccuracy)
				accuracy = location.Accuracy;
			valid = true;
			if (LocationChanged != null)
				LocationChanged (this, new LocationChangedEventArgs (location));
		}

		/// <summary>
		/// Called when the provider is disabled by the user.
		/// </summary>
		/// <param name="provider">the name of the location provider associated with this
		///  update.</param>
		public void OnProviderDisabled (string provider)
		{
			valid = false;
		}

		/// <summary>
		/// Called when the provider is enabled by the user.
		/// </summary>
		/// <param name="provider">the name of the location provider associated with this
		///  update.</param>
		public void OnProviderEnabled (string provider)
		{
			valid = false;
		}

		/// <summary>
		/// Called when the status of the location listener changed.
		/// </summary>
		/// <param name="provider">the name of the location provider associated with this
		///  update.</param>
		/// <param name="status">Status.</param>
		/// <param name="extras">Extras.</param>
		public void OnStatusChanged (string provider, Availability status, Bundle extras)
		{
			// TODO: Remove
			Console.WriteLine ("Status changed: {0} is {1}",provider,status.ToString());
		}

		#endregion

		#region SensorListener Events

		float[] gravity;
		float[] geomagnetic;
		double heading;

		public double Heading {
			get { return heading; }
		}

		public void OnAccuracyChanged(Sensor sensor, SensorStatus status) 
		{
		}

		public void OnSensorChanged(SensorEvent args) 
		{
			if (args.Sensor.Type == SensorType.Accelerometer)
				gravity = args.Values.ToArray();
			if (args.Sensor.Type == SensorType.MagneticField)
				geomagnetic = args.Values.ToArray();
			if (gravity != null && geomagnetic != null) {
				float[] R = new float[9];
				float[] I = new float[9];
				bool success = SensorManager.GetRotationMatrix(R, I, gravity, geomagnetic);
				if (success) {
					float[] orientation = new float[3];
					SensorManager.GetOrientation(R, orientation);
//					double azimuth = Math.toDegrees(matrixValues[0]);
//					double pitch = Math.toDegrees(matrixValues[1]);
//					double roll = Math.toDegrees(matrixValues[2]);
					var newHeading = orientation[0] < 0 ? 360.0 + orientation[0] * 180.0 / Math.PI : orientation[0] * 180.0 / Math.PI;  // orientation contains: azimut, pitch and roll
					if (Math.Abs(heading - newHeading) >= 5.0) {
						heading = newHeading;
						if (HeadingChanged != null)
						HeadingChanged(this, new HeadingEventArgs(heading));
					}
				}
			}
		}

		public sealed class HeadingEventArgs : EventArgs
		{
			double heading;

			public HeadingEventArgs(double head)
			{
				heading = head;
			}

			public double Heading {
				get { return heading; }
			}
		}

		#endregion
	}
}

