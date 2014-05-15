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
	public class FloatListPreference : ListPreference
	{
		static ISharedPreferences _sharedPreferences;
		static ISharedPreferencesEditor _sharedPreferencesEditor;

		public FloatListPreference(Context context, IAttributeSet attrs) : base(context, attrs) 
		{
			_sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
			_sharedPreferencesEditor = _sharedPreferences.Edit();
		}

		public FloatListPreference(Context context) : base(context)
		{
		}

		protected override bool PersistString(String value) 
		{
			if(value == null) {
				return false;
			} else {
				return _sharedPreferencesEditor.PutFloat(Key, Convert.ToSingle(value)).Commit();
			}
		}

		protected override string GetPersistedString(string defaultReturnValue) 
		{
			if(_sharedPreferences.Contains(Key)) {
				// _sharedPreferences.Edit().Remove(Key).Commit();
				float floatValue = _sharedPreferences.GetFloat(Key, Convert.ToSingle(defaultReturnValue));
				return floatValue.ToString();
			} else {
				return defaultReturnValue;
			}
		}
	} 
}

