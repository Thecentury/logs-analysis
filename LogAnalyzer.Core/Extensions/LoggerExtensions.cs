using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogAnalyzer.Extensions
{
	public static class LoggerExtensions
	{
		public static void WriteInfo( this Logger logger, string message )
		{
			if ( logger == null )
				return;

			logger.WriteLine( message, MessageType.Info );
		}

		[Conditional( "DEBUG" )]
		public static void DebugWriteInfo( this Logger logger, string message )
		{
			WriteInfo( logger, message );
		}

		public static void WriteInfo( this Logger logger, string format, params object[] parameters )
		{
			if ( logger == null )
				return;

			string message = String.Format( format, parameters );
			logger.WriteLine( message, MessageType.Info );
		}

		[Conditional( "DEBUG" )]
		public static void DebugWriteInfo( this Logger logger, string message, params object[] parameters )
		{
			WriteInfo( logger, message, parameters );
		}

		public static void WriteError( this Logger logger, string message )
		{
			if ( logger == null )
				return;

			logger.WriteLine( message, MessageType.Error );
		}

		public static void WriteError( this Logger logger, string format, params object[] parameters )
		{
			if ( logger == null )
				return;

			string message = String.Format( format, parameters );
			logger.WriteLine( message, MessageType.Error );
		}

		public static void WriteVerbose( this Logger logger, string message )
		{
			if ( logger == null )
				return;

			logger.WriteLine( message, MessageType.Verbose );
		}

		[Conditional( "DEBUG" )]
		public static void DebugWriteVerbose( this Logger logger, string message )
		{
			WriteVerbose( logger, message );
		}


		public static void WriteVerbose( this Logger logger, string format, params object[] parameters )
		{
			if ( logger == null )
				return;

			string message = String.Format( format, parameters );
			logger.WriteLine( message, MessageType.Verbose );
		}

		[Conditional("DEBUG")]
		public static void DebugWriteVerbose( this Logger logger, string format, params object[] parameters )
		{
			WriteVerbose( logger, format, parameters );
		}
	}
}
