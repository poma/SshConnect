using System;
using System.Linq;
using System.Collections.Generic;

namespace AwsSsh
{
	public class InstanceComparer : IEqualityComparer<Instance>
	{
		public bool Equals(Instance x, Instance y)
		{
			return x != null && y != null && x.Id == y.Id;
		}

		public int GetHashCode(Instance obj)
		{
			return obj.Id.GetHashCode();
		}
	}
}
