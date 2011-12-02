using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Tests.Mocks;
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
			LogAnalyzerConfiguration config = new LogAnalyzerConfiguration()
				.RegisterInstance<IFileSystem>( new RealFileSystem() );

			AssertHaveCreated( config );
		}

		[Test]
		public void ShouldBeAbleToStartWithNoEnabledDirectories()
		{
			LogAnalyzerConfiguration config = LogAnalyzerConfiguration
				.CreateNew()
				.RegisterInstance<IFileSystem>( new RealFileSystem() )
				.AddLogDirectory( new LogDirectoryConfigurationInfo { Enabled = false, DisplayName = "Name", Path = "Path" } );

			AssertHaveCreated( config );
		}

		[Test]
		public void StartsWithZeroDirectories()
		{
			LogAnalyzerConfiguration config = new LogAnalyzerConfiguration()
				.RegisterInstance<IFileSystem>( new RealFileSystem() );

			MockEnvironment env = new MockEnvironment( config );

			ApplicationViewModel vm = new ApplicationViewModel( config, env );
			vm.Core.Start();
		}

		private static void AssertHaveCreated( LogAnalyzerConfiguration config )
		{
			MockEnvironment environment = new MockEnvironment( config );

			ApplicationViewModel application = new ApplicationViewModel( config, environment );

			Assert.NotNull( application );
		}
	}
}
