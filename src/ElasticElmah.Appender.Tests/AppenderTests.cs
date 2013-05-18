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

    public abstract class AppenderTests
    {
        protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected ElasticSearchRepository _appender;

        [Test]
        public void Can_log_properties()
        {
            string id = null;
            _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                new LoggingEventData
                    {
                        Level = Level.Alert,
                        Message = "Message",
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    }),resp=>{
                        id = resp._id;
                    });
            _appender.Flush();

            Can_read_property_when_paging();

            Can_read_property_when_get(id);
        }

        private void Can_read_property_when_get(string id)
        {
            _appender.Get(id, err => {
                Assert.AreEqual("msg", err.Data.Properties["prop"]);
                Assert.That(err.Data.Message, Is.EqualTo("Message"));
            }).AsyncWaitHandle.WaitOne();
        }

        private void Can_read_property_when_paging()
        {
            ExpectedPagingResult(_appender.GetPaged(0, 10)());
            _appender.GetPaged(0, 10, (errors) => {
                ExpectedPagingResult(errors);
            }).AsyncWaitHandle.WaitOne();
        }

        private static void ExpectedPagingResult(ElasticSearchRepository.Errors result)
        {
            Assert.AreEqual(1, result.Total);
            Assert.AreEqual("msg", result.Documents.First().Data.Properties["prop"]);
            Assert.That(result.Documents.Single().Data.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void Should_get_latest()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(new DateTime(2001, 1, 1).AddDays(i));
            }
            var list = new List<IAsyncResult>();
            foreach (var timestamp in times)
            {
               list.Add(_appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                    new LoggingEventData
                    {
                        TimeStamp = timestamp,
                        Level = Level.Alert,
                        Message = "Message",
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    }), (resp) => { }));
            }
            foreach (var item in list)
            {
                item.AsyncWaitHandle.WaitOne();
            }
            _appender.Flush();

            ExpectOrderedResultASync();
            ExpectOrderedResultSync();
        }

        private void ExpectOrderedResultASync()
        {
            _appender.GetPaged(0, 2, errors => {
                ExpectedOrderedResult(errors);
            }).AsyncWaitHandle.WaitOne();
        }
        private void ExpectOrderedResultSync()
        {
            ExpectedOrderedResult(_appender.GetPaged(0, 2)());
        }

        private static void ExpectedOrderedResult(ElasticSearchRepository.Errors result)
        {
            Assert.AreEqual(5, result.Total);
            Assert.That(result.Documents.Select(l => l.Data.TimeStamp).ToArray(),
                Is.EquivalentTo(new[]{ 
                    new DateTime(2001,1,5),
                    new DateTime(2001,1,4)
                }));
        }
    }
}
