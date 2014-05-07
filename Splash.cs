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
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WF.Player.Preferences;

namespace WF.Player
{
	[Activity(MainLauncher = true, NoHistory = true, Label = "WF.Player", Theme="@style/Theme.Splash")]			
	public class Splash : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here

			// Display splash screen on whole screen
			this.RequestWindowFeature(WindowFeatures.NoTitle);

			// Save prefernces instance
			Main.Prefs = new PreferenceValues(PreferenceManager.GetDefaultSharedPreferences(this));

			string path = Main.Prefs.GetString("path", null);

			if (String.IsNullOrEmpty(path))
				path = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Java.IO.File.Separator + "WF.Player";

			try {
				if (!Directory.Exists (path))
					Directory.CreateDirectory (path);
			}
			catch {
			}

			Main.Path = path;

			// Show splash screen
			StartActivity(typeof(MainActivity));
		}
	}
}

