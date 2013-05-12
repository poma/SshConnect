using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AwsSsh.Plugins.Chef
{
	[DebuggerDisplay("Name = {_name}, Endpoint = {endpoint}")]
	public class ChefInstance:Instance
	{
		private string endpoint;
		public string Endpoint
		{
			get { return endpoint; }
			set
			{
				if (endpoint == value) return;
				endpoint = value;
				OnPropertyChanged("Endpoint");
			}
		}

		private DateTime lastUpdate;
		public DateTime LastUpdate
		{
			get { return lastUpdate; }
			set
			{
				if (lastUpdate == value) return;
				lastUpdate = value;
				StateColor = StateColor.Yellow;
				StateColor = lastUpdate.AddHours(1) < DateTime.Now ? StateColor.Red : StateColor.Blue;
				OnPropertyChanged("StateColor");
				OnPropertyChanged("LastUpdate");
			}
		}

		public override string ClipboardText
		{
			get { return Endpoint; }
		}

		public override string Tooltip
		{
			get { return "Endpoint: " + Endpoint; }
		}

		public ChefInstance()
		{
			StateColor = StateColor.Blue;
		}

		public override bool Run()
		{
			//if (string.IsNullOrEmpty(Endpoint)) return false; // offline instancces
			//var session = string.IsNullOrWhiteSpace(App.Settings.PuttySession) ? "" : String.Format("-load \"{0}\"", App.Settings.PuttySession);
			RunPutty(String.Format(@"-ssh {0} -P 220 -l root -i ""{1}""", Endpoint, App.Settings.KeyPath));
			return true;
		}
	}
}
