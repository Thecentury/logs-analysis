using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using Moq;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Gui
{
	[TestFixture]
	public sealed class ApplicationViewModelTests
	{
		[Test]
		public void ShouldBeAbleToStartWithNoDirectories()
		{
			LogAnalyzerConfiguration config = new LogAnalyzerConfiguration();
			AssertHaveCreated( config );
		}

		[Test]
		public void ShouldBeAbleToStartWithNoEnabledDirectories()
		{
			LogAnalyzerConfiguration config = LogAnalyzerConfiguration.CreateNew()
				.AddLogDirectory( new LogDirectoryConfigurationInfo { Enabled = false } );

			AssertHaveCreated( config );
		}

		private static void AssertHaveCreated( LogAnalyzerConfiguration config )
		{
			Mock<IEnvironment> mockEnvironment = new Mock<IEnvironment>();

			ApplicationViewModel application = new ApplicationViewModel( config, mockEnvironment.Object );

			Assert.NotNull( application );
		}
	}
}
