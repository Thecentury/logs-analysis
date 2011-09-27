using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using NUnit.Framework;

namespace ModuleLogsProvider.Tests
{
	//[TestFixture]
	public class DependencyInjectionTest
	{
		//[Test]
		public void TestDependencyInjection()
		{
			const int expected = 1;

			LogAnalyzerConfiguration config = new LogAnalyzerConfiguration();
			config.Register<int>( () => expected );

			var actual = config.Resolve<int>();
			Assert.That( actual, Is.EqualTo( expected ) );
		}
	}
}
