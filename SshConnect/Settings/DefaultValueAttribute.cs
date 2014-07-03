using System;
using System.Collections.Generic;
using System.Linq;

namespace SshConnect
{
	[Serializable]
	public class DefaultValueAttribute : Attribute
	{
		public object DefaultValue { get; set; }

		public DefaultValueAttribute(object value)
		{
			DefaultValue = value;
		}
	}
}
