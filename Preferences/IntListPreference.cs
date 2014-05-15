///
/// ListPreference for Int values.
/// Based on an articel of Kevin Vance (http://kvance.livejournal.com/1039349.html)
///

using System;
using Android.Content;
using Android.Preferences;
using Android.Util;

namespace WF.Player.Preferences
{
	public class IntListPreference : ListPreference
	{
		static ISharedPreferences _sharedPreferences;
		static ISharedPreferencesEditor _sharedPreferencesEditor;

		public IntListPreference(Context context, IAttributeSet attrs) : base(context, attrs) 
		{
			_sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
			_sharedPreferencesEditor = _sharedPreferences.Edit();
		}

		public IntListPreference(Context context) : base(context)
		{
		}

		protected override bool PersistString(String value) 
		{
			if(value == null) {
				return false;
			} else {
				return _sharedPreferencesEditor.PutInt(Key, Convert.ToInt32(value)).Commit();
			}
		}

		protected override string GetPersistedString(string defaultReturnValue) 
		{
			if(_sharedPreferences.Contains(Key)) {
				// _sharedPreferences.Edit().Remove(Key).Commit();
				int intValue = _sharedPreferences.GetInt(Key, Convert.ToInt32(defaultReturnValue));
				return intValue.ToString();
			} else {
				return defaultReturnValue;
			}
		}
	} 
}

