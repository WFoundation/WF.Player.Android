///
/// WF.Player.Android - A Wherigo Player for iPhone which use the Wherigo Foundation Core.
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

namespace WF.Player.Android
{
	/// <summary>
	/// A kind of screen that displays game-related information to the players.
	/// </summary>
	public enum ScreenType : int
	{
		Main = 0,
		Locations,
		Items,
		Inventory,
		Tasks,
		Details,
		Dialog,
		Map
	}

	/// <summary>
	/// Map source types.
	/// </summary>
	public enum MapSource : int
	{
		GoogleMaps,
		GoogleSatellite,
		GoogleTerrain,
		GoogleHybrid,
		OpenStreetMap,
		OpenCycleMap,
		Offline,
		Cartridge,
		None
	}
}