using ElasticElmah.Appender.Storage;
using log4net.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class JsonSerializerTests
    {
        [Test]
        public void WillSerializeOk() 
        {
            var serialized = new DefaultJsonSerializer().Serialize( Map.To(new LoggingEvent(new LoggingEventData
                    {
                        Level = Level.Alert,
                        Message = "Message",
                        UserName = "",
                        ThreadName="",
                        Domain="",
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    })));
            Assert.That(serialized, Json.IsEqualTo(
                @"{
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
}"));
        }
    }
}
