using System;
using System.Reflection;
using NUnit.Framework;
using log4net;
using System.Linq;
using log4net.Core;
using System.Collections.Generic;
using System.Globalization;
using ElasticElmah.Appender.Search;

namespace ElasticElmah.Appender.Tests
{

    public abstract class AppenderTests
    {
        protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected ElasticSearchRepository _appender;

        [Test]
        public virtual void Can_log_properties()
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
                    }),_id=>{
                        id = _id;
                    });
            _appender.Refresh();

            Can_read_property_when_paging();

            Can_read_property_when_get(id);
        }

        protected void Can_read_property_when_get(string id)
        {
            _appender.Get(id, err => {
                Assert.AreEqual("msg", err.Data.Properties["prop"]);
                Assert.That(err.Data.Message, Is.EqualTo("Message"));
            }).AsyncWaitHandle.WaitOne();
        }

        protected void Can_read_property_when_paging()
        {
            ExpectedPagingResult(_appender.GetPaged(0, 10)());
            _appender.GetPaged(0, 10, (errors) => {
                ExpectedPagingResult(errors);
            }).AsyncWaitHandle.WaitOne();
        }

        protected static void ExpectedPagingResult(LogSearchResult result)
        {
            Assert.AreEqual(1, result.Total);
            Assert.AreEqual("msg", result.Hits.First().Data.Properties["prop"]);
            Assert.That(result.Hits.Single().Data.Message, Is.EqualTo("Message"));
        }

        [Test,Ignore]
        public virtual void Should_get_latest_bulk()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(new DateTime(2001, 1, 1).AddDays(i));
            }

            _appender.AddBulk(times.Select(timestamp => new LoggingEvent(GetType(), _log.Logger.Repository,
                    new LoggingEventData
                    {
                        TimeStamp = timestamp,
                        Level = Level.Error,
                        Message = "Message " + timestamp.ToString(),
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    })).ToArray(), () => { }, true);

            _appender.Refresh().AsyncWaitHandle.WaitOne();

            ExpectOrderedResultASync();
            ExpectOrderedResultSync();
        }

        [Test]
        public virtual void Should_get_latest()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(new DateTime(2001, 1, 1).AddDays(i));
            }
            var ids= new List<string>();
            foreach (var logitem in times.Select(timestamp => new LoggingEvent(GetType(), _log.Logger.Repository,
                    new LoggingEventData
                    {
                        TimeStamp = timestamp,
                        Level = Level.Error,
                        Message = "Message "+timestamp.ToString(),
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    })))
            {
                _appender.Add(logitem,id=>{ ids.Add(id); }).AsyncWaitHandle.WaitOne();
            }
            _appender.Refresh().AsyncWaitHandle.WaitOne();
            //_appender.Flush();

            ExpectOrderedResultASync();
            ExpectOrderedResultSync();
        }

        protected void ExpectOrderedResultASync()
        {
            _appender.GetPaged(0, 2, errors => {
                ExpectedOrderedResult(errors);
            }).AsyncWaitHandle.WaitOne();
        }
        protected void ExpectOrderedResultSync()
        {
            ExpectedOrderedResult(_appender.GetPaged(0, 2)());
        }

        protected static void ExpectedOrderedResult(LogSearchResult result)
        {
            Assert.AreEqual(5, result.Total);
            Assert.That(result.Hits.Select(l => l.Data.TimeStamp).ToArray(),
                Is.EquivalentTo(new[]{ 
                    new DateTime(2001,1,5),
                    new DateTime(2001,1,4)
                }));
        }
    }
}
