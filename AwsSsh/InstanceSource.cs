using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AwsSsh
{
	public class InstanceSource
	{
		public virtual List<Instance> GetInstanceList()
		{
			return null;
		}

		public virtual void MergeInstanceList(ObservableCollection<Instance> src, List<Instance> newList)
		{
			
		}
	}
}
