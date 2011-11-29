using System;

namespace LogAnalyzer.Tests.Mocks
{
	/// <summary>
	/// Обертка над лог-файлом, которая позволяет указывать время i-й записи лога.
	/// </summary>
	public sealed class DeterminedTimeLogHelper
	{
		private readonly MockFileInfo file;
		private readonly DateTime start = DateTime.MinValue;

		public DeterminedTimeLogHelper( MockFileInfo file )
		{
			this.file = file;
		}

		public void WriteInfo( string message, int secondsPassed )
		{
			DateTime time = start.AddSeconds( secondsPassed );
			file.WriteInfo( message + Environment.NewLine, time );
		}
	}
}
