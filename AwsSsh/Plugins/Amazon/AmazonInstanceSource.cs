using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace AwsSsh.Plugins.Amazon
{
	public class AmazonInstanceSource : InstanceSource
	{
		public override void RefreshList(ObservableCollection<Instance> list)
		{
			base.RefreshList(list);
		}
	}
}
