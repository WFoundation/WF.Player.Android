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
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Graphics;

namespace WF.Player
{
	/// <summary>
	/// AsyncDrawable derived from BitmapDrawable, which knows, what AsyncImageTask should load the image.
	/// </summary>
	public class AsyncDrawable : BitmapDrawable 
	{
		/// <summary>
		/// Reference to the loading AsyncImageTask.
		/// </summary>
		readonly System.WeakReference<AsyncImageTask> _asyncImageTaskReference;

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.AsyncDrawable"/> class.
		/// </summary>
		/// <param name="res">Resources of the context.</param>
		/// <param name="bitmap">Bitmap to load.</param>
		/// <param name="asyncImageTask">AsyncImageTask, which belongs to this AsyncDrawable.</param>
		public AsyncDrawable(Resources res, Bitmap bitmap, AsyncImageTask asyncImageTask) : base(res, bitmap)
		{
			_asyncImageTaskReference = new WeakReference<AsyncImageTask>(asyncImageTask);
		}

		/// <summary>
		/// Gets the AsyncImageTask belonging to this AsyncDrawable.
		/// </summary>
		/// <value>The async image task.</value>
		public AsyncImageTask AsyncImageTask 
		{
			get { 
				AsyncImageTask asyncImageTask = null;
				_asyncImageTaskReference.TryGetTarget(out asyncImageTask);
				return asyncImageTask;
			}
		}
	}
}

