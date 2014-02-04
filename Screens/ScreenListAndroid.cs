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
using Android.Support.V7.App;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.Android
{
	#region ScreenList

	public partial class ScreenList : global::Android.Support.V4.App.Fragment
	{
		ListView listView;
		IMenuItem menuMap;

		#region Constructor

		public ScreenList(Engine engine, ScreenType screen)
		{
			this.type = screen;
			this.engine = engine;
		}

		#endregion

		#region Android Event Handlers

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((ScreenController)this.Activity);

			if (container == null)
				return null;

			var view = inflater.Inflate(Resource.Layout.ScreenList, container, false);

			listView = view.FindViewById<ListView> (Resource.Id.listView);
			listView.Adapter = new ScreenListAdapter (this, ctrl, type);
			listView.ItemClick += OnItemClick;

			ctrl.SupportActionBar.Title = GetContent ();

			HasOptionsMenu = (type == ScreenType.Locations || type == ScreenType.Items);

			return view;
		}

		/// <summary>
		/// Raised when the fragment is destroyed, so free references to other UI elements.
		/// </summary>
		public override void OnDestroyView()
		{
			base.OnDestroyView();

			Items = null;
			listView = null;
			ctrl = null;
			engine = null;
		}

		public void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			ctrl.Feedback();

			EntrySelected(e.Position);
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater) 
		{
			inflater.Inflate (Resource.Menu.ScreenListMenu, menu);

			menuMap = menu.FindItem (Resource.Id.menu_screen_list_map);

			if (type == ScreenType.Locations || type == ScreenType.Items) {
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

			if (item == menuMap) {
				ctrl.ShowScreen(ScreenType.Map, null);
				return false;
			}

			return true;
		}

		public override void OnResume()
		{
			base.OnResume();

			ctrl.SupportActionBar.SetDisplayHomeAsUpEnabled (true);
			ctrl.SupportActionBar.SetDisplayShowHomeEnabled(true);

			ctrl.SupportActionBar.Title = GetContent ();

			StartEvents();

			if (type == ScreenType.Locations || type == ScreenType.Items)
				MainApp.Instance.GPS.HeadingChanged += OnHeadingChanged;

			Refresh(true);
		}

		public override void OnPause()
		{
			base.OnStop();

			if (type == ScreenType.Locations || type == ScreenType.Items)
				MainApp.Instance.GPS.HeadingChanged -= OnHeadingChanged;

			StopEvents();
		}

		void OnHeadingChanged (object sender, EventArgs e)
		{
			Refresh(false);
		}

		#endregion

		#region Private Functions

		void Refresh(bool itemsChanged)
		{
			if (itemsChanged && Activity != null)
				((ActionBarActivity)Activity).SupportActionBar.Title = GetContent ();

			if (listView != null)
				((ScreenListAdapter)listView.Adapter).NotifyDataSetChanged();
		}

		#endregion
	}

	#endregion

	#region ScreenListAdapter

	public class ScreenListAdapter : BaseAdapter
	{
		ScreenController ctrl;
		ScreenList owner;
		ScreenType screen;

		#region Constructor

		public ScreenListAdapter(ScreenList owner, ScreenController ctrl, ScreenType screen) : base()
		{
			this.ctrl = ctrl;
			this.owner = owner;
			this.screen = screen;
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
			var view = (convertView ?? ctrl.LayoutInflater.Inflate(Resource.Layout.ScreenListItem, parent, false)) as RelativeLayout;

			Bitmap bm = null;

			var layout = view.FindViewById<LinearLayout>(Resource.Id.linearLayoutItemText);
			var imageIcon = view.FindViewById<ImageView>(Resource.Id.imageIcon);
			var textHeader = view.FindViewById<TextView>(Resource.Id.textHeader);

			if (!owner.ShowDirections) {
				var layoutParams = (RelativeLayout.LayoutParams)layout.LayoutParameters;
				layoutParams.RightMargin = 0;
				layout.LayoutParameters = layoutParams;
			}

			try {
				if (owner.ShowIcons) {
					if (owner.Items[position].Icon != null) {
						bm = Bitmap.CreateScaledBitmap(BitmapFactory.DecodeByteArray (owner.Items[position].Icon.Data, 0, owner.Items[position].Icon.Data.Length),32,32,true);
					} else {
						bm = Bitmap.CreateBitmap(32, 32, Bitmap.Config.Argb8888);
					}
					imageIcon.SetImageBitmap (bm);
					imageIcon.Visibility = ViewStates.Visible;
				} else {
					imageIcon.Visibility = ViewStates.Gone;
				}
			} finally {
				if (bm != null)
					bm.Dispose();
			}

			string name = owner.Items[position].Name == null ? "" : owner.Items[position].Name;
			if (owner.Items[position] is Task)
				textHeader.Text = (((Task)owner.Items[position]).Complete ? (((Task)owner.Items[position]).CorrectState == TaskCorrectness.NotCorrect ? Strings.TaskNotCorrect : Strings.TaskCorrect) + " " : "") + name;
			else
				textHeader.Text = name;

			using (var textDistance = view.FindViewById<TextView> (Resource.Id.textDistance)) {
				using(var imageDirection = view.FindViewById<ImageView> (Resource.Id.imageDirection)) {

					if (screen == ScreenType.Locations || screen == ScreenType.Items) {
						if (((Thing)owner.Items[position]).VectorFromPlayer != null) {
							textDistance.Visibility = ViewStates.Visible;
							imageDirection.Visibility = ViewStates.Visible;
							textDistance.Text = ((Thing)owner.Items[position]).VectorFromPlayer.Distance.BestMeasureAs(DistanceUnit.Meters);
							if (((Thing)owner.Items[position]).VectorFromPlayer.Distance.Value == 0)
								imageDirection.SetImageBitmap (ctrl.DrawCenter ());
							else
								imageDirection.SetImageBitmap (ctrl.DrawArrow (((Thing)owner.Items[position]).VectorFromPlayer.Bearing.Value + MainApp.Instance.GPS.Heading));
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
