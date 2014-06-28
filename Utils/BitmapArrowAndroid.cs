using System;
using Android.Graphics;

namespace WF.Player
{
	public static class BitmapArrow
	{
		/// <summary>
		/// Draws image of arrow with given direction.
		/// </summary>
		/// <returns>Bitmap of arrow.</returns>
		/// <param name="direction">Direction of the arrow.</param>
		public static Bitmap Draw(int size, double direction)
		{
			if(size == 0)
				return null;

			return Draw(size, size, direction);
		}

		public static Bitmap Draw(int width, int height, double direction)
		{
			if(width == 0 || height == 0)
					return null;

			int w2 = width / 2;
			int h2 = height / 2;

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
			PointF p4 = new PointF((float) (w2 + width / 3 * Math.Sin (rad4)), (float) (h2 + height / 3 * Math.Cos (rad4)));

			Bitmap b = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

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
	}
}

