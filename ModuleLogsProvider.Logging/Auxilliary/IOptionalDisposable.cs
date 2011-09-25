using System;

namespace ModuleLogsProvider.Logging.Auxilliary
{
	public interface IOptionalDisposable<out T> : IDisposable
	{
		T Inner { get; }
	}
}