using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace AwsSsh.Plugins.Amazon
{
	public class AmazonInstanceSource : InstanceSource
	{
		public override List<Instance> GetInstanceList()
		{
			return AmazonClient.GetInstances().OfType<Instance>().ToList();
		}

		public override void MergeInstanceList(ObservableCollection<Instance> src, List<Instance> theNewList)
		{
			var newList = theNewList.OfType<AmazonInstance>().ToList();
			AmazonClient.MergeInstanceList(src, newList);
		}
	}
}
