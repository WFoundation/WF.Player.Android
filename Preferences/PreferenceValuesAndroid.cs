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

namespace WF.Player.Preferences
{
	partial class PreferenceValues
	{
		ISharedPreferences _preferences;

		#region Constructor

		public PreferenceValues(ISharedPreferences p)
		{
			_preferences = p;
		}

		#endregion

		#region Methods

		public bool GetBool(string key, bool b)
		{
			return _preferences.GetBoolean(key, b);
		}

		public void SetBool(string key, bool b)
		{
			_preferences.Edit().PutBoolean(key, b).Commit();
		}

		public string GetString(string key, string s)
		{
			return _preferences.GetString(key, s);
		}

		public void SetString(string key, string s)
		{
			_preferences.Edit().PutString(key, s).Commit();
		}

		public int GetInt(string key, int i)
		{
			return _preferences.GetInt(key, i);
		}

		public void SetInt(string key, int i)
		{
			_preferences.Edit().PutInt(key, i).Commit();
		}

		public double GetDouble(string key, double d)
		{
			return _preferences.GetFloat(key, (float)d);
		}

		public void SetDouble(string key, double d)
		{
			_preferences.Edit().PutFloat(key, (float)d).Commit();
		}

		#endregion
	}
}

