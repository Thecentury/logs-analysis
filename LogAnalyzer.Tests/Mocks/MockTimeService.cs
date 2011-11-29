using System;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests.Mocks
{
	public sealed class MockTimeService : ITimeService
	{
		public bool IsRelativelyOld( DateTime current, DateTime max )
		{
			return false;
		}
	}
}
