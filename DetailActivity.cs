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
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using ActionbarSherlock.App;
using ActionbarSherlock.View;
using Android.Support.V4.App;
//using SherlockActionBar = ActionbarSherlock.App.ActionBar;
//using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using WF.Player.Core;

namespace WF.Player.Android
{
	[Activity (Label = "DetailActivity", Theme = "@style/Theme.Wfplayer")]			
	public class DetailActivity : SherlockActivity, ActionBar.ITabListener
	{
		private Cartridge cart;
		private ActionbarSherlock.View.IMenuItem menuSave;
		private ActionbarSherlock.View.IMenuItem menuDelete;
		private ActionbarSherlock.View.IMenuItem menuStart;
		private ActionbarSherlock.View.IMenuItem menuResume;
//		private WebView webView;

		protected override void OnCreate (Bundle bundle)
		{
			int[] tabs = {
				Resource.String.detail_tab_overview,
				Resource.String.detail_tab_description,
				Resource.String.detail_tab_map,
				Resource.String.detail_tab_logs
			};

			base.OnCreate (bundle);

			// Create your application here
			// Set our view from the "main" layout resource

//			webView = FindViewById<WebView> (Resource.Id.webView);
//			webView.SetWebViewClient (new DetailWebViewClient ());
//			webView.Settings.AllowFileAccess = true;
//			webView.Settings.JavaScriptEnabled = true;
//			webView.Settings.BuiltInZoomControls = false;

			SupportActionBar.NavigationMode = ActionBar.NavigationModeTabs;
			SupportActionBar.SetDisplayHomeAsUpEnabled (true);

			// Get cartridge
			Intent intent = this.Intent;
			cart = ((MainApp)Application).Cartridges [intent.GetIntExtra("cartridge",0)];

			if (cart == null)
				Finish ();

			string filename = System.IO.Path.Combine (global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "WF.Player", "poster.png");

			initDetailInfo ();

//				using (FileStream stream = new FileStream(filename, FileMode.Create))
//				{
//					Bitmap bm = BitmapFactory.DecodeByteArray (cart.Poster.Data,0,cart.Poster.Data.Length);
//					bm.Compress (Bitmap.CompressFormat.Png, 100, stream);
//				} 
//			}
//			else {
//				if (File.Exists (filename))
//					File.Delete(filename);

			for (int i = 0; i < 3; i++) {
				var tab = SupportActionBar.NewTab ();
				tab.SetText (GetString(tabs[i]));
				tab.SetTabListener (this);
				SupportActionBar.AddTab (tab);
			}

			SupportActionBar.Title = cart.Name;

		}

		public override bool OnCreateOptionsMenu(ActionbarSherlock.View.IMenu menu) 
		{
			SupportMenuInflater.Inflate (Resource.Menu.DetailMenu, menu);

			menuSave = menu.FindItem (Resource.Id.menu_detail_save);
			menuDelete = menu.FindItem (Resource.Id.menu_detail_delete);
			menuStart = menu.FindItem (Resource.Id.menu_detail_start);
			menuResume = menu.FindItem (Resource.Id.menu_detail_resume);

			if (cart != null) {
				menuSave.SetVisible (!File.Exists (cart.Filename));
				menuDelete.SetVisible (File.Exists (cart.Filename));
				menuStart.SetVisible (File.Exists (cart.Filename));
				menuResume.SetVisible (File.Exists (cart.SaveFilename));
			}

			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected (ActionbarSherlock.View.IMenuItem item)
		{
			Intent intent;

			if (item.ItemId == Android.Resource.Id.abs__up || item.ItemId == 0) {
				Finish ();
				return false;
			}

			//This uses the imported MenuItem from ActionBarSherlock
			switch(item.ItemId) {
				case Resource.Id.menu_detail_save:
					if (String.IsNullOrEmpty (cart.Filename)) {
						cart.Filename = System.IO.Path.Combine (((MainApp)Application).Path, cart.WGCode);
						var pd = ProgressDialog.Show(this, "Download", "Please Wait...", false);
						((MainApp)Application).Cartridges.DownloadCartridge (cart, ((MainApp)Application).Path, new FileStream (cart.Filename, FileMode.Create));
						pd.Hide ();
					}
					break;
				case Resource.Id.menu_detail_delete:
					if (!String.IsNullOrEmpty(cart.Filename) && File.Exists (cart.Filename))
						File.Delete (cart.Filename);
					if (!String.IsNullOrEmpty(cart.SaveFilename) && File.Exists (cart.SaveFilename))
						File.Delete (cart.SaveFilename);
					if (!String.IsNullOrEmpty(cart.LogFilename) && File.Exists (cart.LogFilename))
						File.Delete (cart.LogFilename);
					break;
				case Resource.Id.menu_detail_start:
					intent = new Intent (this, typeof(ScreenActivity));
					intent.PutExtra ("cartridge", cart.Filename);
					intent.PutExtra ("resume", false);
					StartActivity (intent);
					break;
				case Resource.Id.menu_detail_resume:
					intent = new Intent (this, typeof(ScreenActivity));
					intent.PutExtra ("cartridge", cart.Filename);
					intent.PutExtra ("resume", true);
					StartActivity (intent);
					break;
				default:
					Toast.MakeText (this, "Got click: " + item.ToString (), ToastLength.Long).Show ();
					break;
			}

			menuSave.SetVisible (!File.Exists (cart.Filename));
			menuDelete.SetVisible (File.Exists (cart.Filename));
			menuStart.SetVisible (File.Exists (cart.Filename));
			menuResume.SetVisible (File.Exists (cart.SaveFilename));

			return true;
		}

		public void OnTabReselected (ActionBar.Tab tab, FragmentTransaction transaction)
		{
		}

		public void OnTabSelected (ActionBar.Tab tab, FragmentTransaction transaction)
		{
			// TODO: Set new web page for tab.Text;
//			string baseURL = "file://" + System.IO.Path.Combine (global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "WF.Player", "");

			switch(tab.Position) {
				case 0:
					initDetailInfo ();
//				string html;
//				using (var input = this.Assets.Open("CartridgeDetail.html"))
//				using (StreamReader sr = new System.IO.StreamReader(input)) {
//					html = sr.ReadToEnd ();
//				}
//				;
//				html = html.Replace ("cartridge.Name", cart.Name);
//				html = html.Replace ("cartridge.AuthorName", cart.AuthorName);
//				html = html.Replace ("cartridge.AuthorCompany", cart.AuthorCompany);
//				webView.LoadDataWithBaseURL (baseURL, html, "text/html", "utf-8", null);
					break;
				case 1:
					initDetailDescription ();
					break;
				case 2:
					// TODO: Map
					break;
				case 3:
					initDetailLogs ();
					break;
			}
		}

		public void OnTabUnselected (ActionBar.Tab tab, FragmentTransaction transaction)
		{
		}

		public void OnButtonStartClick(object sender, EventArgs e)
		{
			var toast = Toast.MakeText (this,"Start clicked.",ToastLength.Short);
			toast.Show ();

			Intent intent = new Intent (this, typeof(ScreenActivity));

			intent.PutExtra ("cartridge", intent.GetIntExtra ("cartridge", 0));
			intent.PutExtra ("resume", false);

			StartActivity (intent);
		}

		#region Private Functions

		private void initDetailInfo()
		{
			SetContentView (Resource.Layout.DetailInfo);

			ImageView imageView = FindViewById<ImageView> (Resource.Id.ivPoster);

			if (cart.Poster != null) {
				Bitmap bm = BitmapFactory.DecodeByteArray (cart.Poster.Data, 0, cart.Poster.Data.Length);
				imageView.SetImageBitmap (bm);
			} else {
				imageView.Visibility = ViewStates.Gone;
			}

			List<DetailInfoEntry> entries = new List<DetailInfoEntry>();

			if (!String.IsNullOrEmpty(cart.AuthorName)) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_author_name),cart.AuthorName));
			if (!String.IsNullOrEmpty(cart.AuthorCompany)) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_author_company),cart.AuthorCompany));
			if (cart.CreateDate != null) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_created),cart.CreateDate.ToString ()));
			if (!String.IsNullOrEmpty(cart.Version)) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_version),cart.Version));
			if (cart.UniqueDownloads != 0) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_unique_downloads),cart.Version));
			if (!String.IsNullOrEmpty(cart.ShortDescription)) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_short_description),cart.ShortDescription));

			ListView listView = FindViewById<ListView> (Resource.Id.listView);
			DetailInfoAdapter adapter = new DetailInfoAdapter(this, entries);

			listView.Adapter = adapter;
		}

		private void initDetailDescription()
		{
			SetContentView (Resource.Layout.DetailDescription);
			TextView textView = FindViewById<TextView> (Resource.Id.textView);
			textView.Text = cart.LongDescription;
		}


		private void initDetailLogs()
		{
			SetContentView (Resource.Layout.DetailLogs);
		}

		#endregion

	}

	class DetailWebViewClient : WebViewClient
	{
		public override bool ShouldOverrideUrlLoading (WebView view, string url)
		{
			return true;
		}
	}

	class DetailInfoEntry
	{
		public string Description { get; private set; }
		public string Content { get; private set; }

		public DetailInfoEntry(string description, string content)
		{
			Description = description;
			Content = content;
		}
	}

	class DetailInfoAdapter : BaseAdapter
	{
		private Activity context;
		private List<DetailInfoEntry> entries;

		public DetailInfoAdapter(Activity context, List<DetailInfoEntry> entries) : base()
		{
			this.context = context;
			this.entries = entries;
		}

		public override int Count
		{
			get { return entries.Count; }
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
			var view = (convertView ?? context.LayoutInflater.Inflate(Resource.Layout.DetailInfoItem, parent, false)) as LinearLayout;

			// Find references to each subview in the list item's view
			var tvDescription = view.FindViewById(Resource.Id.tvDescription) as TextView;
			var tvContent = view.FindViewById(Resource.Id.tvContent) as TextView;

			// Assign this item's values to the various subviews
			tvDescription.SetText(entries[position].Description, TextView.BufferType.Normal);
			tvContent.SetText(entries[position].Content, TextView.BufferType.Normal);

			// Finally return the view
			return view;
		}

		public DetailInfoEntry GetItemAtPosition(int position)
		{
			return entries[position];
		}
	}

}



