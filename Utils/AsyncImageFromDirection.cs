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
using Android.Graphics;
using Android.Graphics.Drawables;

namespace WF.Player
{
	public class AsyncImageFromDirection : AsyncImageTask
	{
		int? _direction = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="WF.Player.AsyncImageFromFile"/> class.
		/// </summary>
		/// <param name="imageView">Image view to show the loaded bitmap.</param>
		/// <param name="pSampleSize">Sample size.</param>
		/// <param name="pReqWidth">Requested width.</param>
		/// <param name="pReqHeight">Requested height.</param>
		public AsyncImageFromDirection(ImageView imageView, int pSampleSize, int pReqWidth, int pReqHeight) : base(imageView, pSampleSize, pReqWidth, pReqHeight)
		{
		}

		/// <summary>
		/// Gets the direction of the arrow.
		/// </summary>
		/// <value>Direction in degrees.</value>
		public int? Direction
		{
			get { return _direction; }
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
			_direction = (int)@params[0];

			if (_direction == null)
				return null;

			while(_direction > 360) _direction -= 360;
			while(_direction < 0) _direction += 360;

			try
			{
				return DrawArrow((double)_direction);
			}
			catch(System.Exception ex)
			{
				Log.Debug("TT", "Exception : " + ex.Message);
				return null;
			}
		}

		#region Private Functions

		/// <summary>
		/// Draws image for inside of a zone.
		/// </summary>
		/// <returns>Bitmap of inside.</returns>
//		internal Bitmap DrawCenter ()
//		{
//			int w = _reqWidth;
//			int h = _reqHeight;
//
//			Paint light = new Paint (PaintFlags.AntiAlias);
//			Paint dark = new Paint (PaintFlags.AntiAlias);
//			Paint black = new Paint (PaintFlags.AntiAlias);
//
//			light.Color = new Color (128, 0, 0, 255);
//			light.StrokeWidth = 0f;
//			light.SetStyle (Paint.Style.FillAndStroke);
//
//			dark.Color = new Color (255, 0, 0, 255);
//			dark.StrokeWidth = 0f;
//			dark.SetStyle (Paint.Style.FillAndStroke);
//
//			black.Color = new Color (0, 0, 0, 255);
//			black.StrokeWidth = 0f;
//			black.SetStyle (Paint.Style.Stroke);
//
//			Bitmap b = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
//
//			using(Canvas c = new Canvas(b)) {
//				light.Color = new Color (128, 0, 0, 255);
//				light.StrokeWidth = 0f;
//				light.SetStyle (Paint.Style.Stroke);
//
//				dark.Color = new Color (255, 0, 0, 255);
//				dark.StrokeWidth = 0f;
//				dark.SetStyle (Paint.Style.FillAndStroke);
//
//				int cx = c.Width / 2 - 1;
//				int cy = c.Height / 2 - 1;
//
//				c.DrawCircle (cx, cy, w / 8, light);
//				c.DrawCircle (cx, cy, w / 32 * 3, dark);
//			}
//
//			return b;
//		}

		/// <summary>
		/// Draws image of arrow with given direction.
		/// </summary>
		/// <returns>Bitmap of arrow.</returns>
		/// <param name="direction">Direction of the arrow.</param>
		private Bitmap DrawArrow(double direction)
		{
			int w = _reqWidth;
			int h = _reqHeight;
			int w2 = w / 2;
			int h2 = h / 2;

			if(w == 0 || h == 0)
				return null;

			Paint light = new Paint(PaintFlags.AntiAlias);
			Paint dark = new Paint(PaintFlags.AntiAlias);
			Paint black = new Paint(PaintFlags.AntiAlias);

			light.Color = new Color(128, 0, 0, 255);
			light.StrokeWidth = 0f;
			light.SetStyle(Paint.Style.FillAndStroke);

			dark.Color = new Color(255, 0, 0, 255);
			dark.StrokeWidth = 0f;
			dark.SetStyle(Paint.Style.FillAndStroke);

			black.Color = new Color(0, 0, 0, 255);
			black.StrokeWidth = 0f;
			black.SetStyle(Paint.Style.Stroke);

			// Values of direction are between 0° and 360°, but for drawing we need -180° to +180°
			direction -= 180;

			double rad1 = direction / 180.0 * Math.PI;
			double rad2 = (direction + 180.0 + 30.0) / 180.0 * Math.PI;
			double rad3 = (direction + 180.0 - 30.0) / 180.0 * Math.PI; 
			double rad4 = (direction + 180.0) / 180.0 * Math.PI; 

			PointF p1 = new PointF((float) (w2 + w2 * Math.Sin (rad1)), (float) (h2 + h2 * Math.Cos (rad1)));
			PointF p2 = new PointF((float) (w2 + w2 * Math.Sin (rad2)), (float) (h2 + h2 * Math.Cos (rad2)));
			PointF p3 = new PointF((float) (w2 + w2 * Math.Sin (rad3)), (float) (h2 + h2 * Math.Cos (rad3)));
			PointF p4 = new PointF((float) (w2 + w / 3 * Math.Sin (rad4)), (float) (h2 + h / 3 * Math.Cos (rad4)));

			Bitmap b = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);

			using(Canvas c = new Canvas(b)) {
				int cx = c.Width / 2 - 1;
				int cy = c.Height / 2 - 1;

				global::Android.Graphics.Path path1 = new global::Android.Graphics.Path ();
				global::Android.Graphics.Path path2 = new global::Android.Graphics.Path ();
				global::Android.Graphics.Path path3 = new global::Android.Graphics.Path ();

				path1.MoveTo (p1.X,p1.Y);
				path1.LineTo (p2.X,p2.Y);
				path1.LineTo (p4.X,p4.Y);
				path1.LineTo (p1.X,p1.Y);

				path2.MoveTo (p1.X,p1.Y);
				path2.LineTo (p3.X,p3.Y);
				path2.LineTo (p4.X,p4.Y);
				path2.LineTo (p1.X,p1.Y);

				path3.MoveTo (p1.X,p1.Y);
				path3.LineTo (p2.X,p2.Y);
				path3.LineTo (p4.X,p4.Y);
				path3.LineTo (p1.X,p1.Y);
				path3.LineTo (p3.X,p3.Y);
				path3.LineTo (p4.X,p4.Y);

				light.Color = new Color (128, 0, 0, 255);
				dark.Color = new Color (255, 0, 0, 255);
				black.Color = new Color (0, 0, 0, 255);

				c.DrawPath (path1, light);
				c.DrawPath (path2, dark);
				c.DrawPath (path3, black);
			}

			return b;
		}

		#endregion

		#region Static Functions

		public static void LoadBitmap(ImageView imageView, double d, int w, int h)
		{
			int direction = (int)d;

			// Check, if there is another image creation is running
			if(CancelPotentialWork(direction, imageView)) {
				// Create task for image creation
				AsyncImageFromDirection asyncImageTask = new AsyncImageFromDirection(imageView, 0, w, h);
				// Has the ImageView an existing AsyncDrawable
				if(imageView.Drawable == null || !(imageView.Drawable is AsyncDrawable)) {
					// No, than create one
					AsyncDrawable asyncDrawable = new AsyncDrawable(imageView.Context.Resources, BitmapFactory.DecodeResource(imageView.Context.Resources, Android.Resource.Drawable.IcMenuGallery), asyncImageTask);
					// Set it as bitmap for the ImageView until the correct image is ready
					imageView.SetImageDrawable(asyncDrawable);
				}
				asyncImageTask.Execute(direction);
			}
		}

		/// <summary>
		/// Determines if there is another task for this ImageView, with the same direction.
		/// </summary>
		/// <returns><c>true</c> if no task is associated with this ImageView with the same direction; otherwise, <c>false</c>.</returns>
		/// <param name="direction">Direction in degrees.</param>
		/// <param name="imageView">ImageView to check.</param>
		public static Boolean CancelPotentialWork(int direction, ImageView imageView) 
		{
			AsyncImageFromDirection asyncImageTask = GetAsyncImageTask(imageView) as AsyncImageFromDirection;

			if (asyncImageTask != null) {
				int? bitmapDirection = asyncImageTask.Direction;
				// If bitmapDirection is not yet set or it differs from the new direction
				if (bitmapDirection == null || bitmapDirection != direction) {
					// Cancel previous task, because it is not longer needed
					asyncImageTask.Cancel(true);
				} else {
					// The same work is already in progress
					return false;
				}
			}
			// No task associated with the ImageView, or an existing task was cancelled
			return true;
		}

		#endregion
	}
}

