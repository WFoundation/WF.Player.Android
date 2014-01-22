///
/// WF.Player.iPhone/WF.Player.Android - A Wherigo Player for Android, iPhone which use the Wherigo Foundation Core.
/// Copyright (C) 2012-2013 Dirk Weltz <web@weltz-online.de>
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

namespace WF.Player.Android
{
	#region ScreenDetail

	public partial class ScreenDetail
	{
		ScreenController ctrl;
		UIObject obj;
		WherigoCollection<Command> commands;
		WherigoCollection<Thing> targets;
		string[] properties = {"Name", "Description", "Media", "Commands"};

		#region Common Functions

		void StartEvents()
		{
			obj.PropertyChanged += OnPropertyChanged;
		}

		void StopEvents()
		{
			obj.PropertyChanged -= OnPropertyChanged;
		}

		public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			bool remove = false;

			if (e.PropertyName.Equals("Commands"))
				commands = ((Thing)obj).ActiveCommands;

			// Check, if one of the visible entries changed
			if (!(e is PropertyChangedEventArgs) || (e is PropertyChangedEventArgs && properties.Contains(((PropertyChangedEventArgs)e).PropertyName)))
				Refresh(e.PropertyName);

			// The object is set to not visible or not active, so it should removed from screen
			if (e.PropertyName.Equals("Visible") || e.PropertyName.Equals("Active"))
				remove = !obj.Visible;
			// The object is moved to nil, so it should removed from screen
			if (e.PropertyName.Equals("Container") && !(obj is Task) && ((Thing)obj).Container == null)
				remove = true;

			if (remove) {
				StopEvents ();
				ctrl.RemoveScreen (ScreenType.Details);
			}
		}

		#endregion

	}

	#endregion

}