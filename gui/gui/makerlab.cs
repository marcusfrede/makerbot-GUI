using System;
using Gtk;
using Pango;
using Newtonsoft.Json;
using System.Collections.Generic;
using GLib;
using System.Globalization;
using System.Net;

namespace gui
{
	public partial class makerlab : Gtk.Window
	{
	
		public int fin = 0;

		public int updateCount = 0;
		public static int ps = 0;
		public JsonDownloader jdown;

		public makerlab (string[] args) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
		//	this.SetSizeRequest (320, 240);
			this.Fullscreen ();
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
			LoadPrintFilesToList();
			StartClock();
		}

		public void SetupLabelUi()
		{
			Gdk.Color col = new Gdk.Color();

			switch (ps) 
			{
			case 0: 
				col = new Gdk.Color (255, 23, 68);  //Pink   A400
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
			

		public void LoadPrintFilesToList()
		{
			string[ ] files = System.IO.Directory.GetFiles("/home/ikn/gui/gui/PrintFiles");

			for ( int x = 0 ; x < files.Length ; x++ )
			{
				comboboxPrintSelect.InsertText (x, files[x].Substring(29));
			}		
		}
	    

		void StartClock()
		{
			GLib.Timeout.Add (5000, new GLib.TimeoutHandler(update_status));
		}


		bool update_status ()
		{
			updateCount++;
		//	label5.Hide ();
			label5.Text = "update nr: " + updateCount;
			labelNextPrint.Text = "Next print: " + comboboxPrintSelect.ActiveText;
			label6.Hide();


			if (comboboxPrintSelect.ActiveText == null)
				buttonStart.Hide ();
			else {
				buttonStart.Show ();
			}

			try
			{
				 

				var jsonlist = jdown.Download();

				labelPrinterName.Text = jsonlist[ps].Name;
				labelNozzleTemp.Text = "Nozzle temp: " + jsonlist[ps].NozzleTemperature.ToString();   //.info.temperatures.nozzle[0].ToString() + " \u00B0C";
				labelBpTemp.Text = "Bed temp: " + jsonlist[ps].BedTemperature.ToString(); // booking[0].info.temperatures.bed[0].ToString() + " \u00B0C";
		
				if(jsonlist[ps].Bookings != null)
				{
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
							labelBooked.Text = "Booket af: " + jsonlist[ps].Bookings[0].User.FirstName  + " Fra: " + jsonlist[ps].Bookings[fin].StartTime.Substring(11) + " - " + jsonlist[ps].Bookings[fin].EndTime.Substring(11) + " Dato: " + jsonlist[ps].Bookings[fin].EndTime.Substring(8,2) + "/" + jsonlist[ps].Bookings[fin].EndTime.Substring(5,2);

						}
						else
							labelBooked.Text = "Ikke flere bookinger i dag :)"; //jsonlist[ps].Bookings.Length -1;
					}


				}
				else 
					labelBooked.Text = "Booket af: N/A" ;





				if (jsonlist[0].Printing == true) // .info.status.Printing == true) 
				{
					progressbarPrint.Show();
					labelNozzleTemp.Show();
					labelBpTemp.Show();

					progressbarPrint.Pulse();

					double max = 22066;
					double cur = jsonlist [ps].CurrentLine; //.info.status.Current_line;
					double per = cur / max;

					progressbarPrint.Fraction = per;
					progressbarPrint.Text = per.ToString("P1", CultureInfo.InvariantCulture);

					comboboxPrintSelect.Hide();
					buttonStart.Hide();

					//		label6.Text = booking[ps].info.status.Current_line.ToString();
					buttonStop.Show();
				} else 
				{
					progressbarPrint.Hide(); 
					comboboxPrintSelect.Show();
					buttonStop.Hide();
				}
		
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception source: {0}", e);
				buttonStart.Hide ();
				buttonStop.Hide ();
				comboboxPrintSelect.Hide ();
				progressbarPrint.Hide ();
				labelNozzleTemp.Hide ();
				labelBpTemp.Hide ();
				labelNextPrint.Text = "No connection\nto node.js server";
			}
				
			return true;
		}
			


		protected void OnButton2Clicked (object sender, EventArgs e)
		{
			WebClient ConnectToNodejs = new WebClient (); 
			if (labelNextPrint.Text == "Next print: crep.gcode")
				ConnectToNodejs.DownloadString("http://10.29.0.67:3000/start_print?url=http://data01.gratisupload.dk/f/8rhnaqxm59.gcode&uuid=55330343434351D072C1");
			if (labelNextPrint.Text == "Next print: crep4.gcode")
				ConnectToNodejs.DownloadString("http://10.29.0.67:3000/start_print?url=http://data01.gratisupload.dk/f/8rmeoklxpi.gcode&uuid=55330343434351D072C1");
		}


		protected void OnCombobox1Changed (object sender, EventArgs e)
		{
			labelNextPrint.Text = "Next print: " + comboboxPrintSelect.ActiveText;
			buttonStart.Show ();
		}	

		protected void OnButton3Clicked (object sender, EventArgs e)
		{
			WebClient ConnectToNodejs = new WebClient (); 
			ConnectToNodejs.DownloadString("http://10.29.0.67:3000/cancel_print?uuid=55330343434351D072C1");		
		}

		










}
}
	




	










