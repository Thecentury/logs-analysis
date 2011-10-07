using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Logging;

namespace LogAnalyzer.Kernel
{
	/// <summary>
	/// Логирует полученные ошибки.
	/// </summary>
	public sealed class LoggingErrorReportingService : IErrorReportingService
	{
		private readonly Logger logger;

		public LoggingErrorReportingService( Logger logger )
		{
			if ( logger == null ) throw new ArgumentNullException( "logger" );

			this.logger = logger;
		}

		public void ReportException( Exception exc )
		{
			logger.WriteLine( MessageType.Error, String.Format( "Exception {0}", exc ) );
		}
	}
}
