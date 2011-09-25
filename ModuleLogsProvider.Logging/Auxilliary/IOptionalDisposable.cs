using System;

namespace ModuleLogsProvider.Logging
{
	public interface IOptionalDisposable<out T> : IDisposable
	{
		T Inner { get; }
	}
}