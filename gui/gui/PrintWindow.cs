using System;
using System.Net;
using System.Net.WebSockets;

using Gtk;

namespace gui
{
	public partial class PrintWindow : Gtk.Window
	{
		public PrintWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			/*		this.Fullscreen ();

			WebClient ConnectToNodejs = new WebClient (); 
			ConnectToNodejs.DownloadString ("http://10.29.0.67:3000/start_print?url=http://data01.gratisupload.dk/f/8rhnaqxm59.gcode&uuid=55330343534351103222");
*/		}

		protected void OnButton1Clicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

	}
}
