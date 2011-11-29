using LogAnalyzer.GUI.ViewModels;
using NUnit.Framework;
using LogAnalyzer.Filters;

namespace LogAnalyzer.Tests.Gui
{
	[TestFixture]
	public class ApplicationViewModelCommandsTests
	{
		/// <summary>
		/// Должен пропускать все записи лога с отличающимся файлом.
		/// </summary>
		[Test]
		public void TestCreateExcludeFileFilter()
		{
			ApplicationViewModel applicationViewModel = new ApplicationViewModel();

			LogFile file1 = new LogFile();
			LogEntry entry1 = new LogEntry( file1 );
			var builder = applicationViewModel.CreateExcludeFileFilter( entry1 );
			var compiled = builder.BuildLogEntriesFilter();

			Assert.IsFalse( compiled.Include( entry1 ) );
			Assert.IsFalse( compiled.Include( new LogEntry( file1 ) ) );

			Assert.IsTrue( compiled.Include( new LogEntry( new LogFile() ) ) );
		}
	}
}
