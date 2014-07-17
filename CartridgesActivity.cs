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
using System.Collections.Specialized;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Vernacular;
using WF.Player.Core;
using WF.Player.Core.Live;
using WF.Player.Preferences;
using Android.Content.PM;
using WF.Player.Location;
using Android.Locations;
using Android.Hardware;

namespace WF.Player
{
	/// <summary>
	/// Cartridges activity to show a list of cartridges.
	/// </summary>
	[Activity (Label = "Cartridges")]
	public class CartridgesActivity : ActionBarActivity
	{
		IMenuItem _menuSearch;
		IMenuItem _menuSettings;
		IMenuItem _menuAbout;

		const string HOCKEYAPP_APPID = "acc974c814cad87cf7e01b8e8c4d5ece";

		#region Android Events

		/// <summary>
		/// Raised, when the activity is created.
		/// </summary>
		/// <param name="bundle">Bundle with cartridge and restore flag.</param>
		protected override void OnCreate (Bundle bundle)
		{
			// Set color schema for activity
			Main.SetTheme(this);

			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Cartridges);

			SupportActionBar.Title = GetString(Resource.String.cartridges_title);
			SupportActionBar.SetDisplayHomeAsUpEnabled(false);

			ListView list = FindViewById<ListView>(Resource.Id.listView);
			list.Adapter = new CartridgesAdapter(this, MainApp.Cartridges);
			list.ItemClick += OnItemClick;
			list.Recycler += OnRecycling;
		}

		/// <summary>
		/// Raised, when one of the list entries is selected.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnItemClick(object sender,AdapterView.ItemClickEventArgs e)
		{
			Intent intent = new Intent(this,typeof(DetailActivity));

			intent.PutExtra("cartridge", e.Position);

			StartActivity(intent);
		}

		/// <summary>
		/// Raises the recycling event.
		/// </summary>
		/// <remarks>
		/// This functions is called, when a list entry is removed from screen.
		/// So it is possible to release the memory on heap, which is occupied
		/// by the bitmap.
		/// </remarks>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void OnRecycling (object sender, AbsListView.RecyclerEventArgs e)
		{
			ImageView iv = e.View.FindViewById<ImageView>(Resource.Id.imageIcon);
			// if there is a image view, than release the memory
			if (iv != null)
				iv.SetImageBitmap(null);
		}

		public override bool OnCreateOptionsMenu(IMenu menu) 
		{
			MenuInflater.Inflate (Resource.Menu.CartridgesMenu, menu);

			_menuSearch = menu.FindItem (Resource.Id.menu_cartridges_search);
			_menuSettings = menu.FindItem (Resource.Id.menu_cartridges_settings);
			_menuAbout = menu.FindItem (Resource.Id.menu_cartridges_about);

			_menuSearch.SetVisible(false);

			return base.OnCreateOptionsMenu(menu);
		}

		/// <summary>
		/// Raised, when an entry of the options menu is selected.
		/// </summary>
		/// <param name="item">Item, which is selected.</param>
		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == 16908332) 
			{
				Finish ();
				return false;
			}

			// This uses the imported MenuItem from action bar
			switch(item.ItemId) {
			case Resource.Id.menu_cartridges_settings:
				Intent intent = new Intent (this, typeof(PreferencesActivity));
				intent.PutExtra ("Cartridges", "Test");
				StartActivity (intent);
				break;
			case Resource.Id.menu_cartridges_about:
				PackageInfo pInfo = this.PackageManager.GetPackageInfo(this.PackageName, 0);
				string version = String.Format("{0}.{1}", pInfo.VersionName, pInfo.VersionCode);
				AlertDialog.Builder builder = new AlertDialog.Builder(this);
				builder.SetTitle(Resource.String.cartridges_about);
				builder.SetMessage(Catalog.Format(GetString(Resource.String.cartridges_about_text), version));
				builder.SetCancelable(true);
				builder.SetNeutralButton(Resource.String.ok, delegate {});
				// TODO: Works this also on devices with API < 14 (Pre 4.0)
				// var test = Build.VERSION.SdkInt;
				// builder.SetNeutralButton(Resource.String.screen_save_before_quit_cancel, delegate { });
				builder.Show();
				break;
			}

			return base.OnOptionsItemSelected(item); 
		}

		protected override void OnPause()
		{
			// Remove from GPS
			Main.GPS.RemoveLocationListener(OnRefreshLocation);

			//Stop Tracking usage in this activity
			HockeyApp.Tracking.StopUsage (this);

			base.OnPause ();
		}

		/// <summary>
		/// Raised, when the activity get the focus.
		/// </summary>
		protected override void OnResume()
		{
			base.OnResume();

			//Register for Crash detection / handling
			// You should do this in your main activity
			HockeyApp.CrashManager.Register (this, HOCKEYAPP_APPID);

			//Start Tracking usage in this activity
			HockeyApp.Tracking.StartUsage (this);

			// Add to GPS
			if (Main.GPS == null)
				// Create object for location listener
				Main.GPS = new GPSListener (GetSystemService (Context.LocationService) as LocationManager, GetSystemService (Context.SensorService) as SensorManager, this.WindowManager);

			Main.GPS.AddLocationListener(OnRefreshLocation);

			Refresh();
		}

		void OnRefreshLocation (object sender, WF.Player.Location.LocationChangedEventArgs e)
		{
		}

		#endregion

		#region Private Functions

		void Refresh()
		{
		}

		#endregion

	}

	/// <summary>
	/// Cartridges adapter.
	/// </summary>
	public class CartridgesAdapter : BaseAdapter
	{
		private Activity context;
		private Cartridges carts;

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.Android.CartridgesAdapter"/> class.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="carts">List of cartridges.</param>
		public CartridgesAdapter(Activity context, Cartridges carts) : base()
		{
			this.context = context;
			this.carts = carts;
			
			carts.CollectionChanged += OnCartridgesChanged;
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
				return carts.Count; 
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
		public Cartridge GetItemAtPosition(int position)
		{
			return carts[position];
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
			//Get our object for this position
			var cart = carts[position];    

			cart.PropertyChanged += OnPropertyChanged;

			// Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			// This gives us some performance gains by not always inflating a new view
			// This will sound familiar to MonoTouch developers with UITableViewCell.DequeueReusableCell()
			var view = (convertView ?? context.LayoutInflater.Inflate(Resource.Layout.CartridgesItem, parent, false)) as LinearLayout;

			// Find references to each subview in the list item's view
			var imageItem = view.FindViewById(Resource.Id.imageViewIcon) as ImageView;
			var textCartName = view.FindViewById(Resource.Id.textCartName) as TextView;
			var textCartVersion = view.FindViewById(Resource.Id.textCartVersion) as TextView;
			var textCartAuthor = view.FindViewById(Resource.Id.textCartAuthor) as TextView;

			// Assign this item's values to the various subviews
			Bitmap bm;
			if (cart.Icon != null) 
			{
				bm = BitmapFactory.DecodeByteArray(cart.Icon.Data, 0, cart.Icon.Data.Length);
			} 
			else if (cart.Poster != null) 
			{
				bm = BitmapFactory.DecodeByteArray(cart.Poster.Data, 0, cart.Poster.Data.Length);
			}
			else
			{
				bm = Bitmap.CreateBitmap(64, 80, Bitmap.Config.Argb8888);
			}

			int maxSize = 960;
			int outWidth;
			int outHeight;
			int inWidth = bm.Width;
			int inHeight = bm.Height;
			if(inWidth/64.0 > inHeight/80.0){
				outWidth = 64;
				outHeight = Convert.ToInt32(Math.Floor((inHeight * 64.0) / inWidth)); 
			} else {
				outHeight = 80;
				outWidth = Convert.ToInt32(Math.Floor((inWidth * 80.0) / inHeight)); 
			}

			bm = Bitmap.CreateScaledBitmap(bm, outWidth, outHeight, true);
			imageItem.SetImageBitmap (bm);

			textCartName.SetText(cart.Name, TextView.BufferType.Normal);
			if (!String.IsNullOrWhiteSpace(cart.Version)) {
				textCartVersion.Visibility = ViewStates.Visible;
				string ver = Catalog.Format(Catalog.GetString("Version {0}"),cart.Version);
				if (!String.IsNullOrWhiteSpace(cart.CreateDate.ToString()))
					ver += " / " + cart.CreateDate.ToString();
				textCartVersion.SetText(ver, TextView.BufferType.Normal);
			}
			else
				textCartVersion.Visibility = ViewStates.Gone;
			if (!String.IsNullOrWhiteSpace(cart.AuthorName) || !String.IsNullOrWhiteSpace(cart.AuthorCompany)) {
				textCartAuthor.Visibility = ViewStates.Visible;
				string author = "";
				if (!String.IsNullOrWhiteSpace(cart.AuthorName))
					author = cart.AuthorName;
				if (!String.IsNullOrWhiteSpace(cart.AuthorCompany))
					author = (!String.IsNullOrWhiteSpace(author) ? author + " / " : "") + cart.AuthorCompany;
				textCartAuthor.SetText(Catalog.Format(Catalog.GetString("By {0}"), author), TextView.BufferType.Normal);
			}
			else
				textCartAuthor.Visibility = ViewStates.Gone;

			// Finally return the view
			return view;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Raised, when a property has changed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			context.RunOnUiThread(NotifyDataSetChanged);
		}

		/// <summary>
		/// Raised, when data of a cartridge changed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		public void OnCartridgesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			context.RunOnUiThread(NotifyDataSetChanged);
		}

		#endregion

	}

}


