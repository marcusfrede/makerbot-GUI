using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using MakerBotNameSpace;

namespace gui
{
	public class JsonDownloader
	{
		WebDownload json;
		string DownloadedData;
		string Url;

		public JsonDownloader (string url)
		{
			json = new WebDownload ();
			Url = url;
		}

		public List<MakerBot> Download ()
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Include;
			DownloadedData = json.DownloadString (Url);
			return JsonConvert.DeserializeObject<List<MakerBot>> (DownloadedData, settings);
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