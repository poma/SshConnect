using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace AwsSsh.Plugins.Chef
{
	public class ChefInstanceSource:IInstanceSource
	{
		public List<Instance> GetInstanceList()
		{
			string jsonString = ChefClient.ChefRequest("/search/node");
			var json = new JsonObject(Json.JsonDecode(jsonString));
			var instances = json["rows"].ToArray()
				.Select(s => new ChefInstance
				{
					Name = (string)s["name"],
					Endpoint = (string)(s["automatic"]["cloud"] != null ? s["automatic"]["cloud"]["public_ipv4"] : s["automatic"]["ipaddress"]),
					LastUpdate = DateTimeFromUnixTime((int)s["automatic"]["ohai_time"])
				})
				.Cast<Instance>().ToList();
			return instances;
		}

		private static DateTime DateTimeFromUnixTime(int unixTime)
		{
			return new DateTime(1970,1,1,0,0,0,0).AddSeconds(unixTime).ToLocalTime();
		}
	}
}