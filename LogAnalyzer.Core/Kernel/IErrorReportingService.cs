using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public interface IErrorReportingService
	{
		void ReportException( Exception exc );
	}
}
