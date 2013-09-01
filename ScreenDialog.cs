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
	public class ScreenDialog : SherlockFragment
	{
		private Engine engine;
		private string text;
		private Media media;
		private string button1;
		private string button2;
		private Action<string> callback;
		private ImageView imageView;
		private Button btnView1;
		private Button btnView2;
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

			btnView1 = view.FindViewById<Button> (Resource.Id.button1);
			btnView2 = view.FindViewById<Button> (Resource.Id.button2);

			btnView1.Click += OnButtonClicked;
			btnView2.Click += OnButtonClicked;

			return view;
		}

		public void OnButtonClicked(object sender, EventArgs e)
		{
			if (sender is Button && callback != null) {
				string btnText = ((Button)sender).Text;
				callback (btnText);
			}

			FragmentManager.PopBackStackImmediate ();
		}

		public override void OnResume()
		{
			base.OnResume();

			//TODO: Load data
			textView.Text = text;

			if (media != null) {
				Bitmap bm = BitmapFactory.DecodeByteArray (media.Data, 0, media.Data.Length);
				imageView.SetImageBitmap (bm);
				imageView.Visibility = ViewStates.Visible;
			} else {
				imageView.Visibility = ViewStates.Gone;
			}

			if (!String.IsNullOrEmpty (button1)) {
				btnView1.Visibility = ViewStates.Visible;
				btnView1.Text = button1;
			} else
				btnView1.Visibility = ViewStates.Gone;

			if (!String.IsNullOrEmpty (button2)) {
				btnView2.Visibility = ViewStates.Visible;
				btnView2.Text = button1;
			} else
				btnView2.Visibility = ViewStates.Gone;

		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			//TODO: Store instance state data
		}

		#region Private Functions

		#endregion

	}
}

