using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using SshConnect.Plugins.Chef;
using System.Windows.Controls;

namespace SshConnect.Plugins.Putty
{
	[Serializable]
	public class PuttyInstanceSource : IInstanceSource
	{
		public string Name { get { return "Putty"; } }

		private SettingsBase _settings;
		public SettingsBase Settings
		{
			get
			{
				if (_settings == null)
					_settings = new SettingsBase();
				return _settings;
			}
			set { _settings = value; }
		}

		public Control SettingsControl
		{
			get	{ return null; }
		}

		private List<string> puttySessions;

		public List<Instance> GetInstanceList()
		{
			if (!App.Settings.IncludePuttySessionsInList)
				return new List<Instance>();
			if (puttySessions == null)
				puttySessions = Registry.CurrentUser
					.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions").GetSubKeyNames()
					.Select(s => s.Replace("%20", " ")).ToList();
			return puttySessions.Select(s => new PuttyInstance(this, s)).OfType<Instance>().ToList();
		}
	}
}
