using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using AwsSsh.Plugins.Chef;
using System.Windows.Controls;

namespace AwsSsh.Plugins.Putty
{
	[Serializable]
	public class PuttyInstanceSource : IInstanceSource
	{
		public string Name { get { return "Putty"; } }

		public SettingsBase Settings
		{
			get { return null; }
			set { }
		}

		public Control SettingsControl
		{
			get	{ return null; }
		}

		private List<string> puttySessions;

		public List<Instance> GetInstanceList()
		{
			if (puttySessions == null)
				puttySessions = Registry.CurrentUser
					.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions").GetSubKeyNames()
					.Select(s => s.Replace("%20", " ")).ToList();
			return puttySessions.Select(s => new PuttyInstance(this, s)).OfType<Instance>().ToList();
		}
	}
}
