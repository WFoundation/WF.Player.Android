///
/// WF.Player - A Wherigo Player, which use the Wherigo Foundation Core.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vernacular;
using WF.Player.Views;

namespace WF.Player.Game
{
	/// <summary>
	/// Screen check location: common part.
	/// </summary>
	/// <remarks>
	/// This part handles all things which are in Android and iOS common
	/// </remarks>
	public partial class GameCheckLocation
	{
		GameController ctrl;
		TextViewBase textDescription;
		TextViewBase textCoordinates;
		TextViewBase textAccuracy;
		ButtonViewBase button;

		#region Methods

		/// <summary>
		/// Commons for creating the view.
		/// </summary>
		void CommonCreate()
		{
			textDescription.TextAlignment = Main.Prefs.TextAlignment;
			textDescription.TextSize = Main.Prefs.TextSize;

			textCoordinates.TextAlignment = Main.Prefs.TextAlignment;
			textCoordinates.TextSize = Main.Prefs.TextSize;

			textAccuracy.TextAlignment = Main.Prefs.TextAlignment;
			textAccuracy.TextSize = Main.Prefs.TextSize;

			button.ClickHandler += OnButtonClicked;

			MakeButtonRed();
		}

		/// <summary>
		/// Commons, when view is visible again.
		/// </summary>
		void CommonResume()
		{
			// Add to GPS
			Main.GPS.AddLocationListener(OnRefreshLocation);

			Refresh();
		}

		/// <summary>
		/// Commons, when the view is not longer visible.
		/// </summary>
		void CommonPause()
		{
			// Remove from GPS
			Main.GPS.RemoveLocationListener(OnRefreshLocation);
		}

		/// <summary>
		/// Commons, when refreshing the view.
		/// </summary>
		void CommonRefresh()
		{
			SetTitle(Catalog.GetString("GPS Check"));

			textDescription.Text = Catalog.GetString("For much fun with the cartridge, you should wait for a good accuracy of your GPS signal.");
			if (Main.GPS.Location.IsValid) {
				textCoordinates.Text = Catalog.Format(Catalog.GetString("Current Coordinates\n{0}"), Main.GPS.Location.ToString());
				textAccuracy.Text = Catalog.Format(Catalog.GetString("Current Accuracy\n{0}"), Main.GPS.Location.ToAccuracyString());
			} else {
				textCoordinates.Text = Catalog.Format(Catalog.GetString("Current Coordinates\n{0}"), Catalog.GetString("unknown"));
				textAccuracy.Text = Catalog.Format(Catalog.GetString("Current Accuracy\n{0} m"), Strings.Infinite);
			}

			if (Main.GPS.Location.IsValid && Main.GPS.Location.Accuracy < 30) {
				button.Text = Catalog.GetString("Start");
				MakeButtonGreen();
			} else {
				button.Text = Catalog.GetString("Start anyway");
				MakeButtonRed();
			}
		}

		#endregion

		#region Events

		void OnButtonClicked (object sender, ButtonClickEventArgs e)
		{
			ctrl.Feedback();
			ctrl.InitController(true);
		}

		void OnRefreshLocation (object sender, WF.Player.Location.LocationChangedEventArgs e)
		{
			Refresh();
		}

		#endregion
	}
}

