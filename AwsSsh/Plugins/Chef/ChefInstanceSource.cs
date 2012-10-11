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
				.Where(s => !s["automatic"].ToDictionary().ContainsKey("cloud"))
				.Select(s => new ChefInstance
				{
					Name = (string)s["name"],
					Endpoint = (string)s["automatic"]["fqdn"]
				})
				.Cast<Instance>().ToList();
			return instances;
		}

		public override void MergeInstanceList(ObservableCollection<Instance> src, List<Instance> newList)
		{
			var names = src.Select(a => a.Name);
			var newNames = newList.Select(a => a.Name);
			newList.ForEach(s => { if (!names.Contains(s.Name)) src.Add(s); });
			src.OfType<ChefInstance>().Where(s => !newNames.Contains(s.Name)).ToList().ForEach(s => src.Remove(s));
		}
	}
}
