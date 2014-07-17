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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using WF.Player.Core;
using WF.Player.Core.Engines;
using WF.Player.Location;
using WF.Player.Types;
using Android.Util;
using Vernacular;

namespace WF.Player.Game
{
	#region GameListScreen

	public partial class GameListScreen : global::Android.Support.V4.App.Fragment
	{
		TextView _textLatitude;
		TextView _textLongitude;
		TextView _textAltitude;
		TextView _textAccuracy;
		ImageView _imageAltitude;
		ImageView _imageAccuracy;
		RelativeLayout _layoutBottom;
		ListView _listView;
		IMenuItem menuMap;
		int _lastAzimuth;

		#region Constructor

		public GameListScreen(Engine engine, ScreenTypes screen)
		{
			this.type = screen;
			this.engine = engine;

			_refresh = new CallDelayer(100, 500, (obj) => Refresh());
		}

		#endregion

		#region Android Event Handlers

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((GameController)this.Activity);

			if (container == null)
				return null;

			var view = inflater.Inflate(Resource.Layout.GameListScreen, container, false);

			// Don't know a better way :(
			_layoutBottom = view.FindViewById<RelativeLayout> (Resource.Id.layoutBottom);
			_layoutBottom.SetBackgroundResource(Main.BottomBackground);

			// Get views
			_textLatitude = view.FindViewById<TextView>(Resource.Id.textLatitude);
			_textLongitude = view.FindViewById<TextView>(Resource.Id.textLongitude);
			_textAltitude = view.FindViewById<TextView>(Resource.Id.textAltitude);
			_textAccuracy = view.FindViewById<TextView>(Resource.Id.textAccuracy);
			_imageAltitude = view.FindViewById<ImageView>(Resource.Id.imageAltitude);
			_imageAccuracy = view.FindViewById<ImageView>(Resource.Id.imageAccuracy);

			// Create list adapter and list events
			_listView = view.FindViewById<ListView> (Resource.Id.listView);
			_listView.Adapter = new GameListScreenAdapter (this, ctrl, type);
			_listView.ItemClick += OnItemClick;
			_listView.Recycler += OnRecycling;

			ctrl.SupportActionBar.Title = GetContent ();

			HasOptionsMenu = (type == ScreenTypes.Locations || type == ScreenTypes.Items);

			_refresh.Abort();

			RefreshLocation();

			return view;
		}

		/// <summary>
		/// Raised when the fragment is destroyed, so free references to other UI elements.
		/// </summary>
//		public override void OnDestroyView()
//		{
//			base.OnDestroyView();
//
//			Items = null;
//			listView = null;
//			ctrl = null;
//			engine = null;
//		}

		public void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			// Update ListView anymore 
			_refresh.Abort();
			// Send feedback to user, if it is selected
			ctrl.Feedback();

			EntrySelected(e.Position);
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater) 
		{
			inflater.Inflate (Resource.Menu.GameListScreenMenu, menu);

			menuMap = menu.FindItem (Resource.Id.menu_screen_list_map);

			if (type == ScreenTypes.Locations || type == ScreenTypes.Items) {
				menuMap.SetVisible (true);
			} else {
				menuMap.SetVisible(false);
			}

			base.OnCreateOptionsMenu(menu, inflater);
		}

		/// <summary>
		/// Raised, when an entry of the options menu is selected.
		/// </summary>
		/// <param name="item">Item, which is selected.</param>
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			ctrl.Feedback();

			switch (item.ItemId) {
				case Resource.Id.menu_screen_list_map:
					ctrl.ShowScreen(ScreenTypes.Map, null);
					return false;
			}

			return base.OnOptionsItemSelected(item);
		}

		/// <summary>
		/// Raised when a list entry is recycled.
		/// </summary>
		/// <remarks>
		/// We had to free bitmap resource to not get a out of memory error.
		/// </remarks>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void OnRecycling (object sender, AbsListView.RecyclerEventArgs e)
		{
			ImageView iv = e.View.FindViewById<ImageView>(Resource.Id.imageIcon);
			// if there is a image view, than release the memory
			if (iv != null)
				iv.SetImageBitmap(null);
			iv = e.View.FindViewById<ImageView> (Resource.Id.imageDirection);
			if (iv != null)
				iv.SetImageBitmap(null);
		}

		public override void OnResume()
		{
			base.OnResume();

			ctrl.SupportActionBar.SetDisplayHomeAsUpEnabled (true);
			ctrl.SupportActionBar.SetDisplayShowHomeEnabled(true);

			ctrl.SupportActionBar.Title = GetContent ();

			StartEvents();

			if (type == ScreenTypes.Locations || type == ScreenTypes.Items)
				Main.GPS.AddOrientationListener(OnOrientationChanged);

			Refresh();
		}

		public override void OnPause()
		{
			StopEvents();

			if (type == ScreenTypes.Locations || type == ScreenTypes.Items)
				Main.GPS.RemoveOrientationListener(OnOrientationChanged);

			base.OnStop();
		}

		void OnOrientationChanged (object sender, OrientationChangedEventArgs e)
		{
			if (ShowDirections && Math.Abs(_lastAzimuth - e.Azimuth) > 2) {
				_lastAzimuth = (int)e.Azimuth;
				_refresh.Call();
			}
		}

		#endregion

		#region Private Functions

		void Refresh()
		{
			if (Activity == null)
				return;

			Activity.RunOnUiThread(() => {
				if (Activity == null)
					return;
				((ActionBarActivity)Activity).SupportActionBar.Title = GetContent ();
				if (_listView != null) {
					((GameListScreenAdapter)_listView.Adapter).NotifyDataSetChanged();
				}
			});
		}

		/// <summary>
		/// Raised, when the screen should be updated.
		/// </summary>
		void RefreshLocation()
		{
			if(Activity == null)
				return;

			Activity.RunOnUiThread(() => {
				if (Activity == null)
					return;
				var gps = Main.GPS;
				var location = gps.IsValid ? gps.Location.ToString() : Catalog.GetString("Unknown");
				var altitude = gps.Location.HasAltitude ? String.Format("{0:0} m", gps.Location.Altitude) : GetString(Resource.String.unknown);
				var accuracy = gps.Location.HasAccuracy ? String.Format("{0:0} m", gps.Location.Accuracy) : Strings.Infinite;
				var status = gps.IsValid ? Catalog.GetString("valid") : Catalog.GetString("invalid");

				if(gps.IsValid) {
					_textLatitude.Visibility = ViewStates.Visible;
					_textLatitude.Text = location.Split(location.Contains("W") ? 'W' : 'E')[0];
					_textLongitude.Visibility = ViewStates.Visible;
					_textLongitude.Text = location.Substring(location.IndexOf(location.Contains("W") ? "W" : "E"));
					if(gps.Location.HasAltitude) {
						_textAltitude.Visibility = ViewStates.Visible;
						_textAltitude.Text = altitude;
						_imageAltitude.Visibility = ViewStates.Visible;
					} else {
						_textAltitude.Visibility = ViewStates.Invisible;
						_imageAltitude.Visibility = ViewStates.Invisible;
					}
					_textAccuracy.Visibility = ViewStates.Visible;
					_textAccuracy.Text = accuracy;
					_imageAccuracy.Visibility = ViewStates.Visible;
				} else {
					_textLatitude.Visibility = ViewStates.Gone;
					_textLongitude.Visibility = ViewStates.Gone;
					_textAltitude.Visibility = ViewStates.Gone;
					_textAccuracy.Visibility = ViewStates.Gone;
					_imageAltitude.Visibility = ViewStates.Gone;
					_imageAccuracy.Visibility = ViewStates.Gone;
				}
			});
		}

		#endregion
	}

	#endregion

	#region ScreenListAdapter

	public class GameListScreenAdapter : BaseAdapter
	{
		GameController ctrl;
		GameListScreen owner;
		ScreenTypes screen;
		int _directionSize;

		#region Constructor

		public GameListScreenAdapter(GameListScreen owner, GameController ctrl, ScreenTypes screen) : base()
		{
			this.ctrl = ctrl;
			this.owner = owner;
			this.screen = screen;

			_directionSize = (int)(ctrl.Resources.DisplayMetrics.Density * 32.0);

		}

		#endregion

		public override int Count
		{
			get
			{
				return owner.Items.Count;
			}
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return position;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			// This gives us some performance gains by not always inflating a new view
			// This will sound familiar to MonoTouch developers with UITableViewCell.DequeueReusableCell()
			var view = (convertView ?? ctrl.LayoutInflater.Inflate(Resource.Layout.GameListScreenItem, parent, false)) as RelativeLayout;

			var layout = view.FindViewById<LinearLayout>(Resource.Id.linearLayoutItemText);
			var imageIcon = view.FindViewById<ImageView>(Resource.Id.imageIcon);
			var textHeader = view.FindViewById<TextView>(Resource.Id.textHeader);

			if (!owner.ShowDirections) {
				var layoutParams = (RelativeLayout.LayoutParams)layout.LayoutParameters;
				layoutParams.RightMargin = 0;
				layout.LayoutParameters = layoutParams;
			}

			Bitmap bm = null;

			try {
				if (owner.ShowIcons) {
					//					imageIcon.SetImageBitmap(null);
					if (owner.Items[position].Icon != null) {
						bm = ctrl.ConvertMediaToBitmap(owner.Items[position].Icon,32);
					} else {
						bm = Images.IconEmpty;
					}
					imageIcon.SetImageBitmap (bm);
					imageIcon.Visibility = ViewStates.Visible;
				} else {
					imageIcon.Visibility = ViewStates.Gone;
				}
			} finally {
				if (bm != null) {
					bm.Dispose();
					bm = null;
				}
			}

			string name = owner.Items[position].Name == null ? "" : owner.Items[position].Name;
			if (owner.Items[position] is Task)
				textHeader.Text = (((Task)owner.Items[position]).Complete ? (((Task)owner.Items[position]).CorrectState == TaskCorrectness.NotCorrect ? Strings.TaskNotCorrect : Strings.TaskCorrect) + " " : "") + name;
			else
				textHeader.Text = name;

			using (var textDistance = view.FindViewById<TextView> (Resource.Id.textDistance)) {
				using(var imageDirection = view.FindViewById<ImageView> (Resource.Id.imageDirection)) {

					if (screen == ScreenTypes.Locations || screen == ScreenTypes.Items) {
						if (((Thing)owner.Items[position]).VectorFromPlayer != null) {
							textDistance.Visibility = ViewStates.Visible;
							imageDirection.Visibility = ViewStates.Visible;
							textDistance.Text = ((Thing)owner.Items[position]).VectorFromPlayer.Distance.BestMeasureAs(DistanceUnit.Meters);
							if (((Thing)owner.Items[position]).VectorFromPlayer.Distance.Value == 0) {
								imageDirection.SetImageBitmap (BitmapFactory.DecodeResource(owner.Activity.Resources, Resource.Drawable.ic_direction_position));
							} else {
								imageDirection.SetImageBitmap(BitmapArrow.Draw(_directionSize, ((Thing)owner.Items[position]).VectorFromPlayer.Bearing.Value + Main.GPS.Bearing));
//								AsyncImageFromDirection.LoadBitmap(imageDirection, ((Thing)owner.Items[position]).VectorFromPlayer.Bearing.Value + Main.GPS.Bearing, imageDirection.Width, imageDirection.Height);
							}
						} else {
							textDistance.Visibility = ViewStates.Gone;
							imageDirection.Visibility = ViewStates.Gone;
						}
					} else {
						textDistance.Visibility = ViewStates.Gone;
						imageDirection.Visibility = ViewStates.Gone;
					}
				}
			}

			// Finally return the view
			return view;
		}

		public int GetItemAtPosition(int position)
		{
			return position;
		}
	}

	#endregion
}
