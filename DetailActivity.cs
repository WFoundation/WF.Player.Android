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
using Android.Support.V4.App;
using Android.Support.V7.App;
using Vernacular;
using WF.Player.Core;
using WF.Player.Types;
using WF.Player.Game;
using Android.Graphics.Drawables;

namespace WF.Player
{
	[Activity (Label = "Cartridge Details")]	
	[MetaData ("android.support.UI_OPTIONS", Value = "splitActionBarWhenNarrow")]		
	public class DetailActivity : ActionBarActivity, global::Android.Support.V7.App.ActionBar.ITabListener
	{
		Cartridge _cart;
		IMenuItem _menuSave;
		IMenuItem _menuDelete;
		IMenuItem _menuNavigate;
		IMenuItem _menuStart;
		IMenuItem _menuResume;
//		private WebView webView;

		#region Android Event Handlers


		protected override void OnCreate (Bundle bundle)
		{
			// Set color schema for activity
			Main.SetTheme(this);

			int[] tabs = {
				Resource.String.detail_tab_overview,
				Resource.String.detail_tab_description,
				Resource.String.detail_tab_history,
				Resource.String.detail_tab_logs
			};

			base.OnCreate (bundle);

			SupportActionBar.NavigationMode = global::Android.Support.V7.App.ActionBar.NavigationModeTabs;
			SupportActionBar.SetDisplayHomeAsUpEnabled (true);

			// Get cartridge
			Intent intent = this.Intent;
			_cart = MainApp.Cartridges [intent.GetIntExtra("cartridge",0)];

			if (_cart == null)
				Finish ();

			string filename = System.IO.Path.Combine (global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "WF.Player", "poster.png");

			InitDetailInfo ();

//				using (FileStream stream = new FileStream(filename, FileMode.Create))
//				{
//					Bitmap bm = BitmapFactory.DecodeByteArray (cart.Poster.Data,0,cart.Poster.Data.Length);
//					bm.Compress (Bitmap.CompressFormat.Png, 100, stream);
//				} 
//			}
//			else {
//				if (File.Exists (filename))
//					File.Delete(filename);

			for (int i = 0; i < 4; i++) {
				var tab = SupportActionBar.NewTab ();
				tab.SetText (GetString(tabs[i]));
				tab.SetTabListener (this);
				SupportActionBar.AddTab (tab);
			}

			SupportActionBar.Title = _cart.Name;

		}

		public override bool OnCreateOptionsMenu(IMenu menu) 
		{
			MenuInflater.Inflate (Resource.Menu.DetailMenu, menu);

			_menuSave = menu.FindItem (Resource.Id.menu_detail_save);
			_menuDelete = menu.FindItem (Resource.Id.menu_detail_delete);
			_menuNavigate = menu.FindItem (Resource.Id.menu_detail_navigate);
			_menuResume = menu.FindItem (Resource.Id.menu_detail_resume);
			_menuStart = menu.FindItem (Resource.Id.menu_detail_start);

			if (_cart != null) {
				_menuSave.SetVisible (!File.Exists (_cart.Filename));
				_menuDelete.SetVisible (File.Exists (_cart.Filename));
				if (_cart.StartingLocationLatitude != 360.0 && _cart.StartingLocationLongitude != 360.0 && HasRouting())
					_menuNavigate.SetEnabled(true);
				else 
					_menuNavigate.SetEnabled(false);
				_menuNavigate.Icon.SetAlpha(_menuNavigate.IsEnabled ? 204 : 96);
				_menuResume.SetVisible (true);
				_menuResume.SetEnabled(File.Exists (_cart.SaveFilename));
				_menuResume.Icon.SetAlpha(_menuResume.IsEnabled ? 204 : 96);
				_menuStart.SetVisible (true);
				_menuStart.SetEnabled(File.Exists (_cart.Filename));
				_menuStart.Icon.SetAlpha(_menuStart.IsEnabled ? 204 : 96);
			}

			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			Intent intent;

			if (item.ItemId == 16908332) {
				Finish ();
				return false;
			}

			// This uses the imported MenuItem from action bar
			switch(item.ItemId) {
				case Resource.Id.menu_detail_save:
					if (String.IsNullOrEmpty (_cart.Filename)) {
					_cart.Filename = System.IO.Path.Combine (Main.Path, _cart.WGCode);
						var pd = ProgressDialog.Show(this, "Download", "Please Wait...", false);
					MainApp.Cartridges.DownloadCartridge (_cart, Main.Path, new FileStream (_cart.Filename, FileMode.Create));
						pd.Hide ();
					}
					break;
				case Resource.Id.menu_detail_delete:
					AlertDialog.Builder builder = new AlertDialog.Builder(this);
					builder.SetTitle(Catalog.GetString("Delete"));
					builder.SetMessage(Catalog.Format(Catalog.GetString("Would you delete the cartridge {0} and all log/save files?"), _cart.Name));
					builder.SetCancelable(true);
					builder.SetPositiveButton(Catalog.GetString("Yes"), delegate { 
						if (!String.IsNullOrEmpty(_cart.Filename) && File.Exists (_cart.Filename))
							File.Delete (_cart.Filename);
						if (!String.IsNullOrEmpty(_cart.SaveFilename) && File.Exists (_cart.SaveFilename))
							File.Delete (_cart.SaveFilename);
						if (!String.IsNullOrEmpty(_cart.LogFilename) && File.Exists (_cart.LogFilename))
							File.Delete (_cart.LogFilename);
					});
					// TODO: Works this also on devices with API < 14 (Pre 4.0)
					// var test = Build.VERSION.SdkInt;
					// builder.SetNeutralButton(Resource.String.screen_save_before_quit_cancel, delegate { });
					builder.SetNegativeButton(Catalog.GetString("No"), delegate { });
					builder.Show();
					break;
				case Resource.Id.menu_detail_navigate:
					if (_cart.StartingLocationLatitude != 360.0 && _cart.StartingLocationLongitude != 360.0)
						StartRouting(_cart.StartingLocationLatitude, _cart.StartingLocationLongitude);
					break;
				case Resource.Id.menu_detail_start:
					intent = new Intent (this, typeof(GameController));
					intent.PutExtra ("cartridge", _cart.Filename);
					intent.PutExtra ("restore", false);
					try {
						Start(intent);
					} 
					catch (Exception ex) {
						AlertDialog.Builder adb = new AlertDialog.Builder(this);
						adb.SetTitle(Catalog.GetString("Error"));
						adb.SetMessage(ex.Message);
						adb.SetPositiveButton(Catalog.GetString("Ok"),  (sender, args) =>
							{
								// Do something when this button is clicked.
							});
						adb.Show();
					}
					break;
				case Resource.Id.menu_detail_resume:
					intent = new Intent (this, typeof(GameController));
					intent.PutExtra ("cartridge", _cart.Filename);
					intent.PutExtra ("restore", true);
					try {
						Start(intent);
					} 
					catch (Exception ex) {
						AlertDialog.Builder adb = new AlertDialog.Builder(this);
						adb.SetTitle(Catalog.GetString("Error"));
						adb.SetMessage(ex.Message);
						adb.SetPositiveButton(Catalog.GetString("Ok"),  (sender, args) =>
							{
								// Do something when this button is clicked.
							});
						adb.Show();
					}
					break;
				default:
					Toast.MakeText (this, "Got click: " + item.ToString (), ToastLength.Long).Show ();
					break;
			}

			_menuSave.SetVisible (!File.Exists (_cart.Filename));
			_menuDelete.SetVisible (File.Exists (_cart.Filename));
			if (_cart.StartingLocationLatitude != 360.0 && _cart.StartingLocationLongitude != 360.0 && HasRouting()) {
				_menuNavigate.SetEnabled(true, this, Resource.Id.menu_detail_navigate);
			} else {
				_menuNavigate.SetEnabled(false, this, Resource.Id.menu_detail_navigate);
			}
			_menuNavigate.Icon.SetAlpha(_menuNavigate.IsEnabled ? 204 : 96);
			_menuResume.SetVisible (true);
			_menuResume.SetEnabled(File.Exists (_cart.SaveFilename), this, Resource.Id.menu_detail_resume);
			_menuResume.Icon.SetAlpha(_menuResume.IsEnabled ? 204 : 96);
			_menuStart.SetVisible (true);
			_menuStart.SetEnabled(File.Exists (_cart.Filename));
			_menuStart.Icon.SetAlpha(_menuStart.IsEnabled ? 204 : 96);

			return true;
		}

		public void OnTabReselected (global::Android.Support.V7.App.ActionBar.Tab tab, global::Android.Support.V4.App.FragmentTransaction transaction)
		{
		}

		public void OnTabSelected (global::Android.Support.V7.App.ActionBar.Tab tab, global::Android.Support.V4.App.FragmentTransaction transaction)
		{
			// TODO: Set new web page for tab.Text;
//			string baseURL = "file://" + System.IO.Path.Combine (global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "WF.Player", "");

			switch(tab.Position) 
			{
				case 0:
					InitDetailInfo();
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
					InitDetailDescription();
					break;
				case 2:
					InitDetailHistory();
					break;
				case 3:
					InitDetailLogs();
					break;
			}
		}

		public void OnTabUnselected (global::Android.Support.V7.App.ActionBar.Tab tab, global::Android.Support.V4.App.FragmentTransaction transaction)
		{
		}

		/// <summary>
		/// Raised, when the activity lost the focus.
		/// </summary>
		protected override void OnPause()
		{
			base.OnPause ();

			// Remove from GPS
			Main.GPS.RemoveLocationListener(OnRefreshLocation);
		}

		/// <summary>
		/// Raised, when the activity get the focus.
		/// </summary>
		protected override void OnResume()
		{
			base.OnResume();

			// Add to GPS
			Main.GPS.AddLocationListener(OnRefreshLocation);

			Refresh();
		}

		void OnRefreshLocation (object sender, WF.Player.Location.LocationChangedEventArgs e)
		{
		}

		#endregion

		#region Private Functions

		void InitDetailInfo()
		{
			SetContentView (Resource.Layout.DetailInfo);

			ImageView imageView = FindViewById<ImageView> (Resource.Id.ivPoster);

			if (_cart.Poster != null) {
				Bitmap bm = BitmapFactory.DecodeByteArray (_cart.Poster.Data, 0, _cart.Poster.Data.Length);
				imageView.SetImageBitmap (bm);
			} else {
				imageView.Visibility = ViewStates.Gone;
			}

			List<DetailInfoEntry> entries = new List<DetailInfoEntry>();

			if (!String.IsNullOrEmpty(_cart.AuthorName)) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_author_name),_cart.AuthorName));
			if (!String.IsNullOrEmpty(_cart.AuthorCompany)) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_author_company),_cart.AuthorCompany));
			if (_cart.CreateDate != null) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_created),_cart.CreateDate.ToString ()));
			if (!String.IsNullOrEmpty(_cart.Version)) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_version),_cart.Version));
			if (_cart.UniqueDownloads != 0) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_unique_downloads),_cart.Version));
			if (!String.IsNullOrEmpty(_cart.ShortDescription)) entries.Add (new DetailInfoEntry(GetString (Resource.String.detail_short_description),_cart.ShortDescription));
			// Start point and description
			string text = GetString (Resource.String.detail_starting_location);
			string description;
			if (_cart.StartingLocationLatitude == 360.0 && _cart.StartingLocationLongitude == 360.0)
				description = GetString (Resource.String.detail_starting_play_anywhare);
			else 
				description = Location.Converters.CoordinatToString(_cart.StartingLocationLatitude, _cart.StartingLocationLongitude, WF.Player.Location.GPSFormat.DecimalMinutes, true);
			description += System.Environment.NewLine;
			description += _cart.StartingDescription;
			entries.Add (new DetailInfoEntry(text, description));

			ListView listView = FindViewById<ListView> (Resource.Id.listView);
			DetailInfoAdapter adapter = new DetailInfoAdapter(this, entries);

			if (entries.Count == 0 && _cart.Poster == null) {
				LinearLayout ll = FindViewById<LinearLayout> (Resource.Id.layoutDetailInfo);
				ll.RemoveAllViews();
				TextView tv = new TextView(this);
				tv.Text = Catalog.GetString("No info availible");
				ll.AddView(tv);
			} else
				listView.Adapter = adapter;
		}

		void InitDetailDescription()
		{
			SetContentView (Resource.Layout.DetailDescription);
			TextView textView = FindViewById<TextView> (Resource.Id.textView);
			textView.Gravity = Main.Prefs.TextAlignment.ToSystem();
			textView.SetTextSize(global::Android.Util.ComplexUnitType.Sp, (float)Main.Prefs.TextSize);
			if (String.IsNullOrEmpty(_cart.LongDescription))
				textView.Text = Catalog.GetString("No description availible");
			else
				textView.Text = _cart.LongDescription;
		}


		void InitDetailHistory()
		{
			SetContentView (Resource.Layout.DetailHistory);
		}

		void InitDetailLogs()
		{
			SetContentView (Resource.Layout.DetailLogs);
		}

		void Start(Intent i)
		{
			try {
				StartActivity (i);
			}
			catch (Exception e)
			{
			}
		}

		void Refresh()
		{
		}

		bool HasRouting()
		{
			Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("google.navigation:q=0,0"));

			return intent.ResolveActivity(this.PackageManager) != null;
		}

		void StartRouting(double lat, double lon)
		{
			// TODO: Conversion from lat/lon to string should be on all devices with point "." instead of ","
			string uri = String.Format("google.navigation:q={0},{1}", lat.ToString().Replace(",", "."), lon.ToString().Replace(",", "."));
			Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(uri));
			if (intent.ResolveActivity(this.PackageManager) != null) {
				this.StartActivity(intent);
			}
		}

		#endregion

	}

	/// <summary>
	/// Detail web view client.
	/// </summary>
	class DetailWebViewClient : WebViewClient
	{
		public override bool ShouldOverrideUrlLoading(WebView view, string url)
		{
			return true;
		}
	}

	/// <summary>
	/// Detail info entry.
	/// </summary>
	class DetailInfoEntry
	{
		public string Description { get; private set; }
		public string Content { get; private set; }

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.Android.DetailInfoEntry"/> class.
		/// </summary>
		/// <param name="description">Description.</param>
		/// <param name="content">Content.</param>
		public DetailInfoEntry(string description, string content)
		{
			Description = description;
			Content = content;
		}

		#endregion

	}

	/// <summary>
	/// Detail info adapter for cartridge details list.
	/// </summary>
	class DetailInfoAdapter : BaseAdapter
	{
		private Activity context;
		private List<DetailInfoEntry> entries;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.Android.DetailInfoAdapter"/> class.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="entries">Entries.</param>
		public DetailInfoAdapter(Activity context, List<DetailInfoEntry> entries) : base()
		{
			this.context = context;
			this.entries = entries;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of entries in list.
		/// </summary>
		/// <value>The count.</value>
		public override int Count
		{
			get 
			{ 
				return entries.Count; 
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the item at given position.
		/// </summary>
		/// <returns>The item.</returns>
		/// <param name="position">Position.</param>
		public override Java.Lang.Object GetItem(int position)
		{
			return position;
		}

		/// <summary>
		/// Gets the item at position.
		/// </summary>
		/// <returns>The item at position.</returns>
		/// <param name="position">Position.</param>
		public DetailInfoEntry GetItemAtPosition(int position)
		{
			return entries[position];
		}

		/// <summary>
		/// Gets the item id.
		/// </summary>
		/// <returns>The item identifier.</returns>
		/// <param name="position">Position.</param>
		public override long GetItemId(int position)
		{
			return position;
		}

		/// <summary>
		/// Gets the view for this position.
		/// </summary>
		/// <returns>The view.</returns>
		/// <param name="position">Position.</param>
		/// <param name="convertView">Convert view.</param>
		/// <param name="parent">Parent.</param>
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
			tvDescription.SetText(Catalog.GetString(entries[position].Description), TextView.BufferType.Normal);
			tvContent.SetText(entries[position].Content, TextView.BufferType.Normal);

			// Finally return the view
			return view;
		}

		#endregion

	}

	/// <summary>
	/// Extensions for IMenuItem to change color of disabled icons.
	/// </summary>
	/// <remarks>
	/// Found at http://blog.ostebaronen.dk/2014/01/disabling-actionbar-menu-items.html.
	/// </remarks>
	public static class MenuItemExtensions
	{
		public static IMenuItem SetEnabled(this IMenuItem item, bool enabled, Context context, int iconId)
		{
			return item.SetEnabled(enabled, context, iconId, Color.Gray);
		}

		public static IMenuItem SetEnabled(this IMenuItem item, bool enabled, Context context, int iconId, Color disabledColor)
		{
			Drawable resIcon;

			try {
				resIcon = context.Resources.GetDrawable(iconId);
			} catch {
//				resIcon = context.Resources.
			}

//			if (!enabled)
//				resIcon.Mutate().SetColorFilter(disabledColor, PorterDuff.Mode.SrcIn);
//
//			item.SetEnabled(enabled);
//			item.SetIcon(resIcon);
			return item;
		}
	}
}



