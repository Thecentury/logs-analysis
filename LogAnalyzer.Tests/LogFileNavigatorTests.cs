using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;
using LogAnalyzer.Kernel;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class LogFileNavigatorTests
	{
		private LogFileReaderArguments _arguments;

		[TestFixtureSetUp]
		public void Setup()
		{
			_arguments = new LogFileReaderArguments
			{
				Encoding = Encoding.UTF8,
				LineParser = new ManualLogLineParser()
			};
		}

		[Test]
		public void ShouldReadFirst10Entries()
		{
			LogFileNavigator navigator = new LogFileNavigator( new FileSystemFileInfo( @"C:\Logs\1.log" ), _arguments );
			var entries = navigator.ToForwardEnumerable().Take( 10 ).ToList();
		}

		[Test]
		public void ShouldReadLog1()
		{
			LogFileNavigator navigator = new LogFileNavigator( new FileSystemFileInfo( @"..\..\Resources\Log1.txt" ), _arguments );
			var entries = navigator.ToForwardEnumerable().ToList();

			Assert.That( entries.Count, Is.EqualTo( 3 ) );
		}

		[Test]
		public void ShouldReadLog2()
		{
			LogFileNavigator navigator = new LogFileNavigator( new FileSystemFileInfo( @"..\..\Resources\Log2.txt" ), _arguments );
			var entries = navigator.ToForwardEnumerable().ToList();

			Assert.That( entries.Count, Is.EqualTo( 4 ) );
			Assert.That( entries[2].TextLines.Count, Is.EqualTo( 2 ) );
		}
	}
}
