///
/// WF.Player - A Wherigo Player, which use the Wherigo Foundation Core.
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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WF.Player.Location;
using WF.Player.Preferences;

namespace WF.Player
{
	static class Main
	{
		static string _path;

		public static string Path
		{
			get { return _path; }
			set { _path = value; }
		}

		#region GPS

		static GPSListener _gps;

		public static GPSListener GPS
		{
			get { return _gps; }
			set { _gps = value; }
		}

		#endregion

		#region Preferences

		static PreferenceValues _prefs;

		public static PreferenceValues Prefs
		{
			get { return _prefs; }
			set { _prefs = value; }
		}

		#endregion

		#region Static Functions

		public static void SetTheme(Activity activity)
		{
			switch(Prefs.GetInt("theme", 1)) {
			case 0:
				BottomBackground = Resource.Drawable.ab_bottom_solid_darktheme;
				activity.SetTheme(Resource.Style.Theme_Darktheme);
				break;
			case 1:
				BottomBackground = Resource.Drawable.ab_bottom_solid_lighttheme;
				activity.SetTheme(Resource.Style.Theme_Lighttheme);
				break;
			default:
				BottomBackground = Resource.Drawable.ab_bottom_solid_lighttheme;
				activity.SetTheme(Resource.Style.Theme_Lighttheme);
				break;
			}
		}

		public static int BottomBackground; 

		public static int ButtonBackground = Resource.Drawable.apptheme_btn_default_holo_light; 

		#endregion
	}
}

