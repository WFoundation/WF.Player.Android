///
/// WF.Player.iPhone/WF.Player.Android - A Wherigo Player for Android and iPhone, which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2014 Dirk Weltz <web@weltz-online.de>
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

namespace WF.Player.Android
{
	/// <summary>
	/// Screen main fragment.
	/// </summary>
	public partial class ScreenMain
	{
		Engine engine;
		ScreenController ctrl;

		string[] properties = {"Name", "Visible"};

		object iconLocation;
		object iconYouSee;
		object iconInventory;
		object iconTask;
		object iconPosition;

		#region Common Functions

		/// <summary>
		/// Raises, when a entry is selected.
		/// </summary>
		/// <param name="pos">Position.</param>
		public void EntrySelected(int pos)
		{
			switch (pos)
			{
			case 0:
				var listZones = ctrl.Engine.ActiveVisibleZones;
				if (listZones.Count > 0)
				{
//                                                if (listZones.Count == 1)
//                                                {
//                                                        ctrl.ShowScreen(ScreenType.Details, listZones[0]);
//                                                }
//                                                else
					{
						ctrl.ShowScreen(ScreenType.Locations, null);
					}
				}
				break;
			case 1:
				var listObjects = ctrl.Engine.VisibleObjects;
				if (listObjects.Count > 0)
				{
//                                                if (listObjects.Count == 1)
//                                                {
//                                                        if (listObjects[0].HasOnClick)
//                                                        {
//                                                                listObjects[0].CallOnClick();
//                                                        }
//                                                        else
//                                                        {
//                                                                ctrl.ShowScreen(ScreenType.Details, listObjects[0]);
//                                                        }
//                                                }
//                                                else
					{
						ctrl.ShowScreen(ScreenType.Items, null);
					}
				}
				break;
			case 2:
				var listInventory = ctrl.Engine.VisibleInventory;
				if (listInventory.Count > 0)
				{
//                                                if (listInventory.Count == 1)
//                                                {
//                                                        if (listInventory[0].HasOnClick)
//                                                        {
//                                                                listInventory[0].CallOnClick();
//                                                        }
//                                                        else
//                                                        {
//                                                                ctrl.ShowScreen(ScreenType.Details, listInventory[0]);
//                                                        }
//                                                }
//                                                else
					{
						ctrl.ShowScreen(ScreenType.Inventory, null);
					}
				}
				break;
			case 3:
				var listTasks = ctrl.Engine.ActiveVisibleTasks;
				if (listTasks.Count > 0)
				{
//                                                if (listTasks.Count == 1)
//                                                {
//                                                        if (listTasks[0].HasOnClick)
//                                                        {
//                                                                listTasks[0].CallOnClick();
//                                                        }
//                                                        else
//                                                        {
//                                                                ctrl.ShowScreen(ScreenType.Details, listTasks[0]);
//                                                        }
//                                                }
//                                                else
					{
						ctrl.ShowScreen(ScreenType.Tasks, null);
					}
				}
				break;
			case 4:
				break;
			}

		}

		public void GetContentEntry(int position, out string header, out string items, out object image)
		{
			if (engine.Cartridge == null) {
				header = "";
				items = "";
				image = null;
				return;
			}

			List<string> itemsList = new List<string>();
			string empty = "";

			header = "";
			items = "";
			image = null;

			if (engine != null)
			{
				switch (position)
				{
				case 0:
					header = Catalog.GetString("Locations");
					empty = Catalog.GetString(engine.Cartridge.EmptyZonesListText);
					image = iconLocation;
					foreach (UIObject o in engine.ActiveVisibleZones)
					{
						itemsList.Add(o.Name == null ? "" : o.Name);
					}
					break;
				case 1:
					header = Catalog.GetString("You see");
					empty = Catalog.GetString(engine.Cartridge.EmptyYouSeeListText);
					image = iconYouSee;
					foreach (UIObject o in engine.VisibleObjects)
					{
						itemsList.Add(o.Name == null ? "" : o.Name);
					}
					break;
				case 2:
					header = Catalog.GetString("Inventory");
					empty = Catalog.GetString(engine.Cartridge.EmptyInventoryListText);
					image = iconInventory;
					foreach (UIObject o in engine.VisibleInventory)
					{
						itemsList.Add(o.Name == null ? "" : o.Name);
					}
					break;
				case 3:
					header = Catalog.GetString("Tasks");
					empty = Catalog.GetString(engine.Cartridge.EmptyTasksListText);
					image = iconTask;
					foreach (UIObject o in engine.ActiveVisibleTasks)
					{
						itemsList.Add((((Task)o).Complete ? (((Task)o).CorrectState == TaskCorrectness.NotCorrect ? Strings.TaskNotCorrect : Strings.TaskCorrect) + " " : "") + (o.Name == null ? "" : o.Name));
					}
					break;
				case 4:
					header = Catalog.GetString("Position");
					image = iconPosition;
					break;
				}

				if (position < 4)
					header = String.Format("{0} [{1}]", header, itemsList.Count);

				if (itemsList.Count == 0)
					items = empty;
				else
				{
				StringBuilder itemsText = new StringBuilder();
				foreach(string s in itemsList)
				{
					if (itemsText.Length != 0)
						itemsText.Append(System.Environment.NewLine);
					itemsText.Append(s);
				}
				items = itemsText.ToString();
				}
			}
		}

		void StartEvents()
		{
			engine.AttributeChanged += OnPropertyChanged;
			engine.InventoryChanged += OnPropertyChanged;
			engine.ZoneStateChanged += OnPropertyChanged;

			engine.PropertyChanged += OnPropertyChanged;
		}

		void StopEvents()
		{
			engine.AttributeChanged -= OnPropertyChanged;
			engine.InventoryChanged -= OnPropertyChanged;
			engine.ZoneStateChanged -= OnPropertyChanged;

			engine.PropertyChanged -= OnPropertyChanged;
		}

		public void OnPropertyChanged(object sender, EventArgs e)
		{
			// Check, if one of the visible entries changed
			if (!(e is PropertyChangedEventArgs) || (e is PropertyChangedEventArgs && properties.Contains(((PropertyChangedEventArgs)e).PropertyName)))
				Refresh();
		}

		#endregion

	}

}