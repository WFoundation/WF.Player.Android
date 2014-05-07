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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WF.Player.Types
{
	public static class EnumExtensionsAndroid
	{
		/// <summary>
		/// Convert TextAlign to a value, which is valid for Android.
		/// </summary>
		/// <returns>The local.</returns>
		/// <param name="ta">Ta.</param>
		public static GravityFlags ToSystem(this TextAlign ta)
		{
			GravityFlags result = GravityFlags.CenterHorizontal;

			switch (ta) {
				case TextAlign.LeftAlign:
					result = GravityFlags.Left;
					break;
				case TextAlign.Center:
					result = GravityFlags.CenterHorizontal;
					break;
				case TextAlign.RightAlign:
					result = GravityFlags.Right;
					break;
			}

			return result;
		}
	}
}

