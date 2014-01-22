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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.Android
{
	#region ScreenMap

	public class ScreenMap : global::Android.Support.V4.App.Fragment
	{
		ScreenController ctrl;
		UIObject activeObject;

		#region Constructor

		public ScreenMap(ScreenController ctrl, UIObject obj)
		{
			this.ctrl = ctrl;
			this.activeObject = obj;
			if (this.activeObject != null) {
				this.activeObject.PropertyChanged += OnPropertyChanged;
			}
		}

		#endregion

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((ScreenController)this.Activity);

			var view = inflater.Inflate(Resource.Layout.ScreenMap, container, false);

			LinearLayout layoutMap = view.FindViewById<LinearLayout> (Resource.Id.layoutMap);

			var text = new TextView(this.Activity);
			text.Text = "Map";

			layoutMap.AddView(text);

			return view;
		}

		void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}
	}

	#endregion

}

