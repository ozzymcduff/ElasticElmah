using ElasticElmah.Appender.Storage;
using log4net.Core;
using NUnit.Framework;

namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class JsonSerializerTests
    {
        private string expected = @"{
    ""loggerName"":null,
    ""level"":""ALERT"",
    ""message"":""Message"",
    ""threadName"":"""",
    ""timeStamp"":""0001-01-01T00:00:00.0000000+01:00"",
    ""locationInfo"":{
        ""className"":""?"",
        ""fileName"":""?"",
        ""lineNumber"":""?"",
        ""methodName"":""?""
    },
    ""userName"":"""",
    ""properties"":{""prop"":""msg""},
    ""exceptionString"":"""",
    ""domain"":"""",
    ""identity"":""""
}";
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
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    })));
            Assert.That(serialized, Json.IsEqualTo(expected));
        }
        [Test]
        public void WillSerializeOk2()
        {
            var serialized = new DefaultJsonSerializer().Serialize(Map.To(new LoggingEvent(new LoggingEventData
            {
                Level = Level.Alert,
                Message = "Message",
                UserName = "",
                ThreadName = "",
                Domain = "",
                Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                {
                    d["prop"] = "msg";
                })
            })));
            Assert.That(serialized.Replace(" ", "").Replace("\r", "").Replace("\n", ""), Is.EqualTo(expected.Replace(" ", "").Replace("\n", "").Replace("\n", "")));
        }
    }
}
