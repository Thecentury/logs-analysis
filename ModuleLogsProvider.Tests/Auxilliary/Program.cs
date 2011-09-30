using System;
using System.Reflection;
using LogAnalyzer.GUI.ViewModels;

namespace ModuleLogsProvider.Tests.Auxilliary
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args )
		{
			ViewModelTests test = new ViewModelTests();
			test.Init();
			test.TestLogEntryAddedAfterCoreStart();

			//test.TestSerevalMessagesFromMost();
			//test.TestSeveralMessagesFromTwoLoggers();
			//test.TestSingleMessageFromMost();

			Console.WriteLine( "Incomplete." );

			//NUnitTestRunner.RunTests<Program>();
		}
	}
}
