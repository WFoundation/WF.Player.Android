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
using WF.Player.Types;

namespace WF.Player.Views
{
	public class TextViewAndroid : TextViewBase
	{
		TextView _view;

		#region Constructor

		public TextViewAndroid(TextView view)
		{
			_view = view;
		}

		#endregion

		#region Members

		public override object View
		{
			get { return _view; }
		}

		public override string Text
		{
			get { return _view.Text; }
			set { 
				if (_view.Text != value)
					_view.Text = value;
			}
		}

		public override TextAlign TextAlignment
		{
			get {
				TextAlign result = TextAlign.Center;

				if (_view.Gravity == GravityFlags.Left)
					result = TextAlign.LeftAlign;
				if (_view.Gravity == GravityFlags.CenterHorizontal)
					result = TextAlign.Center;
				if (_view.Gravity == GravityFlags.Right)
					result = TextAlign.RightAlign;

				return result;
			}
			set {
				switch (value) {
					case TextAlign.LeftAlign:
						_view.Gravity = GravityFlags.Left;
						break;
					case TextAlign.Center:
						_view.Gravity = GravityFlags.CenterHorizontal;
						break;
					case TextAlign.RightAlign:
						_view.Gravity = GravityFlags.Right;
						break;
				}
			}
		}

		public override double TextSize
		{
			get { return (double)_view.TextSize; }
			set { _view.SetTextSize(global::Android.Util.ComplexUnitType.Sp, (float)value); }
		}

		#endregion
	}
}

