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
using Vernacular;

namespace WF.Player.Location
{
	public static class Converters
	{
		public static string CoordinatToString(double lat, double lon, GPSFormat format, bool newLine = false)
		{
			string latDirect;
			string lonDirect;
			int latDegrees;
			int lonDegrees;
			int latMin = 0;
			int lonMin = 0;
			int latSec = 0;
			int lonSec = 0;
			double latDecimalMin = 0;
			double lonDecimalMin = 0;

			latDirect = lat > 0 ? Catalog.GetString("N", comment: "Direction North") : Catalog.GetString("S", comment: "Direction South");
			lonDirect = lon > 0 ? Catalog.GetString("E", comment: "Direction East") : Catalog.GetString("W", comment: "Direction West");

			latDegrees = Convert.ToInt32 (Math.Floor(lat));
			lonDegrees = Convert.ToInt32 (Math.Floor(lon));

			if (format == GPSFormat.DecimalMinutes || format == GPSFormat.DecimalMinutesSeconds) {
				latDecimalMin = Math.Round((lat - latDegrees) * 60.0, 3);
				lonDecimalMin = Math.Round((lon - lonDegrees) * 60.0, 3);

				latMin = Convert.ToInt32 (Math.Floor((lat - latDegrees) * 60.0));
				lonMin = Convert.ToInt32 (Math.Floor((lon - lonDegrees) * 60.0));
			}

			if (format == GPSFormat.DecimalMinutesSeconds) {
				latSec = Convert.ToInt32 (Math.Floor((((lat - latDegrees) * 60.0) - latMin) * 60.0));
				lonSec = Convert.ToInt32 (Math.Floor((((lon - lonDegrees) * 60.0) - lonMin) * 60.0));
			}

			string separator = newLine ? System.Environment.NewLine : " ";
			string result = "";

			switch (format) {
			case GPSFormat.Decimal:
				result = String.Format ("{0} {1:0.00000}°{4}{2} {3:0.00000}°", latDirect, lat, lonDirect, lon, separator);
				break;
			case GPSFormat.DecimalMinutes:
				result = String.Format ("{0} {1:00}° {2:00.000}'{6}{3} {4:000}° {5:00.000}'", latDirect, latDegrees, latDecimalMin, lonDirect, lonDegrees, lonDecimalMin, separator);
				break;
			case GPSFormat.DecimalMinutesSeconds:
				result = String.Format ("{0} {1:00}° {2:00}' {3:00.0}\"{8}{4} {5:000}° {6:00}' {7:00.0}\"", latDirect, latDegrees, latMin, latSec, lonDirect, lonDegrees,	lonMin,	lonSec, separator);
				break;
			}

			return result;
		}
	}
}

