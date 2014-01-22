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
		private LocationManager locManager;
		private double lat;
		private double lon;
		private double alt;
		private double accuracy;
		private bool valid;

		public event EventHandler<LocationChangedEventArgs> LocationChanged;

		public LocListener(LocationManager lm) : base()
		{
			// Get LocationManager
			locManager = lm;
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

		public void Start()
		{
			var locationCriteria = new Criteria ()
			{
				Accuracy = global::Android.Locations.Accuracy.Fine,
				AltitudeRequired = true,
				PowerRequirement = Power.Low
			};

			string locationProvider = locManager.GetBestProvider(locationCriteria, true);

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

			// Now activate location updates
			locManager.RequestLocationUpdates(locationProvider, 500, 1, this);

			valid = false;
		}

		public void Stop()
		{
			locManager.RemoveUpdates(this);
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
			// TODO: Remove
			Console.WriteLine ("Provider disabled");
		}

		public void OnProviderEnabled (string provider)
		{
			valid = false;
			// TODO: Remove
			Console.WriteLine ("Provider endisabled");
		}

		public void OnStatusChanged (string provider, Availability status, Bundle extras)
		{
			// TODO: Remove
			Console.WriteLine ("Status changed: {0} is {1}",provider,status.ToString());
		}
	}
}

