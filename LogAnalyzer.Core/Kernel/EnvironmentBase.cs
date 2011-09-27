using System;
using System.Reactive.Concurrency;
using LogAnalyzer.Config;

namespace LogAnalyzer.Kernel
{
	public abstract class EnvironmentBase : IEnvironment
	{
		private readonly LogAnalyzerConfiguration config;

		protected EnvironmentBase( LogAnalyzerConfiguration config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			this.config = config;
		}

		public abstract IDirectoryInfo GetDirectory( string path );

		public abstract IOperationsQueue OperationsQueue { get; }

		public abstract ITimeService TimeService { get; }

		public virtual IScheduler Scheduler
		{
			get { return config.GetScheduler(); }
		}
	}
}