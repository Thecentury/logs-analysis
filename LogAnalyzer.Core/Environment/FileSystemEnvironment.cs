using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LogAnalyzer
{
	public sealed class FileSystemEnvironment : IEnvironment
	{
		private readonly WorkerThreadOperationsQueue operationsQueue = null;
		private readonly List<FileSystemDirectoryInfo> directories = null;
		private readonly ITimeService timeService = null;

		public FileSystemEnvironment( LogAnalyzerConfiguration config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			this.operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
			this.directories = config.EnabledDirectories.Select( d => new FileSystemDirectoryInfo( d ) ).ToList();
			this.timeService = new ConstIntervalTimeService();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Путь к папке.</param>
		/// <returns></returns>
		public IDirectoryInfo GetDirectory( string path )
		{
			FileSystemDirectoryInfo directoryInfo = directories.First( d => d.Path == path );
			return directoryInfo;
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
