using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace LogAnalyzer.Logging
{
	public static class LoggerExtensions
	{
		public static void WriteInfo( this Logger logger, string message )
		{
			if ( logger == null )
				return;

			logger.WriteLine( MessageType.Info, message );
		}

		[Conditional( "DEBUG" )]
		public static void DebugWriteInfo( this Logger logger, string message )
		{
			WriteInfo( logger, message );
		}

		[StringFormatMethod( "format" )]
		public static void WriteInfo( this Logger logger, string format, params object[] parameters )
		{
			if ( logger == null )
				return;

			string message = String.Format( format, parameters );
			logger.WriteLine( MessageType.Info, message );
		}

		[StringFormatMethod( "format" )]
		[Conditional( "DEBUG" )]
		public static void DebugWriteInfo( this Logger logger, string format, params object[] parameters )
		{
			WriteInfo( logger, format, parameters );
		}

		[Conditional( "DEBUG" )]
		public static void DebugWriteDebug( this Logger logger, string message )
		{
			if ( logger == null )
			{
				return;
			}

			logger.WriteLine( MessageType.Debug, message );
		}

		[StringFormatMethod( "format" )]
		[Conditional( "DEBUG" )]
		public static void DebugWriteDebug( this Logger logger, string format, params object[] parameters )
		{
			if ( logger == null )
			{
				return;
			}

			string message = String.Format( format, parameters );
			logger.WriteLine( MessageType.Debug, message );
		}

		public static void WriteError( this Logger logger, string message )
		{
			if ( logger == null )
				return;

			logger.WriteLine( MessageType.Error, message );
		}

		[StringFormatMethod( "format" )]
		public static void WriteError( this Logger logger, string format, params object[] parameters )
		{
			if ( logger == null )
				return;

			string message = String.Format( format, parameters );
			logger.WriteLine( MessageType.Error, message );
		}

		public static void WriteException( this Logger logger, Exception exc )
		{
			if ( logger == null )
				return;

			logger.WriteLine( MessageType.Error, exc.ToString() );
		}

		public static void WriteVerbose( this Logger logger, string message )
		{
			if ( logger == null )
				return;

			logger.WriteLine( MessageType.Verbose, message );
		}

		[Conditional( "DEBUG" )]
		public static void DebugWriteVerbose( this Logger logger, string message )
		{
			WriteVerbose( logger, message );
		}

		[StringFormatMethod( "format" )]
		public static void WriteVerbose( this Logger logger, string format, params object[] parameters )
		{
			if ( logger == null )
				return;

			string message = String.Format( format, parameters );
			logger.WriteLine( MessageType.Verbose, message );
		}

		[StringFormatMethod( "format" )]
		[Conditional( "DEBUG" )]
		public static void DebugWriteVerbose( this Logger logger, string format, params object[] parameters )
		{
			WriteVerbose( logger, format, parameters );
		}
	}
}
