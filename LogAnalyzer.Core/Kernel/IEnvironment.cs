using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using LogAnalyzer.Operations;

namespace LogAnalyzer.Kernel
{
	public interface IEnvironment
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Путь к папке.</param>
		/// <returns></returns>
		IDirectoryInfo GetDirectory( string path );

		IOperationsQueue OperationsQueue { get; }

		ITimeService TimeService { get; }

		OperationScheduler Scheduler { get; }

		IList<IDirectoryInfo> Directories { get; }
	}
}
