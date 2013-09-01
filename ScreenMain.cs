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
	public class ScreenMain : SherlockFragment
	{
		private Engine engine;
		private ListView listView;

		public ScreenMain(Engine engine)
		{
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

//			var textView = new TextView (container);
//			textView.Text = "Test";
//			container.AddView (textView);

			var view = inflater.Inflate(Resource.Layout.ScreenMain, container, false);

//			ListView view = new ListView (container);
		
			listView = view.FindViewById<ListView> (Resource.Id.listView);
//			listView.Adapter = new ArrayAdapter<string> ((this.Activity, Resource.Layout.ScreenMainItem, new string[] { "Test1", "Test2", "Test3" });
			listView.Adapter = new ScreenMainAdapter (this.Activity, engine);
			listView.ItemClick += OnItemClick;

			return view;
		}

		public override void OnResume()
		{
			base.OnResume();

			//TODO: Load data
			this.SherlockActivity.SupportActionBar.SetDisplayHomeAsUpEnabled (false);
			this.SherlockActivity.SupportActionBar.Title = engine.Cartridge.Name;
			listView.Invalidate ();
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			//TODO: Store instance state data
		}

//		public override bool OnCreateOptionsMenu (IMenu menu)
//		{
//			ISubMenu sub = menu.AddSubMenu ("Theme");
//			sub.Add (0, Resource.Style.Theme_Sherlock, 0, "Default");
//			sub.Add (0, Resource.Style.Theme_Sherlock_Light, 0, "Light");
//			sub.Add (0, Resource.Style.Theme_Sherlock_Light_DarkActionBar, 0, "Light (Dark Action Bar)");
//			sub.Item.SetShowAsAction (MenuItem.ShowAsActionAlways | MenuItem.ShowAsActionWithText);
//			return true;
//		}

//		public override bool OnOptionsItemSelected (IMenuItem item)
//		{
//			if (item.ItemId == Android.Resource.Id.Home || item.ItemId == 0) {
//
//				return false;
//			}
//			return true;
//		}

		private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			switch (e.Position) {
			case 0:
				if (engine.ActiveVisibleZones.Count > 0)
					((ScreenActivity)this.Activity).ShowScreen (ScreenType.LocationScreen, -1);
				break;
			case 1:
				if (engine.VisibleObjects.Count > 0)
					((ScreenActivity)this.Activity).ShowScreen (ScreenType.ItemScreen, -1);
				break;
			case 2:
				if (engine.VisibleInventory.Count > 0)
					((ScreenActivity)this.Activity).ShowScreen (ScreenType.InventoryScreen, -1);
				break;
			case 3:
				if (engine.ActiveVisibleTasks.Count > 0)
					((ScreenActivity)this.Activity).ShowScreen (ScreenType.TaskScreen, -1);
				break;
			}
		}
    }

	public class ScreenMainAdapter : BaseAdapter
	{
		private Activity context;
		private Engine engine;

		public ScreenMainAdapter(Activity context, Engine engine) : base()
		{
			this.context = context;
			this.engine = engine;
		}

		public override int Count
		{
			get { return 4; }
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
			var view = (convertView ?? context.LayoutInflater.Inflate(Resource.Layout.ScreenMainItem, parent, false)) as RelativeLayout;

			view.SetMinimumHeight(context.FindViewById<ListView>(Resource.Id.listView).Height % 4);

			var imageIcon = view.FindViewById<ImageView>(Resource.Id.imageIcon);
			var textHeader = view.FindViewById<TextView>(Resource.Id.textHeader);
			var textItems = view.FindViewById<TextView>(Resource.Id.textItems);

			Bitmap bm = null;
			int count = 0;
			int header = Resource.String.unknown;
			string items = "";

			if (engine != null)
			{
				switch (position) {
					case 0:
						count = engine.ActiveVisibleZones.Count;
						header = Resource.String.screen_locations;
						foreach(UIObject o in engine.ActiveVisibleZones)
							items += (items != "" ? ", " : "") + o.Name;
						bm = BitmapFactory.DecodeResource (context.Resources, Resource.Drawable.IconLocation);
						break;
					case 1:
						count = engine.VisibleObjects.Count;
						header = Resource.String.screen_yousee;
						foreach(UIObject o in engine.VisibleObjects)
							items += (items != "" ? ", " : "") + o.Name;
						bm = BitmapFactory.DecodeResource (context.Resources, Resource.Drawable.IconYouSee);
						break;
					case 2:
						count = engine.VisibleInventory.Count;
						header = Resource.String.screen_inventory;
						foreach(UIObject o in engine.VisibleInventory)
							items += (items != "" ? ", " : "") + o.Name;
						bm = BitmapFactory.DecodeResource (context.Resources, Resource.Drawable.IconInventory);
						break;
					case 3:
						count = engine.ActiveVisibleTasks.Count;
						header = Resource.String.screen_tasks;
						foreach(UIObject o in engine.ActiveVisibleTasks)
							items += (items != "" ? ", " : "") + o.Name;
						bm = BitmapFactory.DecodeResource (context.Resources, Resource.Drawable.IconTask);
						break;
					}
			}

			textHeader.SetText(String.Format ("{0} [{1}]", context.GetString (header), count), TextView.BufferType.Normal);
			textItems.SetText(items, TextView.BufferType.Normal);
			if (bm != null)
				imageIcon.SetImageBitmap (bm);

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
