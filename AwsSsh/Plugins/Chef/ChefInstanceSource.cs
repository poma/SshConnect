using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace AwsSsh.Plugins.Chef
{
	public class ChefInstanceSource:InstanceSource
	{
		public override List<Instance> GetInstanceList()
		{
			string jsonString = ChefClient.ChefRequest("/search/node");
			var json = new JsonObject(Json.JsonDecode(jsonString));
			var instances = json["rows"].ToArray()
				//.Where(s => !s["automatic"].ToDictionary().ContainsKey("cloud"))
				.Select(s => new ChefInstance
				{
					Name = (string)s["name"],
					Endpoint = (string)(s["automatic"]["cloud"] != null ? s["automatic"]["cloud"]["public_ipv4"] : s["automatic"]["ipaddress"]),
					LastUpdate = DateTimeFromUnixTime((int)s["automatic"]["ohai_time"])
				})
				.Cast<Instance>().ToList();
			//var instances2 = json["rows"].ToArray()
			//	.Where(s => s["automatic"].ToDictionary().ContainsKey("cloud"))
			//	.Select(s => new ChefInstance
			//	{
			//		Name = (string)s["name"],
			//		Endpoint = (string)s["automatic"]["ipaddress"],
			//		LastUpdate = DateTimeFromUnixTime((int)s["automatic"]["ohai_time"])
			//	})
			//	.Cast<Instance>().ToList();
			return instances;
		}

		public override void MergeInstanceList(ObservableCollection<Instance> src, List<Instance> newList)
		{
			var names = src.Select(a => a.Name);
			var newNames = newList.Select(a => a.Name);
			newList.ForEach(s => { if (!names.Contains(s.Name)) src.Add(s); });
			src.OfType<ChefInstance>().Where(s => !newNames.Contains(s.Name)).ToList().ForEach(s => src.Remove(s));
		}

		public static DateTime DateTimeFromUnixTime(int unixTime)
		{
			return new DateTime(1970,1,1,0,0,0,0).AddSeconds(unixTime).ToLocalTime();
		}
	}
}

//new DateTime(1970,1,1,0,0,0,0).AddSeconds((int)json["rows"].ToArray()[0]["automatic"]["ohai_time"]).ToLocalTime()
