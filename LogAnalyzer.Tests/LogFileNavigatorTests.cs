using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Parsers;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
    [TestFixture]
    public class LogFileNavigatorTests
    {
        private LogFileReaderArguments _arguments;

        [OneTimeSetUp]
        public void Setup()
        {
            _arguments = new LogFileReaderArguments
            {
                Encoding = Encoding.UTF8,
                LineParser = new ManualLogLineParser()
            };
        }

        [Ignore("Local")]
        [Test]
        public void ShouldReadFirst10Entries()
        {
            LogFileNavigator navigator = new LogFileNavigator(new FileSystemFileInfo(@"C:\Logs\1.log"), _arguments);
            var entries = navigator.ToForwardEnumerable().Take(10).ToList();
        }

        [Test]
        public void ShouldReadSimpleSingleLinesLog()
        {
            LogFileNavigator navigator = new LogFileNavigator(new FileSystemFileInfo(@"..\..\Resources\Log1.txt"), _arguments);
            var entries = navigator.ToForwardEnumerable().ToList();

            Assert.That(entries.Count, Is.EqualTo(3));
        }

        [Test]
        public void ShouldReadLogWithOneDoubleLinedEntry()
        {
            LogFileNavigator navigator = new LogFileNavigator(new FileSystemFileInfo(@"..\..\Resources\Log2.txt"), _arguments);
            var entries = navigator.ToForwardEnumerable().ToList();

            Assert.That(entries.Count, Is.EqualTo(4));
            Assert.That(entries[2].TextLines.Count, Is.EqualTo(2));
        }

        [Test]
        public void ShouldReadLogWithDoubleLinedEntryAtTheEnd()
        {
            LogFileNavigator navigator = new LogFileNavigator(new FileSystemFileInfo(@"..\..\Resources\Log3.txt"), _arguments);
            var entries = navigator.ToForwardEnumerable().ToList();

            Assert.That(entries.Count, Is.EqualTo(4));
            Assert.That(entries[2].TextLines.Count, Is.EqualTo(2));
            Assert.That(entries[3].TextLines.Count, Is.EqualTo(2));
        }

#if !NCRUNCH
        [Test]
        [Ignore("Local")]
#endif
        public void LongLogFileReadingBenchmark()
        {
            LogFileNavigator navigator = new LogFileNavigator(new FileSystemFileInfo(@"C:\Logs\Security2.!log!"), _arguments);

            Stopwatch timer = Stopwatch.StartNew();
            int count = navigator.ToForwardEnumerable().Count();

            timer.Stop();
            Console.WriteLine("Count = {0}", count);
            Console.WriteLine("Elapsed {0} ms", timer.ElapsedMilliseconds);
        }
    }
}
