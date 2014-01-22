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
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.Android
{
	#region ScreenList

	public partial class ScreenList : global::Android.Support.V4.App.Fragment
	{
		ListView listView;

		#region Constructor

		public ScreenList(Engine engine, ScreenType screen)
		{
			this.screen = screen;
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
			listView.Adapter = new ScreenListAdapter (this, ctrl, screen);
			listView.ItemClick += OnItemClick;

			this.Activity.ActionBar.Title = GetContent ();

			//			Refresh(true);
			//			listView.Invalidate ();

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
			EntrySelected(e.Position);
		}

		public override void OnResume()
		{
			base.OnResume();

			this.Activity.ActionBar.SetDisplayHomeAsUpEnabled (true);
			this.Activity.ActionBar.SetDisplayShowHomeEnabled(true);

			this.Activity.ActionBar.Title = GetContent ();

			Refresh(true);
			//			listView.Invalidate ();
		}

		public override void OnStop()
		{
			base.OnStop();

			StopEvents();
		}

		#endregion

		#region Private Functions

		void Refresh(bool itemsChanged)
		{
			if (itemsChanged)
				this.Activity.ActionBar.Title = GetContent ();

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
			Bitmap bmIcon = null;

			try {
				if (owner.Items[position].Icon != null) {
					bm = Bitmap.CreateScaledBitmap(BitmapFactory.DecodeByteArray (owner.Items[position].Icon.Data, 0, owner.Items[position].Icon.Data.Length),32,32,true);
				} else {
					bm = Bitmap.CreateBitmap(32, 32, Bitmap.Config.Argb8888);
				}

				if (screen == ScreenType.Tasks) {
					Canvas canvas;
					if (owner.Items [position].Icon == null) {
						// No task icon available
						bm = Bitmap.CreateScaledBitmap(BitmapFactory.DecodeResource (ctrl.Resources, Resource.Drawable.TaskPending),32,32,true);
					}
					// Copy icons to a bigger icon, so that the state image has place
					bmIcon = Bitmap.CreateBitmap (48, 48, Bitmap.Config.Argb8888);
					canvas = new Canvas (bmIcon);
					canvas.DrawBitmap (bm, (canvas.Width-bm.Width)/2, (canvas.Height-bm.Height)/2, null);
					bm.Dispose();
					Bitmap bmState = null;
					string s = ((Task)owner.Items [position]).CorrectState.ToString().ToLower();
					if (((Task)owner.Items [position]).Complete && (((Task)owner.Items [position]).CorrectState == TaskCorrectness.None || ((Task)owner.Items [position]).CorrectState == TaskCorrectness.Correct))
						bmState = Bitmap.CreateScaledBitmap(BitmapFactory.DecodeResource (ctrl.Resources, Resource.Drawable.TaskComplete),48,48,true);
					else if (((Task)owner.Items [position]).Complete && ((Task)owner.Items [position]).CorrectState == TaskCorrectness.NotCorrect)
						bmState = Bitmap.CreateScaledBitmap(BitmapFactory.DecodeResource (ctrl.Resources, Resource.Drawable.TaskFailed),48,48,true);
					if (bmState != null) {
						canvas = new Canvas (bmIcon);
						canvas.DrawBitmap (bmState, (canvas.Width-bmState.Width)/2, (canvas.Height-bmState.Height)/2, null);
					}
					bm = bmIcon;
				}

				using (var imageIcon = view.FindViewById<ImageView>(Resource.Id.imageIcon)) {
					imageIcon.SetImageBitmap (bm);
					imageIcon.Visibility = ViewStates.Visible;
				}
			} finally {
				bm.Dispose();
			}


			using(var textHeader = view.FindViewById<TextView>(Resource.Id.textHeader)) {
				textHeader.SetText(owner.Items[position].Name, TextView.BufferType.Normal);
			}

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
								imageDirection.SetImageBitmap (ctrl.DrawArrow (((Thing)owner.Items[position]).VectorFromPlayer.Bearing.Value));
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
