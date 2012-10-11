using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace AwsSsh.Plugins.Putty
{
	public class PuttyInstanceSource : InstanceSource
	{
		private List<string> puttySessions;

		public override List<Instance> GetInstanceList()
		{
			if (puttySessions == null)
				puttySessions = Microsoft.Win32.Registry.CurrentUser
					.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions").GetSubKeyNames()
					.Select(s => s.Replace("%20", " ")).ToList();
			return puttySessions.Select(s => new PuttyInstance(s)).OfType<Instance>().ToList();
		}

		public override void MergeInstanceList(ObservableCollection<Instance> src, List<Instance> newList)
		{
			var names = src.Select(a => a.Name);
			newList.ForEach(s => { if (!names.Contains(s.Name)) src.Add(s); });
		}
	}
}
