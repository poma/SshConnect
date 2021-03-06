﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SshConnect.Plugins.Amazon
{
	public class AmazonSettings : SettingsBase
	{
		[DefaultValue("Put your access key here")]
		public string AWSAccessKey { get; set; }

		[DefaultValue("Put your secret key here")]
		public string AWSSecretKey { get; set; }

		[DefaultValue("https://us-east-1.ec2.amazonaws.com")]
		public string ServiceUrl { get; set; }

		[DefaultValue("ubuntu")]
		public string SshUser { get; set; }

		[DefaultValue("C:\\certificate.ppk")]
		public string SshKey { get; set; }

		[DefaultValue("")]
		public string SshArguments { get; set; }
	}
}
