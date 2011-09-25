using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;

namespace ModuleLogsProvider.Logging
{
	public sealed class MostEnvironment : IEnvironment
	{
		private readonly ITimeService timeService = new ConstIntervalTimeService( TimeSpan.FromDays( 1000 ) );
		private readonly WorkerThreadOperationsQueue operationsQueue = null;

		public MostEnvironment( LogAnalyzerConfiguration config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			this.operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
		}

		#region IEnvironment Members

		public IDirectoryInfo GetDirectory( string path )
		{
			throw new NotImplementedException();
		}

		public IOperationsQueue OperationsQueue
		{
			get { return operationsQueue; }
		}

		public ITimeService TimeService
		{
			get { return timeService; }
		}

		#endregion
	}
}
