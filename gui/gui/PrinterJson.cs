using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Example;

namespace gui
{
/*	public class Printer
	{
		public string Uuid { get; set; }
		public bool isConnected { get; set; }
		public DateTime lastSeen { get; set; }
		public int client_id { get; set; }
		public Info info { get; set; }
			
		public class Info
		{
			public Temperatures temperatures { get; set; }

			public Status status { get; set; }
		}

		public class Temperatures
		{
			public int[] bed { get; set; }

			public int[] nozzle { get; set; }
		}

		public class Status
		{
			public bool Printing { get; set; }

			public int Current_line { get; set; }
		}
	}

		
*/



	public class JsonDownloader
	{
		WebDownload json;
		string DownloadedData;

	


		public JsonDownloader ()
		{
			json = new WebDownload ();
		//	DownloadedData = json.DownloadString ("http://10.29.0.67:3000");
		
		}

		public List<SampleResponse1> Download ()
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Include;
			DownloadedData = json.DownloadString ("http://v2.asemakerlab.au.dk/api/printers");
			return JsonConvert.DeserializeObject<List<SampleResponse1>> (DownloadedData, settings);
		
		}

	}








	public class WebDownload : WebClient
	{
		public int Timeout { get; set; }

		public WebDownload () : this (2000)
		{
		}

		public WebDownload (int timeout)
		{
			this.Timeout = timeout;
		}

		protected override WebRequest GetWebRequest (Uri address)
		{
			var request = base.GetWebRequest (address);
			if (request != null) {
				request.Timeout = this.Timeout;
			}
			return request;
		}
	}


}