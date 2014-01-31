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
using Vernacular;
using Com.TestFlightApp.Lib;
using WF.Player.Core;
using WF.Player.Core.Live;

namespace WF.Player.Android
{
	[Application(Debuggable=true)]
	public class MainApp : Application
	{
		static MainApp instance;

		Activity activeActivity;
		ISharedPreferences preferences;
		Cartridges cartridges;
		LocListener locListener;
		string path;

		public MainApp(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
			// Save instance for later use
			instance = this;

			TestFlight.TakeOff(this, "0596e62a-e3cb-4107-8d05-96fa7ae0c26a");

			// Catch unhandled exceptions
			// Found at http://xandroid4net.blogspot.de/2013/11/how-to-capture-unhandled-exceptions.html
			// Add an exception handler for all uncaught exceptions.
			AndroidEnvironment.UnhandledExceptionRaiser += AndroidUnhandledExceptionHandler;
			AppDomain.CurrentDomain.UnhandledException += ApplicationUnhandledExceptionHandler;

			preferences = Application.Context.GetSharedPreferences("WF.Player.preferences", FileCreationMode.MultiProcess);

			path = preferences.GetString("path", "");

			if (String.IsNullOrEmpty(path))
				path = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Java.IO.File.Separator + "WF.Player";

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
				preferences.Edit().PutString("path", path).Commit();
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

		public static MainApp Instance
		{
			get { return instance; }
		}

		public string Path {
			get {
				return path;
			}
		}

		public LocListener GPS {
			get { 
				return locListener;
			}
			set {
				locListener = value;
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
			//				
			// When the UI Thread crashes this is the code that will be executed. There is no context at this point
			// and no way to recover from the exception. This is where you would capture the error and log it to a
			// file for example. You might be able to post to a web handler, I have not tried that.
			//
			// You can access the information about the exception in the args.Exception object.
			//
	
			// Send the Exception to TestFlight.
			TestFlight.SendCrash(e.Exception);

			throw e.Exception;
		}

		void ApplicationUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			//				
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
			// the a different activity is brought to the foreground.
			//

			// Send the Exception to TestFlight.
			TestFlight.SendCrash(e.ToString());

			throw new System.Exception(e.ToString());
		}

		#endregion
	}
}

