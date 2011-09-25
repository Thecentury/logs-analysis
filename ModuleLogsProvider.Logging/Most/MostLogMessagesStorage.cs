using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Most
{
	internal sealed class MostLogMessagesStorage
	{
		private readonly Dictionary<string, OneFileMessages> logNamesToEntries = new Dictionary<string, OneFileMessages>();

		public void AppendMessages( IEnumerable<LogMessageInfo> messages )
		{
			var groupsByLoggerName = messages.GroupBy( m => m.LoggerName );
			foreach ( IGrouping<string, LogMessageInfo> grouping in groupsByLoggerName )
			{
				string loggerName = grouping.Key;
				OneFileMessages fileMessages = GetMessagesBy( loggerName );
				fileMessages.AppendMessages( grouping );
			}
		}

		private OneFileMessages GetMessagesBy( string loggerName )
		{
			OneFileMessages result;
			if ( !logNamesToEntries.TryGetValue( loggerName, out result ) )
			{
				result = new OneFileMessages( loggerName );
				logNamesToEntries.Add( loggerName, result );
			}

			return result;
		}
	}

	internal sealed class OneFileMessages
	{
		private readonly LogFile file;
		private readonly List<LogEntry> entries = new List<LogEntry>();

		private static readonly LogLineParser parser = new LogLineParser();

		public OneFileMessages( string name )
		{
			if ( String.IsNullOrEmpty( name ) ) throw new ArgumentNullException( "name" );

			file = LogFile.FromName( name );
		}

		public void AppendMessages( IEnumerable<LogMessageInfo> messages )
		{
			foreach ( LogMessageInfo logMessageInfo in messages )
			{
				string type;
				int threadId;
				DateTime time;
				string text;

				if ( !parser.TryExtractLogEntryData( logMessageInfo.Message, out type, out threadId, out time, out text ) )
				{
					// todo brinchuk ???
					throw new NotImplementedException();
				}
				LogEntry entry = new LogEntry( type, threadId, time, text, logMessageInfo.IndexInAllMessagesList, file );
				entries.Add( entry );
			}
		}
	}
}
