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
using Android.Text;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using WF.Player.Core;
using WF.Player.Core.Engines;
using WF.Player.Types;

namespace WF.Player.Game
{
	#region GameDetailScreen

	public partial class GameDetailScreen : global::Android.Support.V4.App.Fragment
	{
		ImageView _imageView;
		ImageView _imageDirection;
		ScrollView _layoutDefault;
		LinearLayout _layoutBottom;
		LinearLayout _layoutButtons;
		LinearLayout _layoutDirection;
		TextView _textDescription;
		TextView _textDirection;
		IMenuItem menuMap;
		Command _com;
		double _lastBearing = 0;

		#region Constructor

		public GameDetailScreen(GameController ctrl, UIObject obj)
		{
			this.ctrl = ctrl;
			this.activeObject = obj;
			if (this.activeObject != null) {
				this.activeObject.PropertyChanged += OnPropertyChanged;
			}

			_refresh = new CallDelayer(100, 500, (o) => Refresh(o));
		}

		#endregion

		#region Android Event Handlers

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			ctrl = ((GameController)this.Activity);

			if (container == null)
				return null;

			var view = inflater.Inflate(Resource.Layout.GameDetailScreen, container, false);

			_layoutDefault = view.FindViewById<ScrollView> (Resource.Id.scrollView);

			_imageView = view.FindViewById<ImageView> (Resource.Id.imageView);
			_imageDirection = view.FindViewById<ImageView> (Resource.Id.imageDirection);
			_textDescription = view.FindViewById<TextView> (Resource.Id.textDescription);
			_textDirection = view.FindViewById<TextView> (Resource.Id.textDirection);
			_layoutBottom = view.FindViewById<LinearLayout> (Resource.Id.layoutBottom);
			_layoutButtons = view.FindViewById<LinearLayout> (Resource.Id.layoutButtons);
			_layoutDirection = view.FindViewById<LinearLayout> (Resource.Id.layoutDirection);

			// Don't know a better way :(
			_layoutBottom.SetBackgroundResource(Main.BottomBackground);

			_layoutButtons.Visibility = ViewStates.Visible;

			HasOptionsMenu = !(activeObject is Task);

			return view;
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater) 
		{
			inflater.Inflate (Resource.Menu.GameDetailScreenMenu, menu);

			menuMap = menu.FindItem (Resource.Id.menu_screen_detail_map);

			if (activeObject != null && activeObject is Thing && !ctrl.Engine.VisibleInventory.Contains((Thing)activeObject)) {
				menuMap.SetVisible (true);
			} else {
				menuMap.SetVisible(false);
			}

			base.OnCreateOptionsMenu(menu, inflater);
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
					ctrl.ShowScreen(ScreenTypes.Map, activeObject);
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

			// Show title bar with the correct buttons
			((ActionBarActivity)Activity).SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			((ActionBarActivity)Activity).SupportActionBar.SetDisplayShowHomeEnabled(true);
			((ActionBarActivity)Activity).SupportActionBar.Title = activeObject.Name;

			StartEvents();

			Main.GPS.AddOrientationListener(OnOrientationChanged);

			Refresh ();
		}

		public override void OnPause()
		{
			StopEvents();

			Main.GPS.RemoveOrientationListener(OnOrientationChanged);

			base.OnPause();
		}

		void OnOrientationChanged (object sender, WF.Player.Location.OrientationChangedEventArgs e)
		{
			if (Math.Abs(Main.GPS.Bearing - _lastBearing) > 2) {
				_lastBearing = Main.GPS.Bearing;
				_refresh.Call();
			}
		}

		public void OnButtonClicked(object sender, EventArgs e)
		{
			// Sound and vibration if it is selected
			ctrl.Feedback();

			if (commands.Count == 1) {
				// Don't update anymore
				_refresh.Abort();
				// We have only one command, so we don't need an extra dialog
				CommandSelected(commands[0]);
			} else {
				// We have a list of commands, so show this to the user
				string[] commandNames = new string[commands.Count];
				for(int i = 0; i < commands.Count; i++)
					commandNames[i] = commands[i].Text;
				AlertDialog.Builder builder = new AlertDialog.Builder(ctrl);
				builder.SetTitle(Resource.String.screen_detail_commands);
				builder.SetItems(commandNames, OnCommandListClicked);
				builder.SetNeutralButton(Resource.String.cancel, (s, args) => {});
				builder.Show();
			}
		}

		void OnCommandListClicked (object sender, DialogClickEventArgs e)
		{
			// Don't update anymore
			_refresh.Abort();

			CommandSelected(commands[e.Which]);
		}

		void OnTargetListClicked (object sender, DialogClickEventArgs e)
		{
			// Don't update anymore
			_refresh.Abort();

			_com.Execute(targets[e.Which]);
		}

		#endregion

		#region Private Functions

		void CommandSelected(Command c)
		{
			// Save for later use if there are more than one target
			_com = c;

			if (_com.CmdWith) 
			{
				// Display WorksWith list to user
				targets = _com.TargetObjects;
				if (targets.Count == 0) {
					// We don't have any target, so display empty text
					AlertDialog.Builder builder = new AlertDialog.Builder(ctrl);
					builder.SetTitle(c.Text);
					builder.SetMessage(_com.EmptyTargetListText);
					builder.SetNeutralButton(Resource.String.ok, (sender, e) => {});
					builder.Show();
				} else {
					// We have a list of commands, so show this to the user
					string[] targetNames = new string[targets.Count];
					for(int i = 0; i < targets.Count; i++)
						targetNames[i] = targets[i].Name;
					AlertDialog.Builder builder = new AlertDialog.Builder(ctrl);
					builder.SetTitle(c.Text);
					builder.SetItems(targetNames, OnTargetListClicked);
					builder.SetNeutralButton(Resource.String.cancel, (sender, e) => {});
					builder.Show();
				}
			} 
			else
			{
				// Execute command
				_com.Execute ();
			}
		}

		void Refresh(object o = null)
		{
			string what = o == null ? "" : (string)o;

			if (activeObject == null || this.Activity == null)
				return;

			Activity.RunOnUiThread(() => {
				if(Activity == null)
					return;

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
						using (Bitmap bm = ctrl.ConvertMediaToBitmap(activeObject.Image)) {
							_imageView.SetImageBitmap(null);
							_imageView.SetImageBitmap(bm);
						}
						_imageView.Visibility = ViewStates.Visible;
					} else {
						_imageView.Visibility = ViewStates.Gone;
					}
				}

				if (what.Equals ("") || what.Equals ("Description")) {
					if (!String.IsNullOrWhiteSpace (activeObject.Description)) {
						_textDescription.Visibility = ViewStates.Visible;
						_textDescription.Text = activeObject.Description; // Html.FromHtml(activeObject.HTML.Replace("&lt;BR&gt;", "<br>").Replace("<br>\n", "<br>").Replace("\n", "<br>"));
						_textDescription.Gravity = Main.Prefs.TextAlignment.ToSystem();
						_textDescription.SetTextSize(global::Android.Util.ComplexUnitType.Sp, (float)Main.Prefs.TextSize);
					} else {
						_textDescription.Visibility = ViewStates.Visible;
						_textDescription.Text = "";
						_textDescription.Gravity = Main.Prefs.TextAlignment.ToSystem();
						_textDescription.SetTextSize(global::Android.Util.ComplexUnitType.Sp, (float)Main.Prefs.TextSize);
					}
				}
				// Tasks don't have any command button or direction
				if (activeObject is Task) {
					_layoutBottom.Visibility =  ViewStates.Gone;
					return;
				}

				// Check, if the bottom should be displayed or not
				_layoutButtons.Visibility = ((Thing)activeObject).ActiveCommands.Count == 0 ? ViewStates.Invisible : ViewStates.Visible;
				_layoutDirection.Visibility = ctrl.Engine.Player.Inventory.Contains(activeObject) ? ViewStates.Gone : (((Thing)activeObject).VectorFromPlayer == null ? ViewStates.Gone : ViewStates.Visible);
				_layoutBottom.Visibility =  (_layoutButtons.Visibility == ViewStates.Visible || _layoutDirection.Visibility == ViewStates.Visible) ? ViewStates.Visible : ViewStates.Gone;

				if (_layoutButtons.Visibility == ViewStates.Visible) {
					_layoutButtons.RemoveAllViews ();
					commands = ((Thing)activeObject).ActiveCommands;
					_layoutButtons.WeightSum = 1;
					if (commands.Count > 0) {
						Button btnView = new Button (Activity.ApplicationContext);
						btnView.Text = commands.Count == 1 ? commands[0].Text : GetString(Resource.String.screen_detail_commands);
						btnView.SetTextColor(Color.White);
						btnView.SetHighlightColor(Color.White);
						btnView.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);
						btnView.LayoutChange += (object sender, View.LayoutChangeEventArgs e) => SetTextScale(btnView);
						btnView.Click += OnButtonClicked;
						// Set size of button
						Android.Views.ViewGroup.LayoutParams lp = new Android.Views.ViewGroup.LayoutParams(Android.Views.ViewGroup.LayoutParams.FillParent, Android.Views.ViewGroup.LayoutParams.FillParent);
						// Add button to view
						_layoutButtons.AddView (btnView, lp);
					}
				}

				if (_layoutDirection.Visibility == ViewStates.Visible) {
					// Draw direction content
					var direction = ((Thing)activeObject).VectorFromPlayer;
					if ( direction != null) {
						_textDirection.Visibility = ViewStates.Visible;
						_imageDirection.Visibility = ViewStates.Visible;
						_textDirection.Text = direction.Distance.BestMeasureAs(DistanceUnit.Meters);
						Bitmap bm;
						_imageDirection.SetImageBitmap(null);
						if (direction.Distance.Value == 0) {
							_imageDirection.SetImageBitmap (BitmapFactory.DecodeResource(Resources, Resource.Drawable.ic_direction_position));
						} else {
							_imageDirection.SetImageBitmap(BitmapArrow.Draw(Math.Min(_imageDirection.Width, _imageDirection.Height), direction.Bearing.Value + Main.GPS.Bearing));
	//							AsyncImageFromDirection.LoadBitmap(_imageDirection, direction.Bearing.Value + Main.GPS.Bearing, 48, 48);
							// TODO:
							// Remove
	//							bm = ctrl.DrawArrow (direction.Bearing.Value + Main.GPS.Bearing);
	//							_imageDirection.SetImageBitmap (bm);
	//							bm = null;
						}
					}
				}

				// Resize scrollview
				_layoutDefault.Invalidate();
			});
		}

		void SetTextScale(Button button)
		{
			// Set default scale
			button.TextScaleX = 1.0f;
			// Calculate new scale of text
			// Found at http://catchthecows.com/?p=72
			Rect bounds = new Rect();
			// ask the paint for the bounding rect if it were to draw this text.
			string text = button.Text;
			int length = button.Text.Length;
			float buttonTextWidth = (float)(button.Right - button.Left - button.TotalPaddingLeft - button.TotalPaddingRight);
			// get bounds of text
			button.Paint.GetTextBounds(text, 0, text.Length, bounds);
			// Calc scale
			float scale = (float)(button.Right - button.Left - button.TotalPaddingLeft - button.TotalPaddingRight) / (bounds.Right - bounds.Left);
			// When scale to small, shorten the string and append ...
			while (scale < 0.6f) {
				length -= 1;
				text = button.Text.Substring(0, length) + "...";
				button.Paint.GetTextBounds(text, 0, text.Length, bounds);
				scale = buttonTextWidth / (bounds.Right - bounds.Left);
			}
			scale = scale > 1.0f ? 1.0f : scale;
			button.TextScaleX = scale;
		}

		#endregion

	}

	#endregion

}

