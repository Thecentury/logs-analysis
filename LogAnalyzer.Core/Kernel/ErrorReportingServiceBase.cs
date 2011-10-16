using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	public abstract class ErrorReportingServiceBase
	{
		protected abstract void ReportErrorCore( Exception exc, string message );

		public void ReportError( Exception exc, string message )
		{
			ReportErrorCore( exc, message );
			ErrorOccured.Raise( this, new ErrorOccuredEventArgs( exc, message ) );
		}

		public event EventHandler<ErrorOccuredEventArgs> ErrorOccured;
	}
}
