using System.Globalization;
using ElasticElmah.Appender.Storage;
using log4net.Core;
using NUnit.Framework;
using System;
using LoggingEvent = log4net.Core.LoggingEvent;

#if NET20
namespace ElasticElmah.Appender.net20.Tests
#else
namespace ElasticElmah.Appender.Tests
#endif
{
    [TestFixture]
    public class JsonSerializerTests
    {
        private object expected = new
        {
            loggerName = (string)null,
            level = "ALERT",
            message = "Message",
            threadName = "",
            timeStamp = Map.To(new DateTime(0xdeadbeef, DateTimeKind.Utc)),
            locationInfo = new
            {
                className = "?",
                fileName = "?",
                lineNumber = "?",
                methodName = "?"
            },
            userName = "",
            properties = new { prop = "msg" },
            exceptionString = "",
            domain = "",
            identity = ""
        };

        [Test]
        public void Can_serialize_time()
        {
            var dateTime = DateTime("0001-01-01T00:06:13.5928559+00:00");
            Assert.That(Map.To(dateTime), Is.StringStarting("0001-01-01T00:06:13.5928559+"));
        }

        private static DateTime DateTime(string t00)
        {
            DateTimeOffset localDate;
            if (!DateTimeOffset.TryParseExact(t00, "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out localDate))
                throw new Exception("!");
            return localDate.UtcDateTime;
        }

        [Test]
        public void WillSerializeOk()
        {
            var serialized = new DefaultJsonSerializer().Serialize(Map.To(new LoggingEvent(new LoggingEventData
                    {
                        Level = Level.Alert,
                        Message = "Message",
                        UserName = "",
                        ThreadName = "",
                        Domain = "",
                        TimeStamp = new DateTime(0xdeadbeef, DateTimeKind.Utc),
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    })));
            Assert.That(serialized, Json.IsEqualTo(expected));
        }
    }
}
