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
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.Android
{
	/// <summary>
	/// Screen main fragment.
	/// </summary>
	public partial class ScreenMain : global::Android.Support.V4.App.Fragment
	{
		ListView listView;
		IMenuItem menuSave;
		IMenuItem menuQuit;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.Android.ScreenMain"/> class.
		/// </summary>
		/// <param name="engine">Engine.</param>
		public ScreenMain(Engine engine)
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
			inflater.Inflate (Resource.Menu.ScreenMainMenu, menu);

			menuSave = menu.FindItem (Resource.Id.menu_screen_main_save);
			menuQuit = menu.FindItem (Resource.Id.menu_screen_main_quit);

			base.OnCreateOptionsMenu(menu, inflater);

//			menuSave = menu.Add(Resource.String.menu_screen_main_save);
//			menuSave.SetShowAsAction(ShowAsAction.Always);
//			menuSave.SetIcon(Resource.Drawable.menu_save);
//			menuQuit = menu.Add(Resource.String.menu_screen_main_quit);
//			menuQuit.SetShowAsAction(ShowAsAction.Always);
//			menuQuit.SetIcon(Resource.Drawable.lock_power_off);
//			//			ISubMenu sub = menu.AddSubMenu ("Theme");
//			//			sub.Add (0, Resource.Style.Theme_Sherlock, 0, "Default");
//			//			sub.Add (0, Resource.Style.Theme_Sherlock_Light, 0, "Light");
//			//			sub.Add (0, Resource.Style.Theme_Sherlock_Light_DarkActionBar, 0, "Light (Dark Action Bar)");
//			//			sub.Item.Show.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);
		}

		/// <summary>
		/// Raised, when view of this fragment should be created.
		/// </summary>
		/// <param name="bundle">Bundle with cartridge and restore flag.</param>
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((ScreenController)this.Activity);

			// Get context for this fragment
			Context context = Activity.ApplicationContext;

			if (container == null)
			{
				return null;
			}

			// Set images for icons
			iconLocation = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_locations);
			iconYouSee = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_yousee);
			iconInventory = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_inventory);
			iconTask = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_tasks);
			iconPosition = BitmapFactory.DecodeResource(ctrl.Resources, Resource.Drawable.ic_position);

			// Load layout
			var view = inflater.Inflate(Resource.Layout.ScreenMain, container, false);

			// Create list adapter and list events
			listView = view.FindViewById<ListView>(Resource.Id.listView);
			listView.Adapter = new ScreenMainAdapter(this, ctrl);
			listView.ItemClick += OnItemClick;

			return view;
		}

		/// <summary>
		/// Raised when the fragment is destroyed, so free references to other UI elements.
		/// </summary>
		public override void OnDestroyView()
		{
			base.OnDestroyView();

			iconLocation = null;
			iconYouSee = null;
			iconInventory = null;
			iconTask = null;
			iconPosition = null;
			menuQuit = null;
			menuSave = null;
			listView = null;
			ctrl = null;
			engine = null;
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
			return true;
		}

		/// <summary>
		/// Raised, when this fragment gets focus.
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			//TODO: Load data
			((ActionBarActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(false);
			((ActionBarActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);
			((ActionBarActivity)Activity).SupportActionBar.Title = engine.Cartridge.Name;

			StartEvents();

			MainApp.Instance.GPS.LocationChanged += OnLocationChanged;

			listView.Invalidate ();
		}

		/// <summary>
		/// Raised, when this fragment stops.
		/// </summary>
		public override void OnStop()
		{
			base.OnStop();

			MainApp.Instance.GPS.LocationChanged -= OnLocationChanged;

			StopEvents();
		}

		void OnLocationChanged (object sender, global::Android.Locations.LocationChangedEventArgs e)
		{
			listView.Invalidate ();
		}

		#endregion

		#region Private Functions

		/// <summary>
		/// Raised, when the screen should be updated.
		/// </summary>
		void Refresh()
		{
			((ScreenMainAdapter)listView.Adapter).NotifyDataSetChanged();
		}

		#endregion

    }

	/// <summary>
	/// Screen main adapter.
	/// </summary>
	public class ScreenMainAdapter : BaseAdapter
	{
		ScreenController ctrl;
		ScreenMain owner;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.Android.ScreenMainAdapter"/> class.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="engine">Engine.</param>
		public ScreenMainAdapter(ScreenMain owner, ScreenController ctrl) : base()
		{
			this.ctrl = ctrl;
			this.owner = owner;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of entries in list.
		/// </summary>
		/// <value>The count.</value>
		public override int Count
		{
			get 
			{ 
				return 5; 
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
			var view = (convertView ?? ctrl.LayoutInflater.Inflate(Resource.Layout.ScreenMainItem, parent, false)) as RelativeLayout;

			view.SetMinimumHeight(ctrl.FindViewById<ListView>(Resource.Id.listView).Height % 4);

			string header;
			string items;
			object image;

			owner.GetContentEntry(position, out header, out items, out image);

			// For position we must generate text here
			if (position == 4) {
				var gps = MainApp.Instance.GPS;
				var location = gps.IsValid ? gps.CoordinatesToString(gps.Latitude, gps.Longitude) : Strings.GetString("unknown");
				var altitude = gps.HasAltitude ? String.Format("{0:0}", gps.Altitude) : "\u0335";
				var accuracy = gps.HasAccuracy ? String.Format("{0:0}", gps.Accuracy) : Strings.Infinite;
				var status = gps.IsValid ? Strings.GetString("valid") : Strings.GetString("invalid");
				items = Strings.GetStringFmt("{0}\n\nAltitude:\t\t\t{1} m\nAccuracy:\t\t{2} m\nStatus:\t\t\t{3}", location, altitude, accuracy, status);;
			}

			using (var textHeader = view.FindViewById<TextView>(Resource.Id.textHeader)) {
				textHeader.SetText(header, TextView.BufferType.Normal);
			}

			using (var textItems = view.FindViewById<TextView>(Resource.Id.textItems)) {
				textItems.SetText(items, TextView.BufferType.Normal);
			}

			using (var imageIcon = view.FindViewById<ImageView>(Resource.Id.imageIcon)) {
				if (image != null) 
					imageIcon.SetImageBitmap((Bitmap)image);
			}

			// Finally return the view
			return view;
		}

		#endregion

	}

}
