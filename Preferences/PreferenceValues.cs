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
using WF.Player.Types;

namespace WF.Player.Preferences
{
	partial class PreferenceValues
	{
		#region Members

		public bool FeedbackSound 
		{
			get { return GetBool("feedback_sound", false); }
			set { SetBool("feedback_sound",value); }
		}

		public bool FeedbackVibration 
		{
			get { return GetBool("feedback_vibration", false); }
			set { SetBool("feedback_vibration",value); }
		}

		public TextAlign TextAlignment 
		{
			get { 
				int value;
				value = GetInt("text_alignment", 1);
				return (TextAlign)value;
			 }
			set { SetInt("text_alignment", (int)value); }
		}

		public ImageResize ImageResize
		{
			get { 
				int value;
				value = GetInt("image_size", 1);
				return (ImageResize)value; 
			}
		}

		public bool InputFocus 
		{
			get { return GetBool("input_focus", false); }
			set { SetBool("input_focus",value); }
		}

		public double TextSize
		{
			get { 
				double value;
				if (!Double.TryParse(GetString("text_size", "18"), out value))
					value = 18;
				return value; 
			}
			set { SetString("text_size",value.ToString()); }
		}

		#endregion
	}
}

