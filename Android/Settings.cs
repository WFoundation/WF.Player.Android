using System;
using System.Text;
using Vernacular;

namespace WF.Player.Android
{
	public class Values
	{
		static public float Frame = 10.0f;
		static public float ButtonHeight = 35.0f;
	}

	public class Colors
	{
	}

	public class Images
	{
	}

	public sealed class Strings
	{
		public static string TaskCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x93 } );         // UTF-8 2713
		public static string TaskNotCorrect = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x9C, 0x97 } );         // UTF-8 2717
		public static string Infinite = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x88, 0x9E } );                 // UTF-8 221E
		public static string Newline = "\n";
	}
}
