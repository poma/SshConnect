using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AwsSsh
{
	public abstract class InstanceSource
	{
		public abstract List<Instance> GetInstanceList();
		public abstract void MergeInstanceList(ObservableCollection<Instance> src, List<Instance> newList);
	}

	
}
