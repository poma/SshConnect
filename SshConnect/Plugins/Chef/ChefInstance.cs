using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SshConnect.Plugins.Chef
{
	[DebuggerDisplay("Name = {_name}, Endpoint = {endpoint}")]
	public class ChefInstance:Instance
	{
		private ChefSettings Settings { get { return Source.Settings as ChefSettings; } }

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
			var session = string.IsNullOrWhiteSpace(App.Settings.PuttySession) ? "" : String.Format("-load \"{0}\"", App.Settings.PuttySession);
			RunPutty(String.Format(@"{0} -ssh {1} -l {2} -i ""{3}"" {4}", session, Endpoint, Settings.SshUser, Settings.SshKey, Settings.SshArguments));
			return true;
		}
	}
}
