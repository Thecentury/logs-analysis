using System;
using System.Collections.Generic;
using System.Linq;
using LogAnalyzer.Kernel.Parsers;
using LogAnalyzer.Tests.Mocks;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
    [TestFixture]
    public class ManualLogLineParserTests
    {
        [TestCase("[I] [ 69] 24.05.2011 00:00:12	Checked", "I", 69, "24.05.2011 0:00:12", "Checked")]
        [TestCase("[I] [ 69] 24.05.2011 0:00:12	Checked", "I", 69, "24.05.2011 0:00:12", "Checked")]
        public void ShouldParseLine(string line, string expectedType, int expectedThreadId, string expectedDate, string expectedText)
        {
            ManualLogLineParser parser = new ManualLogLineParser();

            string actualType = null;
            int actualThreadId = 0;
            DateTime actualDateTime = default(DateTime);
            string actualText = null;

            bool parsed = parser.TryExtractLogEntryData(line, ref actualType, ref actualThreadId, ref actualDateTime,
                                                        ref actualText);

            Assert.IsTrue(parsed);

            Assert.AreEqual(expectedType, actualType);
            Assert.AreEqual(expectedThreadId, actualThreadId);
            Assert.AreEqual(DateTime.Parse(expectedDate), actualDateTime);
            Assert.AreEqual(expectedText, actualText);
        }
    }
}
