using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Tests
{
	public sealed class MockTimeService : ITimeService
	{
		#region ITimeService Members

		public bool IsRelativelyOld( DateTime current, DateTime max )
		{
			return false;
		}

		#endregion
	}
}
