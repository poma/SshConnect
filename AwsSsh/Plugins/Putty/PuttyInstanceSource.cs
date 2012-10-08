using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AwsSsh.Plugins.Putty
{
	public class PuttyInstanceSource
	{
		public List<string> PuttySessions { get; private set; }

		//public void GetPuttySessions()
		//{
		//	PuttySessions = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions").GetSubKeyNames().ToList();
		//	if (!App.Settings.IncludePuttySessionsInList) return;
		//	foreach (var s in PuttySessions)
		//	{
		//		var inst = new AmazonInstance
		//		{
		//			IsPuttyInstance = true,
		//			State = InstatnceStates.Unknown,
		//			StateName = "Unknown",
		//			Name = s,
		//			Id = s
		//		};
		//		Instances.Add(inst);
		//	}
		//}

		//void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		//{
		//	//if (e.PropertyName == "IncludePuttySessionsInList")
		//	//{
		//	//	Instances.Where(a => a.IsPuttyInstance).ToList().ForEach(a => Instances.Remove(a));
		//	//	if (Settings.IncludePuttySessionsInList)
		//	//		GetPuttySessions();
		//	//}
		//}

		//Settings.PropertyChanged += Settings_PropertyChanged;
	}
}
