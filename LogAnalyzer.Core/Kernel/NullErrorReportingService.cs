using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public sealed class NullErrorReportingService : IErrorReportingService
	{
		public void ReportError( Exception exc, string message )
		{
			// do nothing here
		}
	}
}
