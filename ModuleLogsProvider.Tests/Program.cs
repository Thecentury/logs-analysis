using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Tests
{
	class Program
	{
		[STAThread]
		public static void Main( string[] args )
		{
			MostEnvironmentTest test = new MostEnvironmentTest();
			test.TestCreation();

			//NUnitTestRunner.RunTests<Program>();
		}
	}
}
