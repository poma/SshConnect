using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AwsSsh
{
	public interface IInstanceSource
	{
		List<Instance> GetInstanceList();
	}	
}
