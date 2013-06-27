using System;
using System.Reflection;
using ElasticElmah.Appender.Presentation;
using NUnit.Framework;
using log4net;
using System.Linq;
using log4net.Core;
using System.Collections.Generic;
using System.Globalization;
using ElasticElmah.Appender.Search;

namespace ElasticElmah.Appender.Tests
{

    public abstract class RepositoryTests
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
                            d["dic"] = new Dictionary<string, object> { 
                                { "key1", "val1" }, 
                                { "key2", "val2" },
                                { "key3", new[]{"val3"}},
                                { "key4", new[]{1, 12345}}
                            };
                            d["array"] = new[] { "val4" };
                            d["int"] = 1;
                            d["double"] = 1.2;
                            d["null"] = null;
                            d["bool"]=true;
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
            var str = FormatDictionary.ToTable(err.Data.Properties);
            Assert.That(str, Is.StringContaining("key1"));
            Assert.That(str, Is.StringContaining("val1"));
            Assert.That(str, Is.StringContaining("key2"));
            Assert.That(str, Is.StringContaining("val2"));
            Assert.That(str, Is.StringContaining("val3"));
            Assert.That(str, Is.StringContaining("val4"));
            Assert.That(str, Is.StringContaining("12345"));

            Assert.That(err.Data.Message, Is.EqualTo("Message"));

        }

        protected void Can_read_property_when_paging()
        {
            ExpectedPagingResult(_appender.GetPagedAsync(0, 10).Result);
            _appender.GetPagedAsync(0, 10).Result.Tap(ExpectedPagingResult);
        }

        protected static void ExpectedPagingResult(LogSearchResult result)
        {
            Assert.AreEqual(1, result.Total);
            Assert.AreEqual("msg", result.Hits.First().Data.Properties["prop"]);
            Assert.That(result.Hits.Single().Data.Message, Is.EqualTo("Message"));
        }

        [Test]
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

        protected void ExpectOrderedResultASync(ElasticSearchRepository.SearchTerm search)
        {
            ExpectedOrderedResult(_appender.GetPagedAsync(search, 0, 2).Result);
        }
        protected void ExpectOrderedResultSync(ElasticSearchRepository.SearchTerm search)
        {
            ExpectedOrderedResult(_appender.GetPaged(search, 0, 2));
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
        protected static void ExpectedEmptyResult(LogSearchResult result) 
        {
            Assert.AreEqual(0, result.Total);
        }

        protected void ExpectEmptyResultASync(ElasticSearchRepository.SearchTerm search)
        {
            ExpectedEmptyResult(_appender.GetPagedAsync(search, 0, 2).Result);
        }
        protected void ExpectEmptyResultSync(ElasticSearchRepository.SearchTerm search)
        {
            ExpectedEmptyResult(_appender.GetPaged(search, 0, 2));
        }
        protected void ExpectEmptyResultASync()
        {
            ExpectedEmptyResult(_appender.GetPagedAsync(0, 2).Result);
        }
        protected void ExpectEmptyResultSync()
        {
            ExpectedEmptyResult(_appender.GetPaged(0, 2));
        }
        [Test]
        public virtual void Should_get_latest_with_property()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(new DateTime(2001, 1, 1).AddDays(i));
            }
            var ids = new List<string>();
            foreach (var logitem in times.Select(timestamp => new LoggingEvent(GetType(), _log.Logger.Repository,
                    new LoggingEventData
                    {
                        TimeStamp = timestamp,
                        Level = Level.Error,
                        Message = "Message " + timestamp.ToString(),
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
            var s = new ElasticSearchRepository.SearchTerm { PropertyName = "LoggingEvent.properties.prop", Value = "msg" };
            ExpectOrderedResultASync(s);
            ExpectOrderedResultSync(s);
            var s2 = new ElasticSearchRepository.SearchTerm { PropertyName = "LoggingEvent.properties.prop", Value = "msg2" };
            ExpectEmptyResultASync(s2);
            ExpectEmptyResultSync(s2);
        }
    }
}
