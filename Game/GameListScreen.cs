///
/// WF.Player.iPhone/WF.Player.Android - A Wherigo Player for Android and iPhone, which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014 Dirk Weltz <mail@wfplayer.com>
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Lesser General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
/// GNU Lesser General Public License for more details.
///
/// You should have received a copy of the GNU Lesser General Public License
/// along with this program. If not, see <http://www.gnu.org/licenses/>.
///

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Vernacular;
using WF.Player.Core;
using WF.Player.Core.Engines;
using WF.Player.Types;

namespace WF.Player.Game
{
	public partial class GameListScreen
	{
		GameController ctrl;
		Engine engine;
		ScreenTypes type;
		string[] properties = {"Name", "Icon", "Active", "Visible", "ObjectLocation", "VisibleObjects", "VisibleInventory", "ActiveVisibleTasks", "ActiveVisibleZones"};

		public List<UIObject> Items = new List<UIObject>();
		public bool ShowIcons;
		public bool ShowDirections;

		#region Properties

		public ScreenTypes Type {
			get { return type; }
		}

		#endregion

		#region Common Functions

		public void EntrySelected(int position)
		{
			UIObject obj = Items[position];

			if (!(obj is Zone) && obj.HasOnClick)
			{
				obj.CallOnClick();
			}
			else
			{
				ctrl.ShowScreen (ScreenTypes.Details, obj);
			}
		}

		public string GetContent()
		{
			string header = "";

			StopEvents();

			ShowIcons = false;
			ShowDirections = false;

			Items = new List<UIObject> ();

			switch (type)
			{
			case ScreenTypes.Locations:
				header = Catalog.GetString("Locations");
				ShowDirections = true;
				foreach (UIObject item in engine.ActiveVisibleZones)
				{
					ShowIcons |= item.Icon != null;
					Items.Add (item);
				}
				break;
			case ScreenTypes.Items:
				header = Catalog.GetString("You see");
				ShowDirections = true;
				foreach (UIObject item in engine.VisibleObjects)
				{
					ShowIcons |= item.Icon != null;
					Items.Add (item);
				}
				break;
			case ScreenTypes.Inventory:
				header = Catalog.GetString("Inventory");
				foreach (UIObject item in engine.VisibleInventory)
				{
					ShowIcons |= item.Icon != null;
					Items.Add (item);
				}
				break;
			case ScreenTypes.Tasks:
				header = Catalog.GetString("Tasks");
				foreach (UIObject item in engine.ActiveVisibleTasks)
				{
					ShowIcons |= item.Icon != null;
					Items.Add (item);
				}
				break;
			}

			StartEvents();

			Refresh(false);

			return header;
		}

		void StartEvents()
		{
			foreach(UIObject o in Items)
				o.PropertyChanged += OnPropertyChanged;

			engine.AttributeChanged += OnPropertyChanged;
			engine.InventoryChanged += OnPropertyChanged;
			engine.ZoneStateChanged += OnPropertyChanged;

			engine.PropertyChanged += OnPropertyChanged;
		}

		void StopEvents()
		{
			foreach(UIObject o in Items)
				o.PropertyChanged -= OnPropertyChanged;

			engine.AttributeChanged -= OnPropertyChanged;
			engine.InventoryChanged -= OnPropertyChanged;
			engine.ZoneStateChanged -= OnPropertyChanged;

			engine.PropertyChanged -= OnPropertyChanged;
		}

		public void OnPropertyChanged(object sender, EventArgs e)
		{
			bool newItems = false;

			newItems |= e is InventoryChangedEventArgs;
			newItems |= e is AttributeChangedEventArgs && ((AttributeChangedEventArgs)e).PropertyName.Equals("Active");
			newItems |= e is AttributeChangedEventArgs && ((AttributeChangedEventArgs)e).PropertyName.Equals("Visible");
			newItems |= e is PropertyChangedEventArgs && ((PropertyChangedEventArgs)e).PropertyName.Equals("Active");
			newItems |= e is PropertyChangedEventArgs && ((PropertyChangedEventArgs)e).PropertyName.Equals("Visible");

			// Check, if one of the visible entries changed
			if (!(e is PropertyChangedEventArgs) || (e is PropertyChangedEventArgs && properties.Contains(((PropertyChangedEventArgs)e).PropertyName)))
				Refresh(newItems);
		}

		#endregion

	}
}