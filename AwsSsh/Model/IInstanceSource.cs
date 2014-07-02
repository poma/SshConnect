using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace AwsSsh
{
	public interface IInstanceSource
	{
		string Name { get; }
		List<Instance> GetInstanceList();
		SettingsBase Settings { get; set; }
		Control SettingsControl { get; }
	}	
}
