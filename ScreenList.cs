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
			listView.Adapter = new ScreenListAdapter (this.Activity, engine, screen);
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
			this.SherlockActivity.SupportActionBar.SetDisplayShowHomeEnabled(true);

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
		private ScreenActivity context;
		private Engine engine;
		private List<UIObject> items;
		private ScreenType screen;

		public ScreenListAdapter(Activity context, Engine engine, ScreenType screen) : base()
		{
			this.context = (ScreenActivity)context;
			this.engine = engine;
			this.screen = screen;
		}

		public List<UIObject> Items {
			get {
				return items;
			}
			set {
				if (items != null)
					foreach (UIObject o in items)
						o.PropertyChanged -= OnPropertyChanged;
				items = value;
				foreach (UIObject o in items)
					o.PropertyChanged += OnPropertyChanged;
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
				Bitmap bm = Bitmap.CreateBitmap(32, 32, Bitmap.Config.Argb8888);
				imageIcon.SetImageBitmap (bm);
				imageIcon.Visibility = ViewStates.Visible;
			}

			textHeader.SetText(items[position].Name, TextView.BufferType.Normal);

			var textDistance = view.FindViewById<TextView> (Resource.Id.textDistance);
			var imageDirection = view.FindViewById<ImageView> (Resource.Id.imageDirection);

			if (screen == ScreenType.LocationScreen || screen == ScreenType.ItemScreen) {
				textDistance.Visibility = ViewStates.Visible;
				imageDirection.Visibility = ViewStates.Visible;
				textDistance.Text = engine.GetDistanceTextOf((Thing)items[position]);
				if (engine.GetDistanceOf((Thing)items[position]) == 0)
					imageDirection.SetImageBitmap (context.DrawCenter ());
				else
					imageDirection.SetImageBitmap (context.DrawArrow (engine.GetBearingOf((Thing)items[position])));
			} else {
				textDistance.Visibility = ViewStates.Gone;
				imageDirection.Visibility = ViewStates.Gone;
			}

			// Finally return the view
			return view;
		}

		public int GetItemAtPosition(int position)
		{
			return position;
		}

		public void OnPropertyChanged(object sender,  PropertyChangedEventArgs e)
		{
			// Check, if one of the visible entries changed
			string[] properties = {"Name","Media","Distance"};

			if (properties.Contains(e.PropertyName))
				context.RunOnUiThread (NotifyDataSetChanged);
		}
	}
}
