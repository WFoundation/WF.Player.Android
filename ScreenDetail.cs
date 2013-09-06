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
		private List<Command> commands;
		private Engine engine;
		private UIObject obj;
		private Command com;
		private ImageView imageView;
		private ActionbarSherlock.View.IMenuItem menuMap;
		private ActionbarSherlock.View.IMenuItem menuCommands;
		private LinearLayout layoutButtons;
		private LinearLayout layoutWorksWith;
		private bool remove = false;
		private TextView textView;
		private TextView textWorksWith;

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
			textWorksWith = view.FindViewById<TextView> (Resource.Id.textWorksWith);
			layoutButtons = view.FindViewById<LinearLayout> (Resource.Id.layoutButtons);
			layoutWorksWith = view.FindViewById<LinearLayout> (Resource.Id.layoutWorksWith);

			layoutButtons.Visibility = ViewStates.Visible;
			layoutWorksWith.Visibility = ViewStates.Gone;

//			SetHasOptionsMenu(!(obj is Task));

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
//			switch (item.ItemId) {
//				case Resource.Id.menu_screen_detail_map:
//					break;
//				case Resource.Id.menu_screen_detail_commands:
//					AlertDialog.Builder commandsDialog = new AlertDialog.Builder (this.Activity);
//					commandsDialog.SetTitle (GetString(Resource.String.menu_screen_detail_title));
//					List<Command> commands = ((Thing)obj).ActiveCommands;
//					string[] commandNames = new string[commands.Count];
//					for (int i = 0; i < commands.Count; i++)
//						commandNames[i] = commands[i].Text;
//					commandsDialog.SetItems(commandNames,OnDialogClicked);
//					commandsDialog.SetNeutralButton( GetString(Resource.String.menu_screen_detail_cancel), OnDialogClicked); 
//					commandsDialog.Show();
//					break;
//			}

			return true;
		}

		public void OnPropertyChanged(object sender,  PropertyChangedEventArgs e)
		{
			string[] properties = {"Name","Description","Media","Commands"};
			if (properties.Contains(e.PropertyName))
				updateContent ();
			if (e.PropertyName.Equals("Visible"))
				remove = !obj.Visible;
		}

		public override void OnResume()
		{
			base.OnResume();

			//TODO: Load data
			if (remove)
				((ScreenActivity)this.Activity).RemoveScreen (ScreenType.DetailScreen);

			updateContent ();
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			//TODO: Store instance state data
		}

		public void OnButtonClicked(object sender, EventArgs e)
		{
			Command c = commands [(int)((Button)sender).Tag];

			if (c.CmdWith) {
				layoutButtons.Visibility = ViewStates.Gone;
				layoutWorksWith.Visibility = ViewStates.Visible;
				com = c;
				updateContent ();
			} else
				c.Execute ();
		}

		public void OnThingClicked(object sender, EventArgs e)
		{
			List<Thing> t = com.TargetObjects;
			com.Execute(t[(int)((Button)sender).Tag]);
		}

		public void OnNothingClicked(object sender, EventArgs e)
		{
			layoutButtons.Visibility = ViewStates.Visible;
			layoutWorksWith.Visibility = ViewStates.Gone;
		}

		#region Private Functions

		private void updateContent()
		{
			if (obj != null && this.SherlockActivity != null) {
				// Assign this item's values to the various subviews
				this.SherlockActivity.SupportActionBar.Title = obj.Name;
				this.SherlockActivity.SupportActionBar.SetDisplayShowHomeEnabled(true);

				if (obj.Image != null) {
					Bitmap bm = BitmapFactory.DecodeByteArray (obj.Image.Data, 0, obj.Image.Data.Length);
					imageView.SetImageBitmap (bm);
					imageView.Visibility = ViewStates.Visible;
				} else {
					imageView.Visibility = ViewStates.Gone;
				}

				textView.SetText (obj.Description, TextView.BufferType.Normal);

				if (obj is Task)
					return;

				if (layoutButtons.Visibility == ViewStates.Visible) {
					layoutButtons.RemoveAllViews ();
					commands = ((Thing)obj).ActiveCommands;
					for (int i = 0; i < commands.Count; i++) {
						Button btnView = new Button (Activity.ApplicationContext);
						btnView.Text = commands [i].Text;
						btnView.Tag = i;
						btnView.Click += OnButtonClicked;
						layoutButtons.AddView (btnView);
					}
				}

				if (layoutWorksWith.Visibility == ViewStates.Visible) {
					layoutWorksWith.RemoveViews(1,layoutWorksWith.ChildCount-1);
					List<Thing> t = com.TargetObjects;
					if (t.Count == 0) {
						textWorksWith.Text = com.EmptyTargetListText;
						Button btnView = new Button (Activity.ApplicationContext);
						btnView.Text = GetString(Resource.String.ok);
						btnView.Click += OnNothingClicked;
						layoutWorksWith.AddView (btnView);
					} else {
						textWorksWith.Text = com.Text;
						for (int i = 0; i < t.Count; i++) {
							Button btnView = new Button (Activity.ApplicationContext);
							btnView.Text = t[i].Name;
							btnView.Tag = i;
							btnView.Click += OnThingClicked;
							layoutWorksWith.AddView (btnView);
						}
					}
				}
			}
		}

		#endregion

	}
}

