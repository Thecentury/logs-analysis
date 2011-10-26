using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;
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

			DependencyInjectionContainer config = DependencyInjectionContainer.Instance;
			config.Register<int>( () => expected );

			var actual = config.Resolve<int>();
			Assert.That( actual, Is.EqualTo( expected ) );
		}
	}
}
