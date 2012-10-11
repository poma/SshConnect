using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace AwsSsh.Plugins.Chef
{
	public class ChefInstanceSource:InstanceSource
	{
		public override List<Instance> GetInstanceList()
		{
			throw new NotImplementedException();
		}

		public override void MergeInstanceList(ObservableCollection<Instance> src, List<Instance> newList)
		{
			throw new NotImplementedException();
		}

		public void ChefRequest()
		{
			
		}
	}
}
