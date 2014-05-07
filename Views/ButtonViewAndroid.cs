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

namespace WF.Player.Views
{
	public class ButtonViewAndroid : ButtonViewBase
	{
		Button _view;

		#region Constructor

		public ButtonViewAndroid(Button view)
		{
			_view = view;
			_view.Click += OnClick;
		}

		#endregion

		#region Members

		public override string Text
		{
			get { return _view.Text; }
			set { 
				if (_view.Text != value)
					_view.Text = value;
			}
		}

		public new Button View
		{
			get { return _view; }
			set { _view = value; }
		}

		#endregion

		#region Events

		void OnClick (object sender, EventArgs e)
		{
			OnClick(sender, new ButtonClickEventArgs(e));
		}

		#endregion
	}

	#region Event Args

	public class ButtonClickEventArgs
	{
		ButtonViewBase _view;
		EventArgs _args;

		public ButtonClickEventArgs(EventArgs args)
		{
			_args = args;
		}

		public ButtonViewBase Button
		{
			get { return _view; }
			set { _view = value; }
		}

		public EventArgs Args
		{
			get { return _args; }
			set { _args = value; }
		}
	}

	#endregion
}

