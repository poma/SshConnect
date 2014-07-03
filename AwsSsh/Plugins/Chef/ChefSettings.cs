using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AwsSsh.Plugins.Chef
{
	public class ChefSettings : SettingsBase
	{
		[DefaultValue(@"http://example.com:4000")]
		public string ChefUrl { get; set; }

		[DefaultValue("C:\\chef.pem")]
		public string ChefKey { get; set; }

		[DefaultValue("root")]
		public string ChefUser { get; set; }

		[DefaultValue("root")]
		public string SshUser { get; set; }

		[DefaultValue("C:\\certificate.ppk")]
		public string SshKey { get; set; }

		[DefaultValue("")]
		public string SshArguments { get; set; }
	}
}
