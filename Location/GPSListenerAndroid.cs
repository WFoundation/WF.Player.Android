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
using Android.Gms.Maps;
using Vernacular;

namespace WF.Player.Location
{
	/// <summary>
	/// GPS listener for Android.
	/// </summary>
	/// <remarks>Handles all things in conjunction with the Android GPS and compass.
	/// Each time, it gets a new location or bearing update, the belonging events are called.
	/// </remarks>
	public partial class GPSListener : Java.Lang.Object, ILocationListener, ISensorEventListener, ILocationSource
	{
		static LocationManager locManager;
		static SensorManager sensorManager;
		ILocationSourceOnLocationChangedListener mapsListener = null;
		Timer timer;
		Sensor accelerometer;
		Sensor magnetometer;
		string locationProvider;

		public GPSListener(LocationManager lm, SensorManager sm) : base()
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
			GPSLocation loc = null;

			if (providers != null) {
				for (int i = providers.Count - 1; i >= 0; i--) {
					loc = new GPSLocation(locManager.GetLastKnownLocation(providers[i]));
					if (loc != null) 
						break;
				}
			}

			// If we have an old location, than use this a first location
			if (loc != null)
			{
				_location = new GPSLocation(loc);
			} else {
				_location = new GPSLocation();
			}

			sensorManager = sm;
			accelerometer = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
			magnetometer = sensorManager.GetDefaultSensor(SensorType.MagneticField);
		}

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

			_valid = false;
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

		#endregion

		#region Google Maps

		public void Activate(ILocationSourceOnLocationChangedListener listener)
		{
			mapsListener = listener;
		}

		public void Deactivate()
		{
			mapsListener = null;
		}

		#endregion

		#region Events

		void OnTimerTick (object sender, ElapsedEventArgs e)
		{
			locManager.RemoveUpdates(this);
			sensorManager.UnregisterListener(this);

			_valid = false;

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
		public void OnLocationChanged (global::Android.Locations.Location l)
		{
			_location = new GPSLocation(l);
			if (_location != null)
				_valid = true;
			if (LocationChanged != null)
				LocationChanged (this, new LocationChangedEventArgs (_location));
			// If Google Maps is active, than send new location
			if (mapsListener != null)
				mapsListener.OnLocationChanged(l);
		}

		/// <summary>
		/// Called when the provider is disabled by the user.
		/// </summary>
		/// <param name="provider">the name of the location provider associated with this
		///  update.</param>
		public void OnProviderDisabled (string provider)
		{
			_valid = false;
		}

		/// <summary>
		/// Called when the provider is enabled by the user.
		/// </summary>
		/// <param name="provider">the name of the location provider associated with this
		///  update.</param>
		public void OnProviderEnabled (string provider)
		{
			_valid = false;
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
			if (status == Availability.Available)
				_valid = true;
			else
				_valid = false;

			// TODO: Remove
			Console.WriteLine ("Status changed: {0} is {1}",provider,status.ToString());
		}

		#endregion

		#region SensorListener Events

		float[] gravity;
		float[] geomagnetic;
		double bearing;

		public double Bearing {
			get { return bearing; }
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
					// double azimuth = Math.toDegrees(matrixValues[0]);
					// double pitch = Math.toDegrees(matrixValues[1]);
					// double roll = Math.toDegrees(matrixValues[2]);
					// orientation contains: azimut, pitch and roll
					var newBearing = orientation[0] < 0 ? 360.0 + orientation[0] * 180.0 / Math.PI : orientation[0] * 180.0 / Math.PI;  
					if (Math.Abs(bearing - newBearing) >= 5.0) {
						bearing = newBearing;
						if (BearingChanged != null)
							BearingChanged(this, new BearingChangedEventArgs(bearing));
					}
				}
			}
		}

		#endregion

	}
}

