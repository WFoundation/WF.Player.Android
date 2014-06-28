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
using Android.OS;
using Android.Util;
using Android.Widget;
using Java.Lang.Ref;
using Android.Graphics.Drawables;

namespace WF.Player
{
	/// <summary>
	/// AsyncImageTask is the base class for derived classes, which are loading bitmaps 
	/// into ImageViews in the background without stopping the UI thread.
	/// </summary>
	public class AsyncImageTask : AsyncTask
	{
		Java.Lang.Ref.WeakReference _imageViewReference;
		protected int _sampleSize = 0;
		protected int _reqHeight = 0;
		protected int _reqWidth = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.AsyncImageTask"/> class.
		/// </summary>
		/// <param name="imageView">Image view to show the loaded bitmap.</param>
		/// <param name="pSampleSize">Sample size.</param>
		/// <param name="pReqWidth">Requested width.</param>
		/// <param name="pReqHeight">Requested height.</param>
		public AsyncImageTask(ImageView imageView, int pSampleSize, int pReqWidth, int pReqHeight)
		{
			// Use a WeakReference to ensure the ImageView can be garbage collected
			_imageViewReference = new Java.Lang.Ref.WeakReference(imageView);

			_reqHeight = pReqHeight;
			_reqWidth = pReqWidth;
			_sampleSize = pSampleSize;
		}

		/// <summary>
		/// Background worker function, which does the work.
		/// </summary>
		/// <param name="params">Parameter, given with the Execute call.</param>
		/// <returns>Bitmap from file or null.</returns>
		protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
		{
			return null;
		}

		/// <summary>
		/// Called after the bitmap is loaded.
		/// </summary>
		/// <param name="result">Result of the DoInBackground function.</param>
		protected override void OnPostExecute(Java.Lang.Object result)
		{
			base.OnPostExecute(result);

			if (IsCancelled)
			{
				result = null;
				Log.Debug("TT", "OnPostExecute - Task Cancelled");
			}
			else
			{
				using (Bitmap bmpResult = result as Bitmap)
				{
					if (_imageViewReference != null && bmpResult != null)
					{
						ImageView imageView = (ImageView)_imageViewReference.Get();

						AsyncImageTask asyncImageTask = GetAsyncImageTask(imageView);

						if(this == asyncImageTask && imageView != null) {
							imageView.SetImageBitmap(bmpResult);
							//							if (!pLRUCache.ContainsKey(path))
							//								pLRUCache.Add(path, bmpResult);
						}
					}
				}
			}
		}

		#region Static Functions

		protected static AsyncImageTask GetAsyncImageTask(ImageView imageView) 
		{
			if (imageView != null) {
				Drawable drawable = imageView.Drawable;
				if (drawable is AsyncDrawable) {
					AsyncDrawable asyncDrawable = (AsyncDrawable) drawable;
					return asyncDrawable.AsyncImageTask;
				}
			}

			return null;
		}

		#endregion
	}
}

