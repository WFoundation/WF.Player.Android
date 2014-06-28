///
/// WF.Player.Android - A Wherigo Player for Android, which use the Wherigo Foundation Core.
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
using Android.Graphics;
using Android.Util;

namespace WF.Player
{
	public static class BitmapHelpers
	{
		public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
		{
			//_____________________________
			// Raw height and width of image
			int height = options.OutHeight;
			int width = options.OutWidth;
			int inSampleSize = 1;

			if (height > reqHeight || width > reqWidth)
			{
				if (width > height)
				{
					inSampleSize = (int)System.Math.Round((float)height / (float)reqHeight);
				}
				else
				{
					inSampleSize = (int)System.Math.Round((float)width / (float)reqWidth);
				}
			}

			return inSampleSize;

		}

		public static Bitmap DecodeSampleBitmapFromFile(string path, int reqWidth, int reqHeight)
		{

			try
			{
				//______________________________________________________________
				// First decode with inJustDecodeBounds=true to check dimensions
				BitmapFactory.Options options = new BitmapFactory.Options();
				options.InJustDecodeBounds = true;
				BitmapFactory.DecodeFile(path, options);
				//BitmapFactory.DecodeStream(url.OpenConnection().InputStream, null, options);

				//______________________
				// Calculate inSampleSize
				options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

				//____________________________________
				// Decode bitmap with inSampleSize set
				options.InJustDecodeBounds = false;
				return BitmapFactory.DecodeFile(path, options);
			}
			catch (System.Exception ex)
			{
				Log.Debug("DecodeBitmapFromFile: ", ex.Message);
				return null;
			}
			finally
			{
				//
			}
		}
	}
}

