using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AwsSsh.Plugins.Chef
{
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

		public override string ClipboardText
		{
			get { return Endpoint; }
		}

		public override string Tooltip
		{
			get { return "Endpoint: " + Endpoint; }
		}

		public override StateColor StateColor
		{
			get { return StateColor.Blue; }
			set { }
		}
	}
}
