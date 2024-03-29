using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.Operations;

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

		public OperationScheduler Scheduler
		{
			get { return config.Container.ResolveNotNull<OperationScheduler>(); }
		}

		public abstract IList<IDirectoryInfo> Directories { get; }
	}
}