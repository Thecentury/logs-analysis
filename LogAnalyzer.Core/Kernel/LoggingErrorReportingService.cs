using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Logging;

namespace LogAnalyzer.Kernel
{
	/// <summary>
	/// Реализация <see cref="IErrorReportingService"/>, которая логирует полученные ошибки.
	/// </summary>
	public sealed class LoggingErrorReportingService : IErrorReportingService
	{
		private readonly Logger logger;

		public LoggingErrorReportingService( Logger logger )
		{
			if ( logger == null ) throw new ArgumentNullException( "logger" );

			this.logger = logger;
		}

		public void ReportError( Exception exc, string message )
		{
			logger.WriteLine( MessageType.Error, String.Format( "{0}; Exception {1}", message, exc ) );
		}
	}
}
