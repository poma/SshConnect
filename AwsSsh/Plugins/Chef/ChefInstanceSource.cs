using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AwsSsh.Plugins.Chef
{
	[Serializable]
	public class ChefInstanceSource:IInstanceSource
	{
		public string Name { get { return "Chef"; } }

		private ChefSettings _settings;
		public SettingsBase Settings
		{
			get
			{
				if (_settings == null)
					_settings = new ChefSettings();
				return _settings;
			}
			set { _settings = value as ChefSettings; }
		}

		[NonSerialized]
		private ChefSettingsControl _settingsControl;
		public Control SettingsControl
		{
			get
			{
				if (_settingsControl == null)
					_settingsControl = new ChefSettingsControl();
				return _settingsControl;
			}
		}
		

		public List<Instance> GetInstanceList()
		{
			string jsonString = new ChefClient(_settings).ChefRequest("/search/node");
			var json = new JsonObject(Json.JsonDecode(jsonString));
			var instances = json["rows"].ToArray()
				.Select(s => new ChefInstance
				{
					Source = this,
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