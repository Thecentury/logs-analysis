using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Logging;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	/// <summary>
	/// Реализация <see cref="ErrorReportingServiceBase"/>, которая логирует полученные ошибки.
	/// </summary>
	public sealed class LoggingErrorReportingService : ErrorReportingServiceBase
	{
		private readonly Logger logger;

		public LoggingErrorReportingService( Logger logger )
		{
			if ( logger == null ) throw new ArgumentNullException( "logger" );

			this.logger = logger;
		}

		protected override void ReportErrorCore( Exception exc, string message )
		{
			logger.WriteLine( MessageType.Error, String.Format( "{0}; Exception {1}", message, exc ) );
		}
	}
}
