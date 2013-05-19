using System;
using System.Reflection;
using NUnit.Framework;
using log4net;
using System.Linq;
using log4net.Core;
using System.Collections.Generic;
using System.Globalization;
using ElasticElmah.Appender.Search;
using ElasticElmah.Appender.Web;
namespace ElasticElmah.Appender.Tests
{

    public abstract class AppenderTests
    {
        protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected ElasticSearchRepository _appender;

        [Test]
        public virtual void Can_log_properties()
        {
            var id = _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                new LoggingEventData
                    {
                        Level = Level.Alert,
                        Message = "Message",
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    }));
            _appender.Refresh();

            Can_read_property_when_paging();

            Can_read_property_when_get(id);
        }

        protected void Can_read_property_when_get(string id)
        {
            var errt = _appender.GetAsync(id);
            var err = errt.Result;
            Assert.AreEqual("msg", err.Data.Properties["prop"]);
            Assert.That(err.Data.Message, Is.EqualTo("Message"));

        }

        protected void Can_read_property_when_paging()
        {
            ExpectedPagingResult(_appender.GetPagedAsync(0, 10).Result);
            _appender.GetPagedAsync(0, 10).Result.Tap(errors => {
                ExpectedPagingResult(errors);
            });
        }

        private void ExpectedPagingResult(Tuple<Func<IAsyncResult>, Func<IAsyncResult, LogSearchResult>> tuple)
        {
            ExpectedPagingResult(tuple.WaitOne());
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
                    })).ToArray(), true);

            _appender.Refresh();

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
                ids.Add(_appender.Add(logitem));
            }
            _appender.Refresh();
            //_appender.Flush();

            ExpectOrderedResultASync();
            ExpectOrderedResultSync();
        }

        protected void ExpectOrderedResultASync()
        {
            ExpectedOrderedResult(_appender.GetPagedAsync(0, 2).Result);
        }
        protected void ExpectOrderedResultSync()
        {
            ExpectedOrderedResult(_appender.GetPaged(0, 2));
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
