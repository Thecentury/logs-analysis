using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	public sealed class ErrorReportingService : IErrorReportingService
	{
		public void ReportError( Exception exc, string message )
		{
			ErrorOccured.Raise( this, new ErrorOccuredEventArgs( exc, message ) );
		}

		public event EventHandler<ErrorOccuredEventArgs> ErrorOccured;
	}
}
