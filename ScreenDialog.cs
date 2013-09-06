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
	public class ScreenDialog : SherlockFragment
	{
		private Engine engine;
		private string text;
		private Media media;
		private string button1;
		private string button2;
		private Action<string> callback;
		private Input input;
		private ImageView imageView;
		private EditText editInput;
		private Button btnView1;
		private Button btnView2;
		private Button btnInput;
		private LinearLayout layoutDialog;
		private LinearLayout layoutMultipleChoice;
		private LinearLayout layoutInput;

		private ActionbarSherlock.View.IMenuItem menuMap;
		private ActionbarSherlock.View.IMenuItem menuCommands;

		private TextView textView;

		public ScreenDialog(Engine engine, string text, Media media, string button1, string button2, Action<string> callback)
		{
			this.engine = engine;
			this.text = text;
			this.media = media;
			this.button1 = button1;
			this.button2 = button2;
			this.callback = callback;
			this.input = null;
		}

		public ScreenDialog(Engine engine, Input input)
		{
			this.engine = engine;
			this.input = input;
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

			var view = inflater.Inflate(Resource.Layout.ScreenDialog, container, false);

			imageView = view.FindViewById<ImageView> (Resource.Id.imageView);
			textView = view.FindViewById<TextView> (Resource.Id.textView);
			layoutDialog = view.FindViewById<LinearLayout> (Resource.Id.layoutDialog);
			layoutMultipleChoice = view.FindViewById<LinearLayout> (Resource.Id.layoutMultipleChoice);
			layoutInput = view.FindViewById<LinearLayout> (Resource.Id.layoutInput);

			if (input == null) {
				// Normal dialog
				layoutDialog.Visibility = ViewStates.Visible;
				layoutMultipleChoice.Visibility = ViewStates.Gone;
				layoutInput.Visibility = ViewStates.Gone;

				btnView1 = view.FindViewById<Button> (Resource.Id.button1);
				btnView2 = view.FindViewById<Button> (Resource.Id.button2);

				btnView1.Click += OnButtonClicked;
				btnView2.Click += OnButtonClicked;
			} else {
				text = input.Text;
				media = input.Image;
				if (input.InputType == InputTypes.MultipleChoice) {
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

					btnInput.Click += OnInputClicked;

					editInput.Text = "";
					btnInput.Text = context.GetString (Resource.String.ok);
				}
			}

			return view;
		}

		public override void OnResume()
		{
			base.OnResume();

			this.SherlockActivity.SupportActionBar.Title = "";
			this.SherlockActivity.SupportActionBar.SetDisplayShowHomeEnabled(false);

			textView.Text = text;

			if (media != null) {
				Bitmap bm = BitmapFactory.DecodeByteArray (media.Data, 0, media.Data.Length);
				imageView.SetImageBitmap (bm);
				imageView.Visibility = ViewStates.Visible;
			} else {
				imageView.Visibility = ViewStates.Gone;
			}

			if (input == null) {
				// Normal dialog
				if (!String.IsNullOrEmpty (button1)) {
					btnView1.Visibility = ViewStates.Visible;
					btnView1.Text = button1;
				} else
					btnView1.Visibility = ViewStates.Gone;
				if (!String.IsNullOrEmpty (button2)) {
					btnView2.Visibility = ViewStates.Visible;
					btnView2.Text = button2;
				} else
					btnView2.Visibility = ViewStates.Gone;
			} else {
				if (input.InputType == InputTypes.MultipleChoice) {
					// Multiple choice dialog
					layoutMultipleChoice.RemoveAllViews ();
					foreach (string s in input.Choices) {
						Button btnView = new Button (Activity.ApplicationContext);
						btnView.Text = s;
						btnView.Click += OnChoiceClicked;
						layoutMultipleChoice.AddView (btnView);
					}
				} else {
					// Input dialog
					// ToDo: Clear text field editInput
				}
			}
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			//TODO: Store instance state data
		}

		public void OnButtonClicked(object sender, EventArgs e)
		{
			// Remove dialog from screen
			((ScreenActivity)this.Activity).RemoveScreen (ScreenType.DialogScreen);

			// Execute callback if there is one
			if (sender is Button && callback != null) {
				string btnText = sender.Equals(btnView1) ? "Button1" : "Button2";
				callback (btnText);
			}
		}

		public void OnChoiceClicked(object sender, EventArgs e)
		{
			if (input != null) {
				string result = ((Button)sender).Text;
				input.Callback (result);
			}
		}

		public void OnInputClicked(object sender, EventArgs e)
		{
			if (input != null) {
				string result = editInput.Text;
				input.Callback (result);
			}
		}

		#region Private Functions

		#endregion

	}
}

