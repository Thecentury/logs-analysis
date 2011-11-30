using System;
using System.Collections.Generic;
using System.Linq;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests.Mocks
{
	public sealed class MockEnvironment : EnvironmentBase
	{
		private readonly List<IDirectoryInfo> directories;

		public override IList<IDirectoryInfo> Directories
		{
			get { return directories; }
		}

		internal IEnumerable<MockDirectoryInfo> MockDirectories
		{
			get { return directories.OfType<MockDirectoryInfo>(); }
		}

		private readonly IOperationsQueue operationsQueue;
		private readonly ITimeService timeService;

		public MockEnvironment( LogAnalyzerConfiguration config ) : this( config, new WorkerThreadOperationsQueue( config.Logger ) ) { }

		public MockEnvironment( LogAnalyzerConfiguration config, IOperationsQueue operationsQueue )
			: base( config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );
			if ( operationsQueue == null )
				throw new ArgumentNullException( "operationsQueue" );

			this.operationsQueue = operationsQueue;
			this.timeService = new MockTimeService();

			directories = new List<IDirectoryInfo>( config.Directories.Select( d => new MockDirectoryInfo( d.Path ) ) );
		}

		public override IDirectoryInfo GetDirectory( string path )
		{
			return directories.First( d => d.Path == path );
		}

		public override IOperationsQueue OperationsQueue
		{
			get { return operationsQueue; }
		}

		public override ITimeService TimeService
		{
			get { return timeService; }
		}
	}
}
