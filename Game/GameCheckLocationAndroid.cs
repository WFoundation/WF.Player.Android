///
/// WF.Player.Android - A Wherigo Player for Android, which use the Wherigo Foundation Core.
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
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using WF.Player.Location;
using WF.Player.Types;
using WF.Player.Views;

namespace WF.Player.Game
{
	/// <summary>
	/// Screen check location: Android part.
	/// </summary>
	/// <remarks>
	/// This part handles all things which are in Android and iOS common
	/// </remarks>
	public partial class GameCheckLocation : global::Android.Support.V4.App.Fragment
	{
		#region Android Events

		/// <summary>
		/// Raises the create view event.
		/// </summary>
		/// <param name="inflater">Inflater.</param>
		/// <param name="container">Container.</param>
		/// <Docs>The LayoutInflater object that can be used to inflate
		///  any views in the fragment,</Docs>
		/// <param name="savedInstanceState">If non-null, this fragment is being re-constructed
		///  from a previous saved state as given here.</param>
		/// <returns>The view to be added.</returns>
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((GameController)this.Activity);

			if (container == null)
				return null;

			var view = inflater.Inflate(Resource.Layout.CheckLocation, container, false);

			textDescription = new TextViewAndroid(view.FindViewById<TextView> (Resource.Id.textDescription));
			textCoordinates = new TextViewAndroid(view.FindViewById<TextView> (Resource.Id.textCoords));
			textAccuracy = new TextViewAndroid(view.FindViewById<TextView> (Resource.Id.textAccuracy));
			button = new ButtonViewAndroid(view.FindViewById<Button> (Resource.Id.buttonStart));

			CommonCreate();

			return view;
		}

		/// <summary>
		/// Raises the resume event.
		/// </summary>
		/// <Docs>Called when the fragment is visible to the user and actively running.</Docs>
		public override void OnResume()
		{
			base.OnResume();

			CommonResume();
		}

		/// <summary>
		/// Raises the pause event.
		/// </summary>
		/// <Docs>Called when the Fragment is no longer visible.</Docs>
		public override void OnPause()
		{
			base.OnPause ();

			CommonPause();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Refresh this view.
		/// </summary>
		void Refresh()
		{
			CommonRefresh();
		}

		/// <summary>
		/// Sets the title of the screen.
		/// </summary>
		/// <param name="title">Title.</param>
		void SetTitle(string title)
		{
			((GameController)Activity).SupportActionBar.Title = title;
		}

		/// <summary>
		/// Makes the button green.
		/// </summary>
		void MakeButtonGreen ()
		{
			((ButtonViewAndroid)button).View.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light_green);
		}

		/// <summary>
		/// Makes the button red.
		/// </summary>
		void MakeButtonRed ()
		{
			((ButtonViewAndroid)button).View.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light_red);
		}

		#endregion
	}
}

