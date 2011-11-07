using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public sealed class NeverOldTimeService : ITimeService
	{
		public bool IsRelativelyOld( DateTime current, DateTime max )
		{
			return false;
		}
	}
}
