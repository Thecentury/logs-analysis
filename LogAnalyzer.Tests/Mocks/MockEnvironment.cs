using System;
using System.Collections.Generic;
using System.Linq;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests.Mocks
{
	public sealed class MockEnvironment : EnvironmentBase
	{
		private readonly List<IDirectoryInfo> _directories;

		public override IList<IDirectoryInfo> Directories
		{
			get { return _directories; }
		}

		internal IEnumerable<MockDirectoryInfo> MockDirectories
		{
			get { return _directories.OfType<MockDirectoryInfo>(); }
		}

		private readonly IOperationsQueue _operationsQueue;
		private readonly ITimeService _timeService;

		public MockEnvironment( LogAnalyzerConfiguration config ) : this( config, new WorkerThreadOperationsQueue( config.Logger ) ) { }

		public MockEnvironment( LogAnalyzerConfiguration config, IOperationsQueue operationsQueue )
			: base( config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );
			if ( operationsQueue == null )
				throw new ArgumentNullException( "operationsQueue" );

			this._operationsQueue = operationsQueue;
			this._timeService = new MockTimeService();

			_directories = new List<IDirectoryInfo>( config.Directories.Select( d => new MockDirectoryInfo( d.Path ) ) );
		}

		public override IDirectoryInfo GetDirectory( string path )
		{
			return _directories.First( d => d.Path == path );
		}

		public override IOperationsQueue OperationsQueue
		{
			get { return _operationsQueue; }
		}

		public override ITimeService TimeService
		{
			get { return _timeService; }
		}
	}
}
