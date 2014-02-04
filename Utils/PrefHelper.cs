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
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WF.Player.Android
{
	class PrefHelper
	{
		static private ISharedPreferences preferences;

		public PrefHelper(ISharedPreferences p)
		{
			preferences = p;
		}

		public static ISharedPreferences Preferences
		{
			get { return preferences; }
			set { preferences = value; }
		}

		public static bool FeedbackSound 
		{
			get { return preferences.GetBoolean("feedback_sound", false); }
			set { preferences.Edit().PutBoolean("feedback_sound",value).Commit();
			}
		}

		public static bool FeedbackVibration 
		{
			get { return preferences.GetBoolean("feedback_vibration", false); }
			set { preferences.Edit().PutBoolean("feedback_vibration",value).Commit();
			}
		}

		public static GravityFlags TextAlignment 
		{
			get { 
				string align = preferences.GetString("text_alignment", "1");

				if (align.Equals("0"))
					return GravityFlags.Left;
				if (align.Equals("2"))
					return GravityFlags.Right;

				return GravityFlags.CenterHorizontal;
			 }
			set {
				if ((value & GravityFlags.Left) == GravityFlags.Left)
					preferences.Edit().PutString("text_alignment","0").Commit();
				if ((value & GravityFlags.CenterHorizontal) == GravityFlags.CenterHorizontal)
					preferences.Edit().PutString("text_alignment","1").Commit();
				if ((value & GravityFlags.Right) == GravityFlags.Right)
					preferences.Edit().PutString("text_alignment","2").Commit();
			}
		}

		public static ImageResize ImageResize
		{
			get { return (ImageResize)Convert.ToInt32(preferences.GetString("image_size", "1")); }
		}

		public static bool InputFocus 
		{
			get { return preferences.GetBoolean("input_focus", false); }
			set { preferences.Edit().PutBoolean("input_focus",value).Commit();
			}
		}

		public static int TextSize
		{
			get { 
				return Convert.ToInt32(preferences.GetString("text_size", "14"));
			}
			set {
				preferences.Edit().PutString("text_size",value.ToString()).Commit();
			}
		}

	}
}

