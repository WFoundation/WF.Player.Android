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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WF.Player
{
	[Activity (Label = "WF.Player", Theme="@android:style/Theme.NoTitleBar")]	
	// For e-mail
	[IntentFilter (new[]{Intent.ActionView},
		Categories=new[]{Intent.CategoryOpenable, Intent.CategoryLauncher, Intent.CategoryDefault, Intent.CategoryBrowsable},
		Icon="@drawable/icon",
		DataScheme="content",
		DataPathPattern=@".*\\.gwc",
		DataMimeType="application/octet-stream")]
//	// For http
//	[IntentFilter (new[]{Intent.ActionView},
//		Categories=new[]{Intent.CategoryDefault, Intent.CategoryBrowsable},
//		Icon="@drawable/icon",
//		DataScheme="http",
//		DataHost="*",
//		DataPathPattern=@".*\\.gwc",
//		DataMimeType="*/*")]
//	// For https
//	[IntentFilter (new[]{Intent.ActionView},
//		Categories=new[]{Intent.CategoryOpenable, Intent.CategoryLauncher, Intent.CategoryDefault, Intent.CategoryBrowsable},
//		Icon="@drawable/icon",
//		DataScheme="https",
//		DataHost="*",
//		DataPathPattern=@".*\\.gwc",
//		DataMimeType="*/*")]
	// For files
	[IntentFilter (new[]{Intent.ActionView},
		Categories=new[]{Intent.CategoryDefault},
		Icon="@drawable/icon",
		DataScheme="file",
		DataHost="*",
		DataPathPattern=@".*\\.gwc",
		DataMimeType="*/*")]
	public class FileImportActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Get adress of file from intent
			Android.Net.Uri data = this.Intent.Data;

			// If there are any information
			if (data != null) {
				// Clear data, because we have it allready in data
				this.Intent.SetData(null);
				var cguid = data.GetQueryParameter("CGUID");
				// Try to import the data
				try {
					string fileName = ImportData(data);
					if (fileName != null) {
					} else {
						Finish();
						return;
					}
				} 
				catch (Exception e) {
					// warn user about bad data here
					Finish(); 
					return;
				}

				// launch MainActivity (with FLAG_ACTIVITY_CLEAR_TOP)
				Intent intent = new Intent(this, typeof(MainActivity));

				intent.AddFlags(ActivityFlags.SingleTop);
				intent.AddFlags(ActivityFlags.ClearTop);

				StartActivity(intent);
			}
		}

		string ImportData(Android.Net.Uri uri) 
		{
			string scheme = uri.Scheme;

			if(ContentResolver.SchemeContent.Equals(scheme) || ContentResolver.SchemeFile.Equals(scheme)) {
				// Now we have the path and filename of the new file
				string fileName = GetFileNameByUri(this, uri);
				var fis = new BinaryReader(ContentResolver.OpenInputStream(uri));
				var fos = new FileStream(Path.Combine(Main.Path, fileName), FileMode.CreateNew);
				fis.BaseStream.CopyTo(fos);
				fos.Close();
				fis.Close();
				return fileName;
			}

			return null;
		}

		string GetFileNameByUri(Context context, Android.Net.Uri uri)
		{
			string fileName = "unknown";
			Android.Net.Uri filePathUri = uri;

			if (ContentResolver.SchemeContent.Equals(uri.Scheme))
			{      
				Android.Database.ICursor cursor = ContentResolver.Query(uri, null, null, null, null);
				if (cursor.MoveToFirst())
				{
					int column_index = cursor.GetColumnIndexOrThrow("_data");
					filePathUri = Android.Net.Uri.Parse(cursor.GetString(column_index));
					fileName = filePathUri.LastPathSegment.ToString();
				}
			}
			else if (ContentResolver.SchemeFile.Equals(uri.Scheme))
			{
				fileName = filePathUri.LastPathSegment.ToString();
			}
			else
			{
				fileName = fileName+"_"+filePathUri.LastPathSegment;
			}
			return fileName;
		}
	}
}

