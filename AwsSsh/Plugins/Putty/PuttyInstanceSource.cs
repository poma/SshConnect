using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace AwsSsh.Plugins.Putty
{
	public class PuttyInstanceSource : IInstanceSource
	{
		private List<string> puttySessions;

		public List<Instance> GetInstanceList()
		{
			if (puttySessions == null)
				puttySessions = Registry.CurrentUser
					.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions").GetSubKeyNames()
					.Select(s => s.Replace("%20", " ")).ToList();
			return puttySessions.Select(s => new PuttyInstance(s)).OfType<Instance>().ToList();
		}
	}
}
