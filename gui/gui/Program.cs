using System;
using Gtk;
using Pango;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using MonoDevelop;

namespace gui
{
	class Makerlab
	{

		Window win;

		public static void Main (string[] args)
		{
			Application.Init ();
			new Makerlab (args);
			Application.Run ();
		}
			
		Makerlab(string[] args)
		{
			win = new makerlab(args);
			win.SetDefaultSize (320, 240);

			win.DeleteEvent += OnWinDelete;
			win.Decorated = false;

			Gdk.Color col = new Gdk.Color();
			Gdk.Color.Parse("black", ref col);
			win.ModifyBg(StateType.Normal, col);

			win.ShowAll ();
		}
			
		void OnWinDelete (object o, DeleteEventArgs args){
			Application.Quit ();
		}

	}



}
