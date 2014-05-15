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
using WF.Player.Core;
using WF.Player.Core.Engines;
using WF.Player.Types;

namespace WF.Player.Game
{
	#region ScreenDetail

	public partial class GameDetailScreen
	{
		GameController ctrl;
		UIObject activeObject;
		WherigoCollection<Command> commands;
		WherigoCollection<Thing> targets;
		string[] properties = {"Name", "Description", "Media", "Commands"};

		ScreenTypes Type = ScreenTypes.Details;

		#region Object Handling

		public UIObject ActiveObject
		{
			get {
				return activeObject;
			}
			set {
				if (activeObject != value) {
					activeObject = value;
					Refresh ();
				}
			}
		}

		#endregion

		#region Common Functions

		void StartEvents()
		{
			activeObject.PropertyChanged += OnPropertyChanged;
		}

		void StopEvents()
		{
			activeObject.PropertyChanged -= OnPropertyChanged;
		}

		public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			bool remove = false;

			if (activeObject == null)
				return;

			if (e.PropertyName.Equals("Commands"))
				commands = ((Thing)activeObject).ActiveCommands;

			// Check, if one of the visible entries changed
			if (!(e is PropertyChangedEventArgs) || (e is PropertyChangedEventArgs && properties.Contains(((PropertyChangedEventArgs)e).PropertyName)))
				Refresh(e.PropertyName);

			// The object is set to not visible or not active, so it should removed from screen
			if (e.PropertyName.Equals("Visible") || e.PropertyName.Equals("Active"))
				remove = !activeObject.Visible;
			// The object is moved to nil, so it should removed from screen
			if (e.PropertyName.Equals("Container") && !(activeObject is Task) && ((Thing)activeObject).Container == null)
				remove = true;

			if (remove) {
				StopEvents ();
				ctrl.RemoveScreen (this);
			}
		}

		#endregion

	}

	#endregion

}