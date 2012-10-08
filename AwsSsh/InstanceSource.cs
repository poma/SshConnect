using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AwsSsh
{
	public class InstanceSource
	{
		public virtual void RefreshList(ObservableCollection<Instance> list)
		{
			
		}

		public virtual Type[] SerializedTypes
		{
			get { return new Type[] { typeof(AmazonInstance) }; }
		}
	}
}
