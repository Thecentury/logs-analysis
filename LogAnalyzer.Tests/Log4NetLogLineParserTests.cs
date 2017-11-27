using System;
using LogAnalyzer.Kernel.Parsers;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
    [TestFixture]
    public class Log4NetLogLineParserTests
    {
        [TestCase("2017-11-23 18:49:17,399 [2614] FATAL Social.Crawler.Akka.Ok.Users.Spider.OkProfilesZipAchiveDataStorage [(null)] - Failed to write temp data")]
        public void ShouldParse(string line)
        {
            var parser = new Log4NetLogLineParser();

            string actualType = null;
            int actualThreadId = 0;
            DateTime actualDateTime = default(DateTime);
            string actualText = null;
            bool parsed = parser.TryExtractLogEntryData(line, ref actualType, ref actualThreadId, ref actualDateTime,
                ref actualText);

            Assert.That(parsed, "parsed");
        }
    }
}