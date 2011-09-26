using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging.MostLogsServices;
using System.Diagnostics;

namespace ModuleLogsProvider.Logging.Most
{
	internal sealed class MostLogMessagesStorage
	{
		private readonly Dictionary<string, OneFileMessages> logNamesToEntries = new Dictionary<string, OneFileMessages>();

		public IEnumerable<string> GetLogFileNames()
		{
			return logNamesToEntries.Keys;
		}

		public List<LogEntry> GetEntriesByName( string logFileName )
		{
			OneFileMessages fileMessages = logNamesToEntries[logFileName];
			return fileMessages.Entries;
		}

		public AppendMessagesResult AppendMessages( IEnumerable<LogMessageInfo> messages )
		{
			AppendMessagesResult result = new AppendMessagesResult();

			var groupsByLoggerName = messages.GroupBy( m => m.LoggerName );
			foreach ( IGrouping<string, LogMessageInfo> grouping in groupsByLoggerName )
			{
				string loggerName = grouping.Key;

				OneFileMessages fileMessages;
				if ( !logNamesToEntries.TryGetValue( loggerName, out fileMessages ) )
				{
					fileMessages = new OneFileMessages( loggerName );
					logNamesToEntries.Add( loggerName, fileMessages );
					result.CreatedLogFiles.Add( loggerName );
				}

				fileMessages.AppendMessages( grouping );
			}

			return result;
		}
	}

	public sealed class AppendMessagesResult
	{
		private readonly HashSet<string> createdLogFiles = new HashSet<string>();
		public HashSet<string> CreatedLogFiles
		{
			get { return createdLogFiles; }
		}
	}

	[DebuggerDisplay("Count = {entries.Count}")]
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

		public List<LogEntry> Entries
		{
			get { return entries; }
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
				Entries.Add( entry );
			}
		}
	}
}
