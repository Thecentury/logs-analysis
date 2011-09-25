using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
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
	}
}
