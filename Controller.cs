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
using WF.Player.Core;

namespace WF.Player.Android
{
	public class Controller
	{
		private Engine engine;

		public Controller(Cartridge cartridge)
		{
			if (cartridge == null)
				return;

			// Create engine
			//			engine = CreateEngine (cartridge);

			// Start cartridge
			//			if (cartRestore)
			//				Restore ();
			//			else
			//				Start ();

		}
	}
}

