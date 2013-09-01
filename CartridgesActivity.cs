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
using ActionbarSherlock.App;
using WF.Player.Core;

namespace WF.Player.Android
{
	[Activity (Label = "WF.Player.Android", Theme = "@style/Theme.Wfplayer")]
	public class CartridgesActivity : SherlockActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Cartridges);

			SupportActionBar.Title = GetString(Resource.String.cartridges_title);
			SupportActionBar.SetDisplayHomeAsUpEnabled (true);

			ListView list = FindViewById<ListView> (Resource.Id.listView);
			list.Adapter = new CartridgesAdapter (this, ((MainApp)this.Application).Cartridges);
			list.ItemClick += OnItemClick;
		}

		public void OnItemClick(object sender,AdapterView.ItemClickEventArgs e)
		{
			Intent intent = new Intent (this,typeof(DetailActivity));

			intent.PutExtra ("cartridge", e.Position);

			StartActivity (intent);
		}

//		public override bool OnOptionsItemSelected(IMenuItem item)
//		{
//			switch (item.ItemId)
//			{
//				case Resource.Id.menu_search: 
//				var intent = new Intent(this, typeof (SearchActivity));
//				StartActivity(intent);
//				return true;
//			}
//
//			return base.OnOptionsItemSelected(item); 
//		}
	}

	public class CartridgesAdapter : BaseAdapter
	{
		private Activity context;
		private Cartridges carts;

		public CartridgesAdapter(Activity context, Cartridges carts) : base()
		{
			this.context = context;
			this.carts = carts;
			
			carts.CollectionChanged += OnCartridgesChanged;
		}

		public override int Count
		{
			get { return carts.Count; }
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
			//Get our object for this position
			var cart = carts[position];    

			cart.PropertyChanged += OnPropertyChanged;

			// Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			// This gives us some performance gains by not always inflating a new view
			// This will sound familiar to MonoTouch developers with UITableViewCell.DequeueReusableCell()
			var view = (convertView ?? context.LayoutInflater.Inflate(Resource.Layout.CartridgesItem, parent, false)) as LinearLayout;

			// Find references to each subview in the list item's view
			var imageItem = view.FindViewById(Resource.Id.imageViewIcon) as ImageView;
			var textTop = view.FindViewById(Resource.Id.textViewTop) as TextView;
			var textBottom = view.FindViewById(Resource.Id.textViewBottom) as TextView;

			// Assign this item's values to the various subviews
			if (cart.Icon != null)
			{
				Bitmap bm = BitmapFactory.DecodeByteArray (cart.Icon.Data,0,cart.Icon.Data.Length);
				imageItem.SetImageBitmap (bm);
			}
			textTop.SetText(cart.Name, TextView.BufferType.Normal);
			textBottom.SetText(cart.AuthorName, TextView.BufferType.Normal);

			// Finally return the view
			return view;
		}

		public Cartridge GetItemAtPosition(int position)
		{
			return carts[position];
		}

		public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			context.RunOnUiThread (NotifyDataSetChanged);
		}

		public void OnCartridgesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			context.RunOnUiThread (NotifyDataSetChanged);
		}
	}
}


