using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AwsSsh.Plugins.Chef
{
	public class ChefSettings : SettingsBase
	{
		[DefaultValue(@"")]
		public string ChefUrl { get; set; }

		[DefaultValue(@"")]
		public string ChefKey { get; set; }

		[DefaultValue("poma")]
		public string ChefUser { get; set; }
	}
}
