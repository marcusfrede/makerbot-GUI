using System;
using Gtk;
using Pango;
using Newtonsoft.Json;
using System.Collections.Generic;
using GLib;
using System.Globalization;
using System.Net;
using MakerBotNameSpace;

namespace gui
{
	public partial class makerlab : Gtk.Window
	{
	
		public int fin;
		public int ps = 0;
		public JsonDownloader jdown;
		public List<MakerBot> jsonlist;
		WebClient ConnectToNodejs;


		public makerlab (string[] args) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.SetSizeRequest (320, 240);
			//	this.Fullscreen ();
			jdown = new JsonDownloader ("http://v2.asemakerlab.au.dk/api/Printers");
			ConnectToNodejs = new WebClient ();
			switch (args [0]) {
			case "A":
				ps = 0;
				break;
			case "B":
				ps = 1;
				break;
			case "C":
				ps = 2;
				break;
			case "D":
				ps = 3;
				break;
			} 
			SetupLabelUi ();

			StartClock ();
		}

		public void SetupLabelUi ()
		{
			Gdk.Color col = new Gdk.Color ();

			switch (ps) {
			case 0: 
				col = new Gdk.Color (255, 23, 68);  //Pink    A400
				break;

			case 1:
				col = new Gdk.Color (0, 229, 255);  //Cyan   A400
				break;

			case 2:
				col = new Gdk.Color (0, 230, 118);  //Green  A400
				break;

			case 3:
				col = new Gdk.Color (255, 234, 0);  //Yellow A400
				break;

			case 4:
				col = new Gdk.Color (41, 121, 255); //Blue   A400
				break;
			}

			Pango.FontDescription fontDesc = new Pango.FontDescription ();
			fontDesc.Family = "Ringbearer";
			fontDesc.Size = 40 * (int)Pango.Scale.PangoScale;
			fontDesc.Weight = Pango.Weight.Normal;

			labelPrinterName.SetAlignment (0, 0);
			labelPrinterName.ModifyFont (fontDesc);
			labelPrinterName.ModifyFg (Gtk.StateType.Normal, col);

			Pango.FontDescription fontPrint = new Pango.FontDescription ();
			fontPrint.Family = "areal";
			fontPrint.Size = 16 * (int)Pango.Scale.PangoScale;

			Pango.FontDescription fontinfo = new Pango.FontDescription ();
			fontinfo.Family = "areal";
			fontinfo.Size = 12 * (int)Pango.Scale.PangoScale;

			Gdk.Color fontcol = new Gdk.Color ();
			Gdk.Color.Parse ("white", ref fontcol);

			labelNextPrint.ModifyFont (fontPrint);
			labelNextPrint.ModifyFg (StateType.Normal, fontcol);

			labelNozzleTemp.ModifyFont (fontinfo);
			labelNozzleTemp.ModifyFg (StateType.Normal, fontcol);

			labelBpTemp.ModifyFont (fontinfo);
			labelBpTemp.ModifyFg (StateType.Normal, fontcol);

			labelBooked.ModifyFont (fontinfo);
			labelBooked.ModifyFg (StateType.Normal, fontcol);
		}



		void StartClock ()
		{
			GLib.Timeout.Add (5000, new GLib.TimeoutHandler (update_status));
		}



		bool update_status ()
		{
			try {
				jsonlist = jdown.Download ();

				labelPrinterName.Text = jsonlist[ps].Name;
				labelNozzleTemp.Text = "Nozzle temp: " + jsonlist [ps].NozzleTemperature.ToString () + " \u00B0C";
				labelBpTemp.Text = "Bed temp: " + jsonlist [ps].BedTemperature.ToString () + " \u00B0C";

				DateTime NowDateTime = DateTime.Now;

				for (int i = 0; i < jsonlist [ps].Bookings.Length; i++) {
				
					String StringStartTime = jsonlist [ps].Bookings [i].StartTime;
					String StringEndTime = jsonlist [ps].Bookings [i].EndTime;

					DateTime StartDateTime = new DateTime ();
					StartDateTime = DateTime.Parse (StringStartTime, null, System.Globalization.DateTimeStyles.RoundtripKind);

					DateTime EndDateTime = new DateTime ();
					EndDateTime = DateTime.Parse (StringEndTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
					
					if ((NowDateTime > StartDateTime) && (NowDateTime < EndDateTime)) {
						fin = i;
						labelBooked.Text = "Booket af: " + jsonlist [ps].Bookings [fin].User.FirstName + " fra: " + jsonlist [ps].Bookings [fin].StartTime.Substring (11, 5) + " - " + jsonlist [ps].Bookings [fin].EndTime.Substring (11, 5) + " " + jsonlist [ps].Bookings [fin].EndTime.Substring (8, 2) + "/" + jsonlist [ps].Bookings [fin].EndTime.Substring (5, 2);
						if(jsonlist[ps].Bookings[fin].File != null)
							labelNextPrint.Text = "Print: " + jsonlist [ps].Bookings [fin].File.FileName.ToString ();

					} else{
						labelBooked.Text = "Ikke booket lige nu";
						labelNextPrint.Text = "Intet print";
					}
				}

				if (jsonlist [ps].Printing == true) {
					progressbarPrint.Show ();
					labelNozzleTemp.Show ();
					labelBpTemp.Show ();

					if(jsonlist[ps].Bookings[fin].File != null)
					{
					double max = jsonlist [ps].Bookings [fin].File.NumberOfLines;
					double cur = jsonlist [ps].CurrentLine;
					double per = cur / max;

					progressbarPrint.Fraction = per;
					progressbarPrint.Text = per.ToString ("P1", CultureInfo.InvariantCulture);
					}

					buttonStart.Hide ();

					buttonStop.Show ();
				} else {
					progressbarPrint.Hide (); 
					buttonStop.Hide ();
				}
		
			} catch (Exception ex) {
				Console.WriteLine ("Exception source: {0}", ex);
				buttonStart.Hide ();
				buttonStop.Hide ();
				progressbarPrint.Hide ();
				labelNozzleTemp.Hide ();
				labelBpTemp.Hide ();
				labelNextPrint.Text = "Ingen forbindelse\ntil server!";
			}
				
			return true;
		}



		protected void OnButtonStartClicked (object sender, EventArgs e)
		{
			var StartString = "http://v2.asemakerlab.au.dk/api/Printers/" + jsonlist [ps].Id.ToString () + "/StartBooking/" + jsonlist [ps].Bookings [fin].Id.ToString ();
			try {
				Console.WriteLine (StartString);
				if(jsonlist[ps].Bookings[fin].File != null)
				ConnectToNodejs.DownloadString (StartString);	
			} catch (Exception ex) {
				Console.WriteLine ("Exception source: {0}", ex);
			}
		}

		protected void OnButtonStopClicked (object sender, EventArgs e)
		{
			var CancelString = "http://v2.asemakerlab.au.dk/api/Printers/" + jsonlist [ps].Id.ToString () + "/CancelPrint";
			Console.WriteLine (CancelString);
			ConnectToNodejs.DownloadString (CancelString);		
		}


	}
}
	




	










