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
using Android.Util;
using Android.Widget;
using Java.Lang.Ref;

namespace WF.Player
{
	public class AsyncImageFromFile : AsyncImageTask
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.AsyncImageFromFile"/> class.
		/// </summary>
		/// <param name="imageView">Image view to show the loaded bitmap.</param>
		/// <param name="pSampleSize">Sample size.</param>
		/// <param name="pReqWidth">Requested width.</param>
		/// <param name="pReqHeight">Requested height.</param>
		public AsyncImageFromFile(ImageView imageView, int pSampleSize, int pReqWidth, int pReqHeight) : base(imageView, pSampleSize, pReqWidth, pReqHeight)
		{
		}
			
		/// <summary>
		/// Background worker function, which does the work.
		/// </summary>
		/// <param name="params">
		/// Parameter, given with the Execute call.
		/// Only parameter is [0], the path to bitmap file.
		/// </param>
		/// <returns>Bitmap from file or null.</returns>
		protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
		{
			string path = @params[0].ToString();

			try
			{
				return BitmapHelpers.DecodeSampleBitmapFromFile(path, _reqWidth, _reqHeight);
			}
			catch (System.Exception ex)
			{
				Log.Debug("TT", "Exception : " + ex.Message);
				return null;
			}
		}
	}
}

