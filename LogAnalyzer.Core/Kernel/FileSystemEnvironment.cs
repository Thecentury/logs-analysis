using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using LogAnalyzer.Config;

namespace LogAnalyzer.Kernel
{
	public sealed class FileSystemEnvironment : EnvironmentBase
	{
		private readonly WorkerThreadOperationsQueue operationsQueue;
		private readonly List<FileSystemDirectoryInfo> directories;
		private readonly ITimeService timeService;

		public FileSystemEnvironment( LogAnalyzerConfiguration config )
			: base( config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			this.operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
			this.directories = config.EnabledDirectories.Select( FileSystemDirectoryInfo.Create ).ToList();
			this.timeService = new ConstIntervalTimeService();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Путь к папке.</param>
		/// <returns></returns>
		public override IDirectoryInfo GetDirectory( string path )
		{
			FileSystemDirectoryInfo directoryInfo = directories.First( d => d.Path == path );
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
	}
}
