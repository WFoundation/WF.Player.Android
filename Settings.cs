using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WF.Player.Android
{
	[Activity (Label = "Settings")]			
	public class Settings : PreferenceActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
			AddPreferencesFromResource(Resource.Layout.Settings);
		}
	}
}

