using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public interface ITimeService
	{
		bool IsRelativelyOld( DateTime current, DateTime max );
	}
}
