using System;
using System.Reflection;
using NUnit.Framework;
using log4net;
using System.Linq;
using log4net.Core;
using System.Collections.Generic;
using System.Globalization;

namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class AppenderTests
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Guid _index;
        private ElasticSearchRepository _appender;
        [Test, Ignore]
        public void Log()
        {
            _log.Debug("test");
        }
        [SetUp]
        public void Init()
        {
            _index = Guid.NewGuid();
            _appender = new ElasticSearchRepository("Server=localhost;Index=" + _index + ";Port=9200");
            _appender.CreateIndex();
        }

        [TearDown]
        public void Cleanup()
        {
            _appender.DeleteIndex();
        }

        [Test]
        public void Can_log()
        {
            _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                new LoggingEventData
                    {
                        Level = Level.Alert,
                        Message = "Message"
                    }));
            _appender.Flush();
            var result = _appender.All();
            Assert.AreEqual(1, result.Count());
            Assert.That(result.Single().Message, Is.EqualTo("Message"));
        }

        [Test]
        public void Can_log_properties()
        {
            _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                new LoggingEventData
                    {
                        Level = Level.Alert,
                        Message = "Message",
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    }));
            _appender.Flush();
            var result = _appender.All();
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("msg", result.First().Properties["prop"]);
        }

        [Test]
        public void Should_get_latest()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(new DateTime(2001, 1, 1).AddDays(i));
            }
            foreach (var timestamp in times)
            {
                _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                    new LoggingEventData
                    {
                        TimeStamp = timestamp,
                        Level = Level.Alert,
                        Message = "Message",
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    }));
            }
            _appender.Flush();
            var result = _appender.GetPaged(0,2);
            Assert.AreEqual(5, result.Item2);
            Assert.That(result.Item1.Select(l=>l.Item2.TimeStamp).ToArray(),
                Is.EquivalentTo(new []{ 
                    new DateTime(2001,1,5),
                    new DateTime(2001,1,4)
                }));
        }
    }
}
