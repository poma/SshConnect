using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AwsSsh.Plugins.Putty
{
	public class PuttyInstance : Instance
	{
		public override bool Run()
		{
			RunPutty(String.Format("-load \"{0}\"", Name));
			return true;
		}

		public override string Tooltip
		{
			get
			{
				return "Putty saved session " + Name;
			}
		}

		public PuttyInstance()
		{
			
		}

		public PuttyInstance(string name)
		{
			this.Name = name;
		}
	}
}
