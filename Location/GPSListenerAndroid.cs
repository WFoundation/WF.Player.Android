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
		static LocationManager _locManager;
		static SensorManager _sensorManager;
		static IWindowManager _windowManager;
		static double RadToDeg = (180.0 / Math.PI);

		// Base for time convertions from time in seconds since 1970-01-01 to DateTime
		readonly DateTime _baseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		ILocationSourceOnLocationChangedListener mapsListener = null;
		Timer timer;
		Sensor _orientationSensor;
		Sensor _accelerometerSensor;
		double _lastGPSAzimuth;
		double _lastSensorAzimuth;
		double _lastPitch;
		double _lastRoll;
		double _azimuth;
		double _pitch;
		double _roll;
		double _aboveOrBelow = 0.0;

		string locationProvider;

		#region Constructor

		public GPSListener(LocationManager lm, SensorManager sm, IWindowManager wm) : base()
		{
			// Get LocationManager
			_locManager = lm;
			_windowManager = wm;

			var locationCriteria = new Criteria ()
			{
				Accuracy = global::Android.Locations.Accuracy.Fine,
				AltitudeRequired = true,
				PowerRequirement = Power.Low
			};

			locationProvider = _locManager.GetBestProvider(locationCriteria, true);

			if (locationProvider == null)
				throw new Exception("No location provider found");

			List<String> providers = _locManager.GetProviders(true) as List<String>;

			// Loop over the array backwards, and if you get an accurate location, then break out the loop
			GPSLocation loc = null;

			if (providers != null) {
				for (int i = providers.Count - 1; i >= 0; i--) {
					loc = new GPSLocation(_locManager.GetLastKnownLocation(providers[i]));
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

			_sensorManager = sm;

			_orientationSensor = _sensorManager.GetDefaultSensor(SensorType.Orientation);
			_accelerometerSensor = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
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
			_locManager.RequestLocationUpdates(locationProvider, 500, 1, this);

			_sensorManager.RegisterListener(this, _orientationSensor, SensorDelay.Game);
			_sensorManager.RegisterListener(this, _accelerometerSensor, SensorDelay.Game);

			_valid = false;
		}

		public void Stop()
		{
			if (timer != null) {
				timer.Stop();
			} else {
				timer = new Timer();
			}

			// Set interval to 30 seconds. This is the time while running GPS in background
			timer.Interval = 30000;
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
			_locManager.RemoveUpdates(this);
			_sensorManager.UnregisterListener(this);

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
			if (_location != null) {
				_valid = true;
				// Check if the new location has bearing info
				if (_location.HasBearing) {
					_lastGPSAzimuth = _location.Bearing;
					SendOrientation(_lastPitch, _lastRoll);
				}
			}

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

		#region SensorListener Members

		DateTime _lastDeclinationCalculation;
		double _declination = 0;

		/// <summary>
		/// Gets the declination in degrees.
		/// </summary>
		/// <remarks>
		/// The declinations changes are very slow. So it is enough to calculat this very 5 minutes.
		/// </remarks>
		/// <value>The declination.</value>
		public double Declination {
			get {
				DateTime time = DateTime.Now;
				// Compute this only if needed
				if (_lastDeclinationCalculation == null || time.Subtract(_lastDeclinationCalculation).Seconds > 300) {
					using (GeomagneticField _geoField = new GeomagneticField((Single) _location.Latitude, (Single) _location.Longitude, (Single) _location.Altitude, time.Subtract(_baseTime).Milliseconds)) {
						// Save for later use
						_lastDeclinationCalculation = time;
						_declination = _geoField.Declination;
					}
				}

				return _declination;
			}
		}

		#endregion

		#region SensorListener Events

		public void OnAccuracyChanged(Sensor sensor, SensorStatus status) 
		{
		}

		/// <summary>
		/// Function, which is called, when the sensors change.
		/// </summary>
		/// <param name="args">Arguments.</param>
		public void OnSensorChanged(SensorEvent args) 
		{
			switch (args.Sensor.Type) {
			case SensorType.MagneticField:
				break;
			case SensorType.Accelerometer:
				double filter = GetFilterValue();
				_aboveOrBelow = (args.Values[2] * filter) + (_aboveOrBelow * (1.0 - filter));
				break;
			case SensorType.Orientation:
				double azimuth = args.Values[0];
				// Fix to true bearing
				if (Main.Prefs.GetBool("sensor_azimuth_true", true)) {
					azimuth += Declination;
				}

				_azimuth = FilterValue(azimuth, _azimuth);
				_pitch = FilterValue(args.Values[1], _pitch);
				_roll = FilterValue(args.Values[2], _roll);

				_lastSensorAzimuth = _azimuth;

				double rollDef;

				if (_aboveOrBelow < 0) {
					if (_roll < 0) {
						rollDef = -180 - _roll;
					} else {
						rollDef = 180 - _roll;
					}
				} else {
					rollDef = _roll;
				}

				// Adjust the rotation matrix for the device orientation
				SurfaceOrientation screenRotation = _windowManager.DefaultDisplay.Rotation;

				switch (screenRotation) 
				{
				case SurfaceOrientation.Rotation0:
					// no need for change
					break;
				case SurfaceOrientation.Rotation90:
					_lastSensorAzimuth += 90;
					break;
				case SurfaceOrientation.Rotation180:
					_lastSensorAzimuth -= 180;
					break;
				case SurfaceOrientation.Rotation270:
					_lastSensorAzimuth -= 90;
					break;
				}

				SendOrientation(_pitch, rollDef);
				break;
			}
		}

		#endregion

		#region SensorListener Functions

		/// <summary>
		/// Filters the value to flatten the value by combining the actual and last value.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="valueActual">Actual value in degrees.</param>
		/// <param name="valueLast">Last value in degrees.</param>
		private double FilterValue(double valueActual, double valueLast) 
		{
			if (valueActual < valueLast - 180.0) {
				valueLast -= 360.0;
			} else if (valueActual > valueLast + 180.0) {
				valueLast += 360.0;
			}

			double filter = GetFilterValue();

			return valueActual * filter + valueLast * (1.0 - filter);
		}

		/// <summary>
		/// Gets the filter value like it is set in the preferences.
		/// </summary>
		/// <returns>The filter value.</returns>
		double GetFilterValue() 
		{
			switch (Main.Prefs.GetInt("sensor_orientation_filter", 0)) 
			{
			case 1: // PreferenceValues.VALUE_SENSORS_ORIENT_FILTER_LIGHT:
				return 0.20;
			case 2: // PreferenceValues.VALUE_SENSORS_ORIENT_FILTER_MEDIUM:
				return 0.06;
			case 3: // PreferenceValues.VALUE_SENSORS_ORIENT_FILTER_HEAVY:
				return 0.03;
			}

			return 1.0;
		}

		void SendOrientation(double pitch, double roll) 
		{
			double azimuth;

			if (!Main.Prefs.GetBool("sensor_hardware_compass_auto_change", true) || _location.Speed < Main.Prefs.GetDouble("sensor_hardware_compass_auto_change_value", 1.0)) {
				if (!Main.Prefs.GetBool("sensor_hardware_compass", true))
					azimuth = _lastGPSAzimuth;
				else
					// Substract 90° because the bearing 0° is in direction east
					azimuth = _lastSensorAzimuth;
			} else {
				azimuth = _lastGPSAzimuth;
			}

			_lastPitch = pitch;
			_lastRoll = roll;

			_bearing = azimuth;

			if (OrientationChanged != null)
				OrientationChanged(this, new OrientationChangedEventArgs(azimuth, _lastPitch, _lastRoll));
		}

		#endregion

	}
}

