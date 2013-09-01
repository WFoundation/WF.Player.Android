using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using WF.Player.Core;

namespace WF.Player.Android
{
//	public delegate void LocationChangedEventHandler(Object IntentSender, LocationChangedEventArgs e);

	[Application(Debuggable=true)]
	public class MainApp : Application
	{
		private Activity activeActivity;
		private Cartridges cartridges;
		private LocListener locListener;
		private string path;

		public event EventHandler<LocationChangedEventArgs> LocationChanged;
		
		public MainApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			path = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Java.IO.File.Separator + "WF.Player";

			Directory.CreateDirectory (path);

			if (!Directory.Exists (path))
			{
				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetTitle (GetString (Resource.String.main_error));
				builder.SetMessage(String.Format(GetString(Resource.String.main_error_directory_not_found),path));
				builder.SetCancelable (true);
				builder.SetNeutralButton(Resource.String.ok,(obj,arg) => { });
				builder.Show ();
			}

			// Create LocListener
			locListener = new LocListener (GetSystemService (Context.LocationService) as LocationManager);
			locListener.LocationChanged += OnLocationChanged;
		}

		public Activity ActiveActivity {
			get {
				return activeActivity;
			}
			set {
				if (activeActivity != value) {
					activeActivity = value;
				}
			}
		}

		public Cartridges Cartridges {
			get {
				return cartridges;
			}
			set {
				cartridges = value;
			}
		}

		public string Path {
			get {
				return path;
			}
		}

		private void OnLocationChanged(Object sender, LocationChangedEventArgs e)
		{
			if (LocationChanged != null)
				LocationChanged (this, e);

		}
	}
}

