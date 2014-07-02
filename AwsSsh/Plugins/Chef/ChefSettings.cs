using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AwsSsh.ApplicationSettings;

namespace AwsSsh.Plugins.Chef
{
	public class ChefSettings : SettingsBase
	{
		public ChefSettings() : base(typeof(ChefSettings)) { }

		[DefaultValue(@"")]
		public string ChefUrl { get; set; }

		[DefaultValue(@"")]
		public string ChefKey { get; set; }

		[DefaultValue("poma")]
		public string ChefUser { get; set; }
	}
}
