using System;
using System.Linq;
using System.Collections.Generic;

namespace AwsSsh
{
	public class AmazonInstanceComparer : IEqualityComparer<AmazonInstance>
	{
		public bool Equals(AmazonInstance x, AmazonInstance y)
		{
			return x != null && y != null && x.Id == y.Id;
		}

		public int GetHashCode(AmazonInstance obj)
		{
			return obj.Id.GetHashCode();
		}
	}
}
