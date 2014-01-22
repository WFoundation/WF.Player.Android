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
using Android.Text;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.Support.V4.App;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.Android
{
	#region ScreenDetail

	public partial class ScreenDetail : global::Android.Support.V4.App.Fragment
	{
		ImageView imageView;
		ScrollView layoutDefault;
		LinearLayout layoutMap;
		LinearLayout layoutButtons;
		LinearLayout layoutWorksWith;
		TextView textDescription;
		TextView textWorksWith;
		IMenuItem menuMap;
		IMenuItem menuDefault;
		Command com;

		#region Constructor

		public ScreenDetail(ScreenController ctrl, UIObject obj)
		{
			this.ctrl = ctrl;
			this.obj = obj;
			if (this.obj != null) {
				this.obj.PropertyChanged += OnPropertyChanged;
			}
		}

		#endregion

		#region Android Event Handlers

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			ctrl = ((ScreenController)this.Activity);

			if (container == null)
				return null;

			var view = inflater.Inflate(Resource.Layout.ScreenDetail, container, false);

			layoutDefault = view.FindViewById<ScrollView> (Resource.Id.scrollView);
			layoutMap = view.FindViewById<LinearLayout> (Resource.Id.layoutMap);

			imageView = view.FindViewById<ImageView> (Resource.Id.imageView);
			textDescription = view.FindViewById<TextView> (Resource.Id.textDescription);
			textWorksWith = view.FindViewById<TextView> (Resource.Id.textWorksWith);
			layoutButtons = view.FindViewById<LinearLayout> (Resource.Id.layoutButtons);
			layoutWorksWith = view.FindViewById<LinearLayout> (Resource.Id.layoutWorksWith);

			layoutButtons.Visibility = ViewStates.Visible;
			layoutWorksWith.Visibility = ViewStates.Gone;
			layoutMap.Visibility = ViewStates.Gone;

			HasOptionsMenu = !(obj is Task);

			return view;
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater) 
		{
			inflater.Inflate (Resource.Menu.ScreenDetailMenu, menu);

			menuMap = menu.FindItem (Resource.Id.menu_screen_detail_map);
			menuDefault = menu.FindItem (Resource.Id.menu_screen_detail_default);

			if (obj != null && obj is Thing && !ctrl.Engine.VisibleInventory.Contains((Thing)obj)) {
				menuMap.SetVisible (true);
			} else {
				menuMap.SetVisible(false);
			}
			menuDefault.SetVisible(false);

			base.OnCreateOptionsMenu(menu, inflater);
		}

		/// <summary>
		/// Raised when the fragment is destroyed, so free references to other UI elements.
		/// </summary>
		public override void OnDestroyView()
		{
			base.OnDestroyView();

			imageView = null;
			layoutButtons = null;
			layoutWorksWith = null;
			textDescription = null;
			textWorksWith = null;
			menuMap = null;
			menuDefault = null;
			obj = null;
			com = null;
			commands = null;
			targets = null;
			ctrl = null;
		}

		public void OnDialogClicked(object sender, DialogClickEventArgs e)
		{
			Console.WriteLine (e.Which);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			//This uses the imported MenuItem from ActionBarSherlock
			switch (item.ItemId) {
				case Resource.Id.menu_screen_detail_map:
					// TODO: Show map
					menuMap.SetVisible(false);
					menuDefault.SetVisible(true);
					layoutMap.Visibility = ViewStates.Visible;
					layoutDefault.Visibility = ViewStates.Gone;
					break;
				case Resource.Id.menu_screen_detail_default:
					// TODO: Show default
					menuMap.SetVisible(true);
					menuDefault.SetVisible(false);
					layoutMap.Visibility = ViewStates.Gone;
					layoutDefault.Visibility = ViewStates.Visible;
					break;
			}

			return true;
		}

		public override void OnResume()
		{
			base.OnResume();

			StartEvents();

			//TODO: Load data
//			if (remove)
//				((ScreenController)this.Activity).RemoveScreen (ScreenType.Details);
//
			Refresh ();
		}

		public override void OnStop()
		{
			StopEvents();

			base.OnStop();
		}

		public void OnButtonClicked(object sender, EventArgs e)
		{
			int tag = (int)((Button)sender).Tag;

			Command c = commands [tag];
					
			if (c.CmdWith) 
			{
				layoutButtons.Visibility = ViewStates.Gone;
				layoutWorksWith.Visibility = ViewStates.Visible;
				com = c;
				targets = com.TargetObjects;
				Refresh ();
			} 
			else
			{
				c.Execute ();
			}
		}

		public void OnThingClicked(object sender, EventArgs e)
		{
			layoutButtons.Visibility = ViewStates.Visible;
			layoutWorksWith.Visibility = ViewStates.Gone;

			Refresh();

			com.Execute(targets[(int)((Button)sender).Tag]);
		}

		public void OnNothingClicked(object sender, EventArgs e)
		{
			layoutButtons.Visibility = ViewStates.Visible;
			layoutWorksWith.Visibility = ViewStates.Gone;

			Refresh();
		}

		#endregion

		#region Private Functions

		private void Refresh(string propertyName = null)
		{
			if (obj != null && this.Activity != null) {
				// Assign this item's values to the various subviews
				this.Activity.ActionBar.Title = obj.Name;
				this.Activity.ActionBar.SetDisplayShowHomeEnabled(true);

				Bitmap bm = null;

				try {
					if (obj.Image != null) {
						bm = BitmapFactory.DecodeByteArray (obj.Image.Data, 0, obj.Image.Data.Length);
					} else {
						if (obj is Task) {
							if (((Task)obj).Complete && (((Task)obj).CorrectState == TaskCorrectness.None || ((Task)obj).CorrectState == TaskCorrectness.Correct))
								bm = Bitmap.CreateScaledBitmap (BitmapFactory.DecodeResource (Activity.ApplicationContext.Resources, Resource.Drawable.TaskDetailComplete), 128, 128, true);
							else if (((Task)obj).Complete && ((Task)obj).CorrectState == TaskCorrectness.NotCorrect)
								bm = Bitmap.CreateScaledBitmap (BitmapFactory.DecodeResource (Activity.ApplicationContext.Resources, Resource.Drawable.TaskDetailFailed), 128, 128, true);
						}
					}

					if (bm != null) {
						imageView.SetImageBitmap (bm);
						imageView.Visibility = ViewStates.Visible;
					} else {
						imageView.Visibility = ViewStates.Gone;
					}
				} finally {
					if (bm != null)
						bm.Dispose();
				}

				// TODO: HTML
				textDescription.TextFormatted = Html.FromHtml(obj.HTML.Replace("&lt;BR&gt;", "<br>"));

				if (obj is Task)
					return;

				if (layoutButtons.Visibility == ViewStates.Visible) {
					layoutButtons.RemoveAllViews ();
					commands = ((Thing)obj).ActiveCommands;
					for (int i = 0; i < commands.Count; i++) {
						Button btnView = new Button (Activity.ApplicationContext) {
							Text = commands[i].Text,
							Tag = i
						};
						btnView.SetTextColor(Color.Black);
						btnView.SetHighlightColor(Color.Black);
						btnView.Click += OnButtonClicked;
						layoutButtons.AddView (btnView);
					}
				}

				if (layoutWorksWith.Visibility == ViewStates.Visible) {
					layoutWorksWith.RemoveViews(1,layoutWorksWith.ChildCount-1);
					if (targets.Count == 0) {
						textWorksWith.Text = com.EmptyTargetListText;
						Button btnView = new Button (Activity.ApplicationContext);
						btnView.SetTextColor(Color.Black);
						btnView.SetHighlightColor(Color.Black);
						btnView.Text = GetString(Resource.String.ok);
						btnView.Click += OnNothingClicked;
						layoutWorksWith.AddView (btnView);
					} else {
						textWorksWith.Text = com.Text;
						for (int i = 0; i < targets.Count; i++) {
							Button btnView = new Button (Activity.ApplicationContext);
							btnView.SetTextColor(Color.Black);
							btnView.SetHighlightColor(Color.Black);
							btnView.Text = targets[i].Name;
							btnView.Tag = i;
							btnView.Click += OnThingClicked;
							layoutWorksWith.AddView (btnView);
						}
					}
				}

				// Resize scrollview
				layoutDefault.Invalidate();
			}
		}

		#endregion

	}

	#endregion

}

