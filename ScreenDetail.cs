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
	public class ScreenDetail : SherlockFragment
	{
		private Engine engine;
		private UIObject obj;
		private ImageView imageView;
		private ActionbarSherlock.View.IMenuItem menuMap;
		private ActionbarSherlock.View.IMenuItem menuCommands;

		private TextView textView;

		public ScreenDetail(Engine engine, UIObject obj)
		{
			this.engine = engine;
			this.obj = obj;
			if (this.obj != null)
				this.obj.PropertyChanged += OnPropertyChanged;
		}

		public ScreenDetail(Engine engine, int objIndex)
		{
			this.engine = engine;
			if (engine.IsUIObject (engine.GetObject (objIndex))) {
				obj = (UIObject)engine.GetObject (objIndex);
				obj.PropertyChanged += OnPropertyChanged; 
			}
			else
				this.obj = null;
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

			var view = inflater.Inflate(Resource.Layout.ScreenDetail, container, false);

			imageView = view.FindViewById<ImageView> (Resource.Id.imageView);
			textView = view.FindViewById<TextView> (Resource.Id.textView);

			return view;
		}

		public override void OnCreateOptionsMenu(ActionbarSherlock.View.IMenu menu, ActionbarSherlock.View.MenuInflater inflater) 
		{
			inflater.Inflate (Resource.Menu.ScreenDetailMenu, menu);

			menuMap = menu.FindItem (Resource.Id.menu_screen_detail_map);
			menuCommands = menu.FindItem (Resource.Id.menu_screen_detail_commands);

			if (obj != null && obj is Thing) {
				menuMap.SetVisible (false);
				menuCommands.SetVisible (((Thing)obj).ActiveCommands.Count > 0);
			}

			base.OnCreateOptionsMenu(menu, inflater);
		}

		public void OnDialogClicked(object sender, DialogClickEventArgs e)
		{
			Console.WriteLine (e.Which);
		}

		public override bool OnOptionsItemSelected (ActionbarSherlock.View.IMenuItem item)
		{
			//This uses the imported MenuItem from ActionBarSherlock
			switch (item.ItemId) {
				case Resource.Id.menu_screen_detail_map:
					break;
				case Resource.Id.menu_screen_detail_commands:
					AlertDialog.Builder builder = new AlertDialog.Builder (this.Activity);
					builder.SetTitle (GetString(Resource.String.menu_screen_detail_title));
					List<Command> commands = ((Thing)obj).ActiveCommands;
					string[] commandNames = new string[commands.Count];
					for (int i = 0; i < commands.Count; i++)
						commandNames[i] = commands[i].Text;
					builder.SetItems(commandNames,OnDialogClicked);
					builder.SetNeutralButton( GetString(Resource.String.menu_screen_detail_cancel), OnDialogClicked); 
					builder.Show();
					break;
			}

			return true;
		}

		public void OnPropertyChanged(object sender,  PropertyChangedEventArgs e)
		{
			updateContent ();
		}

		public override void OnResume()
		{
			base.OnResume();

			//TODO: Load data
			updateContent ();
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			//TODO: Store instance state data
		}

		#region Private Functions

		private void updateContent()
		{
			if (obj != null) {
				// Assign this item's values to the various subviews
				this.SherlockActivity.SupportActionBar.Title = obj.Name;
				if (obj.Image != null) {
					Bitmap bm = BitmapFactory.DecodeByteArray (obj.Image.Data, 0, obj.Image.Data.Length);
					imageView.SetImageBitmap (bm);
					imageView.Visibility = ViewStates.Visible;
				} else {
					imageView.Visibility = ViewStates.Gone;
				}
				textView.SetText (obj.Description, TextView.BufferType.Normal);
			}
		}

		#endregion

	}
}

