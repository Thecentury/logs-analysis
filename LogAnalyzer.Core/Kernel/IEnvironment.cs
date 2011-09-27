using System.Reactive.Concurrency;

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

		IScheduler Scheduler { get; }
	}
}
