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

namespace WF.Player.Location
{
	public partial class GPSListener
	{
		bool _valid;

		// Public events
		event EventHandler<LocationChangedEventArgs> LocationChanged;
		event EventHandler<BearingChangedEventArgs> BearingChanged;

		#region Members

		GPSLocation _location;

		/// <summary>
		/// Gets the last known location.
		/// </summary>
		/// <value>The location.</value>
		public GPSLocation Location {
			get { return _location; }
		}

		/// <summary>
		/// Gets a value indicating whether the location is valid.
		/// </summary>
		/// <value><c>true</c> if the location is valid; otherwise, <c>false</c>.</value>
		public bool IsValid
		{
			get { return _valid; }
		}

		#endregion

		#region Methods

		public void AddLocationListener(EventHandler<LocationChangedEventArgs> handler)
		{
			Start();
			LocationChanged += (EventHandler<LocationChangedEventArgs>)handler;
		}

		public void RemoveLocationListener(EventHandler<LocationChangedEventArgs> handler)
		{
			LocationChanged -= (EventHandler<LocationChangedEventArgs>)handler;
			Stop();
		}

		public void AddBearingListener(EventHandler<BearingChangedEventArgs> handler)
		{
			Start();
			BearingChanged += (EventHandler<BearingChangedEventArgs>)handler;
		}

		public void RemoveBearingListener(EventHandler<BearingChangedEventArgs> handler)
		{
			BearingChanged -= (EventHandler<BearingChangedEventArgs>)handler;
			Stop();
		}

		#endregion
	}

	#region EventArgs

	public sealed class LocationChangedEventArgs : EventArgs
	{
		GPSLocation _location;

		public LocationChangedEventArgs(GPSLocation loc)
		{
			_location = loc;
		}

		public LocationChangedEventArgs(global::Android.Locations.Location loc)
		{
			_location = new GPSLocation(loc);
		}

		public GPSLocation Location {
			get { return _location; }
		}
	}

	public sealed class BearingChangedEventArgs : EventArgs
	{
		double _bearing;

		public BearingChangedEventArgs(double bear)
		{
			_bearing = bear;
		}

		public double Bearing {
			get { return _bearing; }
		}
	}

	#endregion

}

