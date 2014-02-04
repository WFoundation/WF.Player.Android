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
using Android.Views.InputMethods;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using WF.Player.Core;
using WF.Player.Core.Engines;

namespace WF.Player.Android
{
	#region ScreenDialog

	public class ScreenDialog : global::Android.Support.V4.App.Fragment
	{
		ScreenController ctrl;
		MessageBox messageBox;
		Input input;
		ImageView imageView;
		EditText editInput;
		Button btnView1;
		Button btnView2;
		Button btnInput;
		LinearLayout layoutDialog;
		LinearLayout layoutMultipleChoice;
		LinearLayout layoutInput;
		TextView textDescription;

		#region Constructor

		public ScreenDialog(MessageBox messageBox)
		{
			this.messageBox = messageBox;
			this.input = null;
		}

		public ScreenDialog(Input input)
		{
			this.messageBox = null;
			this.input = input;
		}

		#endregion

		#region Android Event Handlers

		public void OnButtonClicked(object sender, EventArgs e)
		{
			ctrl.Feedback();

			// Remove dialog from screen
			ctrl.RemoveScreen (ScreenType.Dialog);

			// Execute callback if there is one
			if (sender is Button) {
				messageBox.GiveResult (sender.Equals(btnView1) ? MessageBoxResult.FirstButton : MessageBoxResult.SecondButton);
			}
		}

		public void OnChoiceClicked(object sender, EventArgs e)
		{
			ctrl.Feedback();

			// Remove dialog from screen
			ctrl.RemoveScreen (ScreenType.Dialog);

			if (input != null) {
				string result = ((Button)sender).Text;
				input.GiveResult (result);
			}
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			// Save ScreenController for later use
			ctrl = ((ScreenController)this.Activity);

			if (container == null)
				return null;

			var view = inflater.Inflate(Resource.Layout.ScreenDialog, container, false);

			imageView = view.FindViewById<ImageView> (Resource.Id.imageView);
			textDescription = view.FindViewById<TextView> (Resource.Id.textDescription);
			layoutDialog = view.FindViewById<LinearLayout> (Resource.Id.layoutDialog);
			layoutMultipleChoice = view.FindViewById<LinearLayout> (Resource.Id.layoutMultipleChoice);
			layoutInput = view.FindViewById<LinearLayout> (Resource.Id.layoutInput);

			if (input == null) {
				// Normal dialog
				layoutDialog.Visibility = ViewStates.Visible;
				layoutMultipleChoice.Visibility = ViewStates.Gone;
				layoutInput.Visibility = ViewStates.Gone;

				btnView1 = view.FindViewById<Button> (Resource.Id.button1);
				btnView1.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);
				btnView1.Click += OnButtonClicked;

				btnView2 = view.FindViewById<Button> (Resource.Id.button2);
				btnView2.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);
				btnView2.Click += OnButtonClicked;
			} else {
				if (input.InputType == InputType.MultipleChoice) {
					// Multiple choice dialog
					layoutDialog.Visibility = ViewStates.Gone;
					layoutMultipleChoice.Visibility = ViewStates.Visible;
					layoutInput.Visibility = ViewStates.Gone;
				} else {
					// Input dialog
					layoutDialog.Visibility = ViewStates.Gone;
					layoutMultipleChoice.Visibility = ViewStates.Gone;
					layoutInput.Visibility = ViewStates.Visible;

					editInput = view.FindViewById<EditText> (Resource.Id.editInput);

					btnInput = view.FindViewById<Button> (Resource.Id.buttonInput);
					btnInput.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);
					btnInput.Click += OnInputClicked;

					editInput.Text = "";
					editInput.EditorAction += HandleEditorAction;  

					btnInput.Text = ctrl.Resources.GetString(Resource.String.done);
				}
			}

			return view;
		}

		/// <summary>
		/// Raised when the fragment is destroyed, so free references to other UI elements.
		/// </summary>
		public override void OnDestroyView()
		{
			base.OnDestroyView();

			imageView.SetImageBitmap(null);
			imageView = null;
			messageBox = null;
			input = null;
			editInput = null;
			btnView1 = null;
			btnView2 = null;
			btnInput = null;
			layoutDialog = null;
			layoutMultipleChoice = null;
			layoutInput = null;
			textDescription = null;
			ctrl = null;
		}

		public void OnInputClicked(object sender, EventArgs e)
		{
			ctrl.Feedback();

			// Remove keyboard
			new Handler().Post(delegate
				{
					var view = ctrl.CurrentFocus;
					if (view != null)
					{
						InputMethodManager manager = (InputMethodManager)ctrl.GetSystemService(Context.InputMethodService);
						manager.HideSoftInputFromWindow(view.WindowToken, 0);
					}
				});

			// Remove dialog from screen
			ctrl.RemoveScreen (ScreenType.Dialog);

			if (input != null) {
				string result = editInput.Text;
				input.GiveResult (result);
			}
		}

		public override void OnResume()
		{
			base.OnResume();

			ctrl.SupportActionBar.Title = "";
			ctrl.SupportActionBar.SetDisplayShowHomeEnabled(false);

			Refresh();
		}

		#endregion

		#region Private Functions

		void Refresh()
		{
			if (input == null) {
				// Normal dialog
				// TODO: HTML
				textDescription.Text = messageBox.Text; // Html.FromHtml(messageBox.HTML.Replace("&lt;BR&gt;", "<br>").Replace("<br>\n", "<br>").Replace("\n", "<br>"));
				textDescription.Gravity = PrefHelper.TextAlignment;
				textDescription.SetTextSize(global::Android.Util.ComplexUnitType.Sp, PrefHelper.TextSize);
				if (messageBox.Image != null) {
					imageView.SetImageBitmap(null);
					using (Bitmap bm = ctrl.ConvertMediaToBitmap(messageBox.Image)) {
						imageView.SetImageBitmap (bm);
					}
					imageView.Visibility = ViewStates.Visible;
				} else {
					imageView.Visibility = ViewStates.Gone;
				}
				if (!String.IsNullOrEmpty (messageBox.FirstButtonLabel)) {
					btnView1.Visibility = ViewStates.Visible;
					btnView1.Text = messageBox.FirstButtonLabel;
				} else
					btnView1.Visibility = ViewStates.Gone;
				if (!String.IsNullOrEmpty (messageBox.SecondButtonLabel)) {
					btnView2.Visibility = ViewStates.Visible;
					btnView2.Text = messageBox.SecondButtonLabel;
				} else
					btnView2.Visibility = ViewStates.Gone;
			} else {
				// TODO: HTML
				textDescription.Text = input.Text; // Html.FromHtml(input.HTML.Replace("&lt;BR&gt;", "<br>").Replace("<br>\n", "<br>").Replace("\n", "<br>"));
				textDescription.Gravity = PrefHelper.TextAlignment;
				textDescription.SetTextSize(global::Android.Util.ComplexUnitType.Sp, PrefHelper.TextSize);
				if (input.Image != null) {
					imageView.SetImageBitmap(null);
					using (Bitmap bm = ctrl.ConvertMediaToBitmap(input.Image)) {
						imageView.SetImageBitmap (bm);
					}
					imageView.Visibility = ViewStates.Visible;
				} else {
					imageView.Visibility = ViewStates.Gone;
				}
				if (input.InputType == InputType.MultipleChoice) {
					// Multiple choice dialog
					layoutMultipleChoice.RemoveAllViews ();
					foreach (string s in input.Choices) {
						Button btnView = new Button (Activity.ApplicationContext);
						btnView.SetBackgroundResource(Resource.Drawable.apptheme_btn_default_holo_light);
						btnView.SetTextColor(Color.Black);
						btnView.SetHighlightColor(Color.Black);
						btnView.Text = s;
						btnView.Click += OnChoiceClicked;
						layoutMultipleChoice.AddView (btnView);
					}
				} else {
					// Input dialog
					// ToDo: Clear text field editInput
					if (PrefHelper.InputFocus) {
						editInput.RequestFocus();
						((InputMethodManager)ctrl.GetSystemService(Context.InputMethodService)).ShowSoftInput(editInput, ShowFlags.Implicit);
					}
				}
			}
		}

		// Add this method to your class
		private void HandleEditorAction(object sender, EditText.EditorActionEventArgs e)
		{
			e.Handled = false;
			if (e.ActionId == global::Android.Views.InputMethods.ImeAction.Done || e.Event.UnicodeChar == 10)
			{
//				InputMethodManager inputMethodManager = Application.GetSystemService(Context.InputMethodService) as InputMethodManager;
//				inputMethodManager.HideSoftInputFromWindow(editInput.WindowToken, HideSoftInputFlags.None);
				OnInputClicked (sender, e);
				e.Handled = true;   
			}
		}

		#endregion

	}

	#endregion
}

