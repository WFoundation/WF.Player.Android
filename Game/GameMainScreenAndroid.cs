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
using Android.Support.V4.View;
using Android.Support.V7.App;
using Vernacular;
using WF.Player.Core;
using WF.Player.Core.Engines;
using WF.Player.Location;

namespace WF.Player.Game
{
	/// <summary>
	/// Screen main fragment.
	/// </summary>
	public partial class GameMainScreen : global::Android.Support.V4.App.Fragment
	{
		TextView _textLatitude;
		TextView _textLongitude;
		TextView _textAltitude;
		TextView _textAccuracy;
		LinearLayout _layoutBottom;
		ListView _listView;
		IMenuItem menuSave;
		IMenuItem menuQuit;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.Android.ScreenMain"/> class.
		/// </summary>
		/// <param name="engine">Engine.</param>
		public GameMainScreen(Engine engine)
		{
			this.engine = engine;
		}

		#endregion

		#region Android Event Handlers

		/// <summary>
		/// Raised, when this fragment is created.
		/// </summary>
		/// <param name="bundle">Bundle with cartridge and restore flag.</param>
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// This fragment has a option menu (Save/Quit)
			this.HasOptionsMenu = true;
		}

		/// <summary>
		/// Raised, when option menu for this fragment should be created.
		/// </summary>
		/// <param name="menu">Menu.</param>
		/// <param name="inflater">Inflater.</param>
		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate (Resource.Menu.GameMainScreenMenu, menu);

			menuSave = menu.FindItem (Resource.Id.menu_screen_main_save);
			menuQuit = menu.FindItem (Resource.Id.menu_screen_main_quit);

			base.OnCreateOptionsMenu(menu, inflater);
		}

		/// <summary>
		/// Raised, when view of this fragment should be created.
		/// </summary>
		/// <param name="bundle">Bundle with cartridge and restore flag.</param>
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((GameController)this.Activity);

			// Set images for icons
			_iconLocation = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_locations);
			_iconYouSee = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_yousee);
			_iconInventory = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_inventory);
			_iconTask = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_tasks);
			_iconPosition = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_position);

			// Load layout
			var view = inflater.Inflate(Resource.Layout.GameMainScreen, container, false);

			// Don't know a better way :(
			_layoutBottom = view.FindViewById<LinearLayout> (Resource.Id.layoutBottom);
			_layoutBottom.SetBackgroundResource(Main.BottomBackground);

			// Get views
			_textLatitude = view.FindViewById<TextView>(Resource.Id.textLatitude);
			_textLongitude = view.FindViewById<TextView>(Resource.Id.textLongitude);
			_textAltitude = view.FindViewById<TextView>(Resource.Id.textAltitude);
			_textAccuracy = view.FindViewById<TextView>(Resource.Id.textAccuracy);

			// Create list adapter and list events
			_listView = view.FindViewById<ListView>(Resource.Id.listView);
			_listView.Adapter = new GameMainScreenAdapter(this, ctrl);
			_listView.ItemClick += OnItemClick;

			RefreshLocation();

			return view;
		}

		/// <summary>
		/// Raised, when one of the list entries is selected.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			ctrl.Feedback();

			EntrySelected(e.Position);
		}

		/// <summary>
		/// Raised, when an entry of the options menu is selected.
		/// </summary>
		/// <param name="item">Item, which is selected.</param>
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			ctrl.Feedback();

			if (item.ItemId == menuSave.ItemId) {
				ctrl.Save();
				return false;
			}
			if (item.ItemId == menuQuit.ItemId) {
				ctrl.Quit();
				return false;
			}
			return base.OnOptionsItemSelected(item);
		}

		/// <summary>
		/// Raised, when this fragment gets focus.
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			// Show title bar with the correct buttons
			((ActionBarActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(false);
			((ActionBarActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);
			((ActionBarActivity)Activity).SupportActionBar.Title = engine.Cartridge.Name;

			CommonResume();
		}

		/// <summary>
		/// Raised, when this fragment stops.
		/// </summary>
		public override void OnStop()
		{
			CommonPause();

			base.OnStop();
		}

		#endregion

		#region Private Functions

		/// <summary>
		/// Raised, when the screen should be updated.
		/// </summary>
		void Refresh()
		{
			if (_listView != null)
				((GameMainScreenAdapter)_listView.Adapter).NotifyDataSetChanged();
		}

		/// <summary>
		/// Raised, when the screen should be updated.
		/// </summary>
		void RefreshLocation()
		{
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
					_textAltitude.Text = altitude + " Hm";
				} else {
					_textAltitude.Visibility = ViewStates.Invisible;
				}
				_textAccuracy.Visibility = ViewStates.Visible;
				_textAccuracy.Text = accuracy + " Ac";
			} else {
				_textLatitude.Visibility = ViewStates.Gone;
				_textLongitude.Visibility = ViewStates.Gone;
				_textAltitude.Visibility = ViewStates.Gone;
				_textAccuracy.Visibility = ViewStates.Gone;
			}
		}

		#endregion

    }

	/// <summary>
	/// Screen main adapter.
	/// </summary>
	public class GameMainScreenAdapter : BaseAdapter
	{
		GameController _ctrl;
		GameMainScreen _owner;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.Android.ScreenMainAdapter"/> class.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="engine">Engine.</param>
		public GameMainScreenAdapter(GameMainScreen owner, GameController ctrl) : base()
		{
			_ctrl = ctrl;
			_owner = owner;
		}

		#endregion

		#region Members

		/// <summary>
		/// Gets the number of entries in list.
		/// </summary>
		/// <value>The count.</value>
		public override int Count
		{
			get 
			{ 
				return 4; 
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the item at given position.
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="position">Position.</param>
		public override Java.Lang.Object GetItem(int position)
		{
			return position;
		}

		/// <summary>
		/// Gets the item at position.
		/// </summary>
		/// <returns>The item at position.</returns>
		/// <param name="position">Position.</param>
		public int GetItemAtPosition(int position)
		{
			return position;
		}

		/// <summary>
		/// Gets the item id.
		/// </summary>
		/// <returns>The item identifier.</returns>
		/// <param name="position">Position.</param>
		public override long GetItemId(int position)
		{
			return position;
		}

		/// <summary>
		/// Gets the view for this position.
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="position">Position.</param>
		/// <param name="convertView">Convert view.</param>
		/// <param name="parent">Parent.</param>
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			// This gives us some performance gains by not always inflating a new view
			// This will sound familiar to MonoTouch developers with UITableViewCell.DequeueReusableCell()
			var view = (convertView ?? _ctrl.LayoutInflater.Inflate(Resource.Layout.GameMainScreenItem, parent, false)) as RelativeLayout;

			view.SetMinimumHeight(_ctrl.FindViewById<ListView>(Resource.Id.listView).Height % Count);

			string header;
			string items;
			object image;

			_owner.GetContentEntry(position, out header, out items, out image);

			using (var textHeader = view.FindViewById<TextView>(Resource.Id.textHeader)) {
				textHeader.SetText(header, TextView.BufferType.Normal);
			}

			using (var textItems = view.FindViewById<TextView>(Resource.Id.textItems)) {
				textItems.SetText(items, TextView.BufferType.Normal);
			}

			using (var imageIcon = view.FindViewById<ImageView>(Resource.Id.imageIcon)) {
				if (image != null) {
					imageIcon.SetImageBitmap(null);
					imageIcon.SetImageBitmap((Bitmap)image);
				}
			}

			// Finally return the view
			return view;
		}

		#endregion

	}

}
