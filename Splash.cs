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
using Vernacular;
using Android.Content.PM;
using WF.Player.Core.Live;
using System.Threading.Tasks;

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

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Splash);

			var textSubHeader = FindViewById<TextView> (Resource.Id.textSubHeader);
			var textVersion = FindViewById<TextView> (Resource.Id.textVersion);
			var textCopyright = FindViewById<TextView> (Resource.Id.textCopyright);

			PackageInfo pInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
			string version = String.Format("{0}.{1}", pInfo.VersionName, pInfo.VersionCode);

			textSubHeader.Text = String.Format(Catalog.GetString(this.Resources.GetString(Resource.String.splash_subheader)), version);
			textVersion.Text = String.Format(Catalog.GetString(this.Resources.GetString(Resource.String.splash_version)), version);
			textCopyright.Text = Catalog.GetString(this.Resources.GetString(Resource.String.splash_copyright));

			LoadCartridges();
		}

		async void LoadCartridges()
		{
			await Task<bool>.Run(() => 
				{
					// Create Vernacular catalog for translation
					Catalog.Implementation = new Vernacular.AndroidCatalog (Resources, typeof (Resource.String));

					//
					// TODO: Show MainActivity instead of local file list
					//
					Cartridges carts = new Cartridges();
					List<string> fileList = new List<string>();
					FileInfo[] files = null;
					try {
						// Read all GWC, GWZ, WFC and WFZ from default directory
						files = new DirectoryInfo(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + Java.IO.File.Separator + "WF.Player").GetFiles("*");
					}
					catch(Exception e) {
						AlertDialog.Builder builder = new AlertDialog.Builder(this);
						builder.SetTitle(GetString(Resource.String.main_error));
						builder.SetMessage(e.Message);
						builder.SetCancelable(true);
						builder.SetNeutralButton(Resource.String.ok, (obj, arg) =>  {
						});
						builder.Show();
						return;
					}
					foreach(FileInfo fi in files) {
						string ext = Path.GetExtension(fi.Name).ToUpper();
						if(ext.Equals(".GWC") || ext.Equals(".GWZ") || ext.Equals("WFC") || ext.Equals("WFZ"))
							fileList.Add(fi.FullName);
					}
					if(fileList.Count == 0) {
						AlertDialog.Builder builder = new AlertDialog.Builder(this);
						builder.SetTitle(GetString(Resource.String.main_error));
						builder.SetMessage(String.Format(GetString(Resource.String.main_error_no_cartridges), Main.Path));
						builder.SetCancelable(true);
						builder.SetNeutralButton(Resource.String.ok, (obj, arg) =>  {
						});
						builder.Show();
					}

					// Create CartridgesList
					carts.GetByFileList(fileList);
					MainApp.Cartridges = carts;

					// Show splash screen
					StartActivity(typeof(CartridgesActivity));
				});
		}
	}
}

