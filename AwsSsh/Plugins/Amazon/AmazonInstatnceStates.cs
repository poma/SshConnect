using System;
using System.Linq;
using System.Collections.Generic;

namespace AwsSsh
{
	public enum AmazonInstatnceStates
	{
		Unknown = -1,
		Pending = 0,
		Running = 16,
		ShuttingDown = 32,
		Terminated = 48,
		Stopping = 64,
		Stopped = 80,
		//Failing = 272
	}
}
