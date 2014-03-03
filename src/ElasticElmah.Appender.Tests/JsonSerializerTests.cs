using ElasticElmah.Appender.Storage;
using log4net.Core;
using NUnit.Framework;
using System;

namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class JsonSerializerTests
    {
        private object expected = new {
    loggerName=(string)null,
    level="ALERT",
    message="Message",
    threadName="",
    timeStamp="0001-01-01T00:06:13.5928559+00:00",
    locationInfo=new{
        className="?",
        fileName="?",
        lineNumber="?",
        methodName="?"
    },
    userName="",
    properties=new{prop="msg"},
    exceptionString="",
    domain="",
    identity=""
};
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
                        TimeStamp = new DateTime(0xdeadbeef),
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    })));
            Assert.That(serialized, Json.IsEqualTo(expected));
        }
    }
}
