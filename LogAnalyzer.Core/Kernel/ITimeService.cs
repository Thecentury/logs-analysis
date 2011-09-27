using System;

namespace LogAnalyzer.Kernel
{
	/// <summary>
	/// Проверяет, насколько "стара" одна дата относительно другой.
	/// </summary>
	public interface ITimeService
	{
		bool IsRelativelyOld( DateTime current, DateTime max );
	}
}
