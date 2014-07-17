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
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Vernacular;
//using Com.TestFlightApp.Lib;
using WF.Player.Core;
using WF.Player.Core.Live;
using WF.Player.Location;
using WF.Player.Preferences;
using Android.Hardware;

namespace WF.Player
{
	[Application(Debuggable=true)]
	public class MainApp : Application
	{
		static Cartridges cartridges;

		public MainApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			// Catch unhandled exceptions
			// Found at http://xandroid4net.blogspot.de/2013/11/how-to-capture-unhandled-exceptions.html
			// Add an exception handler for all uncaught exceptions.
			AndroidEnvironment.UnhandledExceptionRaiser += AndroidUnhandledExceptionHandler;
			AppDomain.CurrentDomain.UnhandledException += ApplicationUnhandledExceptionHandler;

			// Save prefernces instance
			Main.Prefs = new PreferenceValues(PreferenceManager.GetDefaultSharedPreferences(this));

			// Get path from preferences or default path
			string path = Main.Prefs.GetString("filepath", Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "WF.Player"));

			try {
				if (!Directory.Exists (path))
					Directory.CreateDirectory (path);
			}
			catch {
			}

			if (!Directory.Exists (path))
			{
				AlertDialog.Builder builder = new AlertDialog.Builder (this);
				builder.SetTitle (GetString (Resource.String.main_error));
				builder.SetMessage(String.Format(GetString(Resource.String.main_error_directory_not_found), path));
				builder.SetCancelable (true);
				builder.SetNeutralButton(Resource.String.ok,(obj,arg) => { });
				builder.Show ();
			} else {
				Main.Path = path;
				Main.Prefs.SetString("filepath", path);
			}
		}

		public static Cartridges Cartridges {
			get {
				return cartridges;
			}
			set {
				cartridges = value;
			}
		}

		#region Events

		protected override void Dispose(bool disposing)
		{
			// Remove the exception handler.
			AndroidEnvironment.UnhandledExceptionRaiser -= AndroidUnhandledExceptionHandler;
			AppDomain.CurrentDomain.UnhandledException -= ApplicationUnhandledExceptionHandler;

			base.Dispose(disposing);
		}

		void AndroidUnhandledExceptionHandler(object sender, RaiseThrowableEventArgs e)
		{
			// When the UI Thread crashes this is the code that will be executed. There is no context at this point
			// and no way to recover from the exception. This is where you would capture the error and log it to a
			// file for example. You might be able to post to a web handler, I have not tried that.
			//
			// You can access the information about the exception in the args.Exception object.
	
			// Send the Exception to HockeyApp
			HockeyApp.ManagedExceptionHandler.SaveException (e.Exception);
		}

		void ApplicationUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			// When a background thread crashes this is the code that will be executed. You can
			// recover from this.
			// You might for example:
			//  _CurrentActivity.RunOnUiThread(() => Toast.MakeText(_CurrentActivity, "Unhadled Exception was thrown", ToastLength.Short).Show());
			// 
			// or
			//
			// _CurrentActivity.StartActivity(typeof(SomeClass));
			// _CurrentActivity.Finish();
			//
			// It is up to the developer as to what he/she wants to do here.
			//
			// If you are requiring a minimum version less than API 14, you would have to set _CurrentActivity in each time
			// a different activity is brought to the foreground.

			// Send the Exception to TestFlight.
			//			TestFlight.SendCrash(e.ToString());

			throw new System.Exception(e.ToString());
		}

		#endregion
	}
}

