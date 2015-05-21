using System;
using Gtk;
using Pango;
using Newtonsoft.Json;
using System.Collections.Generic;
using GLib;
using System.Globalization;
using System.Net;
using Example;

namespace gui
{
	public partial class makerlab : Gtk.Window
	{
	
		public int fin;
		public int updateCount = 0;
		public int ps = 0;
		public JsonDownloader jdown;
		public List<SampleResponse1> jsonlist;

		public makerlab (string[] args) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.SetSizeRequest (320, 240);
		//	this.Fullscreen ();
			jdown = new JsonDownloader();
		

			switch (args[0] ) 
			{
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



			SetupLabelUi();
			StartClock();
		}

		public void SetupLabelUi()
		{
			Gdk.Color col = new Gdk.Color();

			switch (ps) 
			{
			case 0: 
				col = new Gdk.Color (255, 23, 68);  //Pink    A400
				break;
				case 1:
					col = new Gdk.Color(0,229,255);  //Cyan   A400
				break;

				case 2:
					col = new Gdk.Color(0,230,118);  //Green  A400
				break;

				case 3:
					col = new Gdk.Color(255,234,0);  //Yellow A400
				break;

				case 4:
					col = new Gdk.Color(41,121,255); //Blue   A400
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

			Gdk.Color fontcol = new Gdk.Color();
			Gdk.Color.Parse("white", ref fontcol);

			buttonStart.BorderWidth = 0;

			labelNextPrint.ModifyFont (fontPrint);
			labelNextPrint.ModifyFg (StateType.Normal, fontcol);

			label2.ModifyFont (fontPrint);
			label2.ModifyFg (StateType.Normal, fontcol);

			labelNozzleTemp.ModifyFont (fontinfo);
			labelNozzleTemp.ModifyFg (StateType.Normal, fontcol);

			labelBpTemp.ModifyFont (fontinfo);
			labelBpTemp.ModifyFg (StateType.Normal, fontcol);

			label5.ModifyFg (StateType.Normal, fontcol);


			labelBooked.ModifyFont (fontinfo);
			labelBooked.ModifyFg (StateType.Normal, fontcol);

		}
			


	    

		void StartClock()
		{
			GLib.Timeout.Add (5000, new GLib.TimeoutHandler(update_status));
		}


		bool update_status ()
		{
			updateCount++;
			label5.Text = "update nr: " + updateCount;



			try
			{
			    jsonlist = jdown.Download();

				labelPrinterName.Text = jsonlist[ps].Name;
				labelNozzleTemp.Text = "Nozzle temp: " + jsonlist[ps].NozzleTemperature.ToString() + " \u00B0C";
				labelBpTemp.Text = "Bed temp: " + jsonlist[ps].BedTemperature.ToString() + " \u00B0C";


		
					DateTime NowDateTime = DateTime.Now;


					for (int i=0; i< jsonlist[ps].Bookings.Length; i++)
					{

						String StringStartTime = jsonlist[ps].Bookings[i].StartTime;

						DateTime StartDateTime;
						StartDateTime = new DateTime();
						StartDateTime = DateTime.Parse(StringStartTime,  null, System.Globalization.DateTimeStyles.RoundtripKind);

						String StringEndTime = jsonlist[ps].Bookings[i].EndTime;

						DateTime EndDateTime;
						EndDateTime = new DateTime();
						EndDateTime = DateTime.Parse(StringEndTime,  null, System.Globalization.DateTimeStyles.RoundtripKind);

					
						if ((NowDateTime > StartDateTime) && (NowDateTime < EndDateTime))
						{
							fin = i;
							labelBooked.Text = "Booket af: " + jsonlist[ps].Bookings[fin].User.FirstName  + " fra: " + jsonlist[ps].Bookings[fin].StartTime.Substring(11,5) + " - " + jsonlist[ps].Bookings[fin].EndTime.Substring(11,5) + " " + jsonlist[ps].Bookings[fin].EndTime.Substring(8,2) + "/" + jsonlist[ps].Bookings[fin].EndTime.Substring(5,2);
							labelNextPrint.Text = "Print: " + jsonlist[ps].Bookings[fin].File.FileName.ToString() ;

					}
						else
							labelBooked.Text = "Ikke booket lige nu";
					}


		

				if (jsonlist[ps].Printing == true)
				{
					progressbarPrint.Show();
					labelNozzleTemp.Show();
					labelBpTemp.Show();

					progressbarPrint.Pulse();

					double max = jsonlist[ps].Bookings[fin].File.NumberOfLines;
					double cur = jsonlist [ps].CurrentLine;
					double per = cur / max;

					progressbarPrint.Fraction = per;
					progressbarPrint.Text = per.ToString("P1", CultureInfo.InvariantCulture);

					buttonStart.Hide();

					buttonStop.Show();
				} else 
				{
					progressbarPrint.Hide(); 
					buttonStop.Hide();
				}
		
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception source: {0}", e);
				buttonStart.Hide ();
				buttonStop.Hide ();
				progressbarPrint.Hide ();
				labelNozzleTemp.Hide ();
				labelBpTemp.Hide ();
				labelNextPrint.Text = "No connection\nto node.js server";
			}
				
			return true;
		}
			


		protected void OnButtonStartClicked (object sender, EventArgs e)
		{
			try
			{
				Console.WriteLine("http://v2.asemakerlab.au.dk/api/Printers/" + jsonlist[ps].Id.ToString() + "/StartBooking/" + jsonlist[ps].Bookings[fin].Id.ToString() );
				WebClient ConnectToNodejs = new WebClient (); 
				ConnectToNodejs.DownloadString ("http://v2.asemakerlab.au.dk/api/Printers/" + jsonlist[ps].Id.ToString() + "/StartBooking/" + jsonlist[ps].Bookings[fin].Id.ToString() );	
			}
			catch (Exception r)
			{
				Console.WriteLine("Exception source: {0}", r);
			}
		}

		protected void OnButtonStopClicked (object sender, EventArgs e)
		{
			Console.WriteLine("http://v2.asemakerlab.au.dk/api/Printers/" + jsonlist[ps].Id.ToString() + "/CancelPrint");
			WebClient ConnectToNodejs = new WebClient (); 
			ConnectToNodejs.DownloadString ("http://v2.asemakerlab.au.dk/api/Printers/" + jsonlist[ps].Id.ToString() + "/CancelPrint");		
		}


}
}
	




	










