using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	public sealed class FileSystemEnvironment : EnvironmentBase
	{
		private readonly WorkerThreadOperationsQueue operationsQueue;
		private readonly List<IDirectoryInfo> directories;
		private readonly ITimeService timeService;

		public FileSystemEnvironment( LogAnalyzerConfiguration config )
			: base( config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			IDirectoryFactory directoryFactory = config.ResolveNotNull<IDirectoryFactory>();

			this.operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
			this.directories = config.EnabledDirectories.Select( directoryFactory.CreateDirectory ).ToList();
			this.timeService = config.ResolveNotNull<ITimeService>();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Путь к папке.</param>
		/// <returns></returns>
		public override IDirectoryInfo GetDirectory( string path )
		{
			var directoryInfo = directories.First( d => d.Path == path );
			return directoryInfo;
		}

		public override IOperationsQueue OperationsQueue
		{
			get { return operationsQueue; }
		}

		public override ITimeService TimeService
		{
			get { return timeService; }
		}

		public override IList<IDirectoryInfo> Directories
		{
			get { return directories; }
		}
	}
}
