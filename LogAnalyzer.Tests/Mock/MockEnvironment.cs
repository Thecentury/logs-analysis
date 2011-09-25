using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;
using System.IO;

namespace LogAnalyzer.Tests
{
	public sealed class MockEnvironment : IEnvironment
	{
		private const string directoryName = "Dir";
		private readonly MockLogRecordsSource recordsSource = new MockLogRecordsSource( directoryName );
		private readonly List<MockDirectoryInfo> directories = null;
		internal List<MockDirectoryInfo> Directories
		{
			get { return directories; }
		}

		private readonly Encoding encoding = Encoding.Unicode;
		private readonly IOperationsQueue operationsQueue = null;
		private readonly ITimeService timeService = null;

		public MockEnvironment( LogAnalyzerConfiguration config ) : this( config, new WorkerThreadOperationsQueue( config.Logger ) ) { }

		public MockEnvironment( LogAnalyzerConfiguration config, IOperationsQueue operationsQueue )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );
			if ( operationsQueue == null )
				throw new ArgumentNullException( "operationsQueue;" );

			this.operationsQueue = operationsQueue;
			this.timeService = new MockTimeService();

			directories = config.Directories.Select( d => new MockDirectoryInfo( d.Path ) ).ToList();
		}

		public MockEnvironment( Encoding encoding )
		{
			if ( encoding == null )
				throw new ArgumentNullException( "encoding" );

			this.encoding = encoding;
		}

		public IDirectoryInfo GetDirectory( string path )
		{
			return directories.First( d => d.Path == path );
		}

		public IOperationsQueue OperationsQueue
		{
			get { return operationsQueue; }
		}

		public ITimeService TimeService
		{
			get { return timeService; }
		}
	}
}
