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
using Android.Text;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
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
		Command com;

		#region Constructor

		public ScreenDetail(ScreenController ctrl, UIObject obj)
		{
			this.ctrl = ctrl;
			this.activeObject = obj;
			if (this.activeObject != null) {
				this.activeObject.PropertyChanged += OnPropertyChanged;
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

			HasOptionsMenu = !(activeObject is Task);

			return view;
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater) 
		{
			inflater.Inflate (Resource.Menu.ScreenDetailMenu, menu);

			menuMap = menu.FindItem (Resource.Id.menu_screen_detail_map);

			if (activeObject != null && activeObject is Thing && !ctrl.Engine.VisibleInventory.Contains((Thing)activeObject)) {
				menuMap.SetVisible (true);
			} else {
				menuMap.SetVisible(false);
			}

			base.OnCreateOptionsMenu(menu, inflater);
		}

		/// <summary>
		/// Raised when the fragment is destroyed, so free references to other UI elements.
		/// </summary>
		public override void OnDestroyView()
		{
			base.OnDestroyView();

			imageView.SetImageBitmap(null);
			imageView = null;
			layoutButtons = null;
			layoutWorksWith = null;
			textDescription = null;
			textWorksWith = null;
			menuMap = null;
			activeObject = null;
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
			ctrl.Feedback();

			//This uses the imported MenuItem from ActionBarSherlock
			switch (item.ItemId) {
				case Resource.Id.menu_screen_detail_map:
					ctrl.ShowScreen(ScreenType.Map, activeObject);
				// TODO: Remove, Show map
//					menuMap.SetVisible(false);
//					menuDefault.SetVisible(true);
//					layoutMap.Visibility = ViewStates.Visible;
//					layoutDefault.Visibility = ViewStates.Gone;
					return false;
					break;
			}

			return base.OnOptionsItemSelected(item);;
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

		public override void OnPause()
		{
			StopEvents();

			base.OnPause();
		}

		public void OnButtonClicked(object sender, EventArgs e)
		{
			ctrl.Feedback();

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
			ctrl.Feedback();

			layoutButtons.Visibility = ViewStates.Visible;
			layoutWorksWith.Visibility = ViewStates.Gone;

			Refresh();

			com.Execute(targets[(int)((Button)sender).Tag]);
		}

		public void OnNothingClicked(object sender, EventArgs e)
		{
			ctrl.Feedback();

			layoutButtons.Visibility = ViewStates.Visible;
			layoutWorksWith.Visibility = ViewStates.Gone;

			Refresh();
		}

		#endregion

		#region Private Functions

		private void Refresh(string what = "")
		{
			if (activeObject != null && this.Activity != null) {
				// Assign this item's values to the various subviews
				ctrl.SupportActionBar.SetDisplayShowHomeEnabled(true);

				string name = activeObject.Name == null ? "" : activeObject.Name;

				if (what.Equals ("") || what.Equals ("Name"))
				{
					if (activeObject is Task)
						ctrl.SupportActionBar.Title = (((Task)activeObject).Complete ? (((Task)activeObject).CorrectState == TaskCorrectness.NotCorrect ? Strings.TaskNotCorrect : Strings.TaskCorrect) + " " : "") + name;
					else
						ctrl.SupportActionBar.Title = name;
				}

				if (what.Equals ("") || what.Equals ("Media")) {
					if (activeObject.Image != null) {
						imageView.SetImageBitmap(null);
						using (Bitmap bm = ctrl.ConvertMediaToBitmap(activeObject.Image)) {
							imageView.SetImageBitmap (bm);
						}
						imageView.Visibility = ViewStates.Visible;
					} else {
						imageView.Visibility = ViewStates.Gone;
					}
				}

				if (what.Equals ("") || what.Equals ("Description")) {
					if (!String.IsNullOrWhiteSpace (activeObject.Description)) {
						textDescription.Visibility = ViewStates.Visible;
						textDescription.Text = activeObject.Description; // Html.FromHtml(activeObject.HTML.Replace("&lt;BR&gt;", "<br>").Replace("<br>\n", "<br>").Replace("\n", "<br>"));
						textDescription.Gravity = PrefHelper.TextAlignment;
						textDescription.SetTextSize(global::Android.Util.ComplexUnitType.Sp, PrefHelper.TextSize);
					} else {
						textDescription.Visibility = ViewStates.Visible;
						textDescription.Text = "";
						textDescription.Gravity = PrefHelper.TextAlignment;
						textDescription.SetTextSize(global::Android.Util.ComplexUnitType.Sp, PrefHelper.TextSize);
					}
				}
				// Tasks don't have any command button
				if (activeObject is Task)
					return;

				if (layoutButtons.Visibility == ViewStates.Visible) {
					layoutButtons.RemoveAllViews ();
					commands = ((Thing)activeObject).ActiveCommands;
					for (int i = 0; i < commands.Count; i++) {
						Button btnView = new Button (Activity.ApplicationContext) {
							Text = commands[i].Text,
							Tag = i
						};
						btnView.SetTextColor(Color.Black);
						btnView.SetHighlightColor(Color.Black);
						btnView.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);
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
						btnView.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);
						btnView.Text = GetString(Resource.String.ok);
						btnView.Click += OnNothingClicked;
						layoutWorksWith.AddView (btnView);
					} else {
						textWorksWith.Text = com.Text;
						for (int i = 0; i < targets.Count; i++) {
							Button btnView = new Button (Activity.ApplicationContext);
							btnView.SetTextColor(Color.Black);
							btnView.SetHighlightColor(Color.Black);
							btnView.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);
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

