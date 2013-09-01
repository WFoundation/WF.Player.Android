using System;
/// WF.Player.Android - A Wherigo Player User Interface for Android platform.
/// Copyright (C) 2012-2013  Dirk Weltz <web@weltz-online.de>
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
using ActionbarSherlock.App;
using WF.Player.Core;


namespace WF.Player.Android
{
	public class ScreenList : SherlockFragment
	{
		private Engine engine;
		private ListView listView;
		private ScreenType screen;

		public ScreenList(Engine engine, ScreenType screen)
		{
			this.screen = screen;
			this.engine = engine;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			//TODO: Restore instance state data
			//TODO: Handle creation logic
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			//TODO: Restore instance state data
			Context context = Activity.ApplicationContext;

			if (container == null)
				return null;

			var view = inflater.Inflate(Resource.Layout.ScreenList, container, false);

			listView = view.FindViewById<ListView> (Resource.Id.listView);
			listView.Adapter = new ScreenListAdapter (this.Activity, engine);
			listView.ItemClick += OnItemClick;

			updateContent ();

			return view;
		}

		public void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			((ScreenActivity)this.Activity).ShowScreen (ScreenType.DetailScreen, ((ScreenListAdapter)listView.Adapter).Items[e.Position].ObjIndex);
		}

		public override void OnResume()
		{
			base.OnResume();

			//TODO: Load data
			this.SherlockActivity.SupportActionBar.SetDisplayHomeAsUpEnabled (true);
			updateContent ();
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			//TODO: Store instance state data
		}

		#region Private Functions

		private void updateContent()
		{
			List<UIObject> items = new List<UIObject> ();

			switch (screen) {
			case ScreenType.LocationScreen:
				this.SherlockActivity.SupportActionBar.Title = GetString (Resource.String.screen_locations);
				foreach (UIObject item in engine.ActiveVisibleZones)
					items.Add (item);
				((ScreenListAdapter)listView.Adapter).Items = items;
				break;
			case ScreenType.ItemScreen:
				this.SherlockActivity.SupportActionBar.Title = GetString (Resource.String.screen_yousee);
				foreach (UIObject item in engine.VisibleObjects)
					items.Add (item);
				((ScreenListAdapter)listView.Adapter).Items = items;
				break;
			case ScreenType.InventoryScreen:
				this.SherlockActivity.SupportActionBar.Title = GetString (Resource.String.screen_inventory);
				foreach (UIObject item in engine.VisibleInventory)
					items.Add (item);
				((ScreenListAdapter)listView.Adapter).Items = items;
				break;
			case ScreenType.TaskScreen:
				this.SherlockActivity.SupportActionBar.Title = GetString (Resource.String.screen_tasks);
				foreach (UIObject item in engine.ActiveVisibleTasks)
					items.Add (item);
				((ScreenListAdapter)listView.Adapter).Items = items;
				break;
			}
			listView.Invalidate ();
		}

		#endregion

	}

	public class ScreenListAdapter : BaseAdapter
	{
		private Activity context;
		private Engine engine;
		private List<UIObject> items;

		public ScreenListAdapter(Activity context, Engine engine) : base()
		{
			this.context = context;
			this.engine = engine;
		}

		public List<UIObject> Items {
			get {
				return items;
			}
			set {
				items = value;
				NotifyDataSetInvalidated ();																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																	
			}
		}

		public override int Count
		{
			get { 
				if (items != null)
					return items.Count;
				else
					return 0;
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
			var view = (convertView ?? context.LayoutInflater.Inflate(Resource.Layout.ScreenListItem, parent, false)) as RelativeLayout;

			var imageIcon = view.FindViewById<ImageView>(Resource.Id.imageIcon);
			var textHeader = view.FindViewById<TextView>(Resource.Id.textHeader);

			if (items[position].Icon != null) {
				Bitmap bm = BitmapFactory.DecodeByteArray (items[position].Icon.Data, 0, items[position].Icon.Data.Length);
				imageIcon.SetImageBitmap (bm);
				imageIcon.Visibility = ViewStates.Visible;
			} else {
				imageIcon.Visibility = ViewStates.Invisible;
			}

			textHeader.SetText(items[position].Name, TextView.BufferType.Normal);

			// Finally return the view
			return view;
		}

		public int GetItemAtPosition(int position)
		{
			return position;
		}

		public void OnPropertyChanged(object sender,  PropertyChangedEventArgs e)
		{
			context.RunOnUiThread (NotifyDataSetChanged);
		}
	}
}
