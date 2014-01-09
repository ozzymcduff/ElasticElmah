using System;
using System.Net;
using System.Reflection;
using System.Web;
using ElasticElmah.Appender.Presentation;
using NUnit.Framework;
using Subtext.TestLibrary;
using log4net;
using System.Linq;
using log4net.Core;
using System.Collections.Generic;
using ElasticElmah.Appender.Search;

namespace ElasticElmah.Appender.Tests
{
    public class Assertions
    {
        protected ElasticSearchRepository _appender;
        protected static void ExpectedPagingResult(LogSearchResult result)
        {
            Assert.AreEqual(1, result.Total);
            Assert.AreEqual("msg", result.Hits.First().Data.Properties["prop"]);
            Assert.That(result.Hits.Single().Data.Message, Is.EqualTo("Message"));
        }
        protected void ExpectOrderedResultASync(DateTime now)
        {
            ExpectedOrderedResult(_appender.GetPagedAsync(0, 2).Result, now);
        }
        protected void ExpectOrderedResultSync(DateTime now)
        {
            ExpectedOrderedResult(_appender.GetPaged(0, 2), now);
        }

        protected void ExpectOrderedResultASync(ElasticSearchRepository.SearchTerm search, DateTime now)
        {
            ExpectedOrderedResult(_appender.GetPagedAsync(search, 0, 2).Result, now);
        }
        protected void ExpectOrderedResultSync(ElasticSearchRepository.SearchTerm search, DateTime now)
        {
            ExpectedOrderedResult(_appender.GetPaged(search, 0, 2), now);
        }
        protected static void ExpectedOrderedResult(LogSearchResult result, DateTime now)
        {
            Assert.AreEqual(5, result.Total);
            Assert.That(result.Hits.Select(l => l.Data.TimeStamp).ToArray(),
                Is.EquivalentTo(new[]{ 
                    now.AddDays(4), now.AddDays(3)
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
    }

    public abstract class RepositoryTests:Assertions
    {
        protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected static DateTime now = DateTime.Now;

        [Test]
        public virtual void When_nothing_inserted()
        {
            _appender.Refresh();
            System.Threading.Thread.Sleep(50);
            ExpectEmptyResultSync();
            ExpectEmptyResultASync();
        }

        [Test]
        public virtual void Can_log_properties()
        {
            var id = _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                new LoggingEventData
                    {
                        Level = Level.Alert,
                        Message = "Message",
                        TimeStamp = now,
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                            d["dic"] = new Dictionary<string, object> { 
                                { "key1", "val1" }, 
                                { "key2", "val2" },
                                { "key3", new[]{"val3"}},
                                { "key4", new[]{1, 12345}},
                                { "key5", new[]{new Dictionary<string,object>{{"key6","val6"}}}}
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

            _appender.GetTimestampRangeAsync(null,
                now.AddMilliseconds(-1),
                now.AddMilliseconds(1), 0, 10).Result.Tap(ExpectedPagingResult);

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
            Assert.That(str, Is.StringContaining("key6"));
            Assert.That(str, Is.StringContaining("val6"));
            Assert.That(err.Data.Message, Is.EqualTo("Message"));

        }

        protected void Can_read_property_when_paging()
        {
            ExpectedPagingResult(_appender.GetPagedAsync(0, 10).Result);
            _appender.GetPagedAsync(0, 10).Result.Tap(ExpectedPagingResult);
        }


        [Test]
        public virtual void Can_log_web_properties()
        {
            string id;
            using (var simulator = new HttpSimulator())
            {
                var form = new WebHeaderCollection();
                form.Add("variable1", "keyvalue1");

                simulator.SimulateRequest(new Uri("http://localhost/path/to/something/filename.aspx?query=somevalue"),form);
                
                id = _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                                                    new LoggingEventData
                                                        {
                                                            Level = Level.Alert,
                                                            Message = "Message",
                                                            TimeStamp = now,
                                                        }).Tap(evt => ElasticSearchWebAppender.AddHttpContextProperties(evt,
                                                                                                   new HttpContextWrapper
                                                                                                       (HttpContext.Current))));
            }
            _appender.Refresh();

            var err = _appender.Get(id);
            var str = FormatDictionary.ToTable(err.Data.Properties);
            Assert.That(str, Is.StringContaining("filename.aspx"));
            Assert.That(str, Is.StringContaining("localhost"));
            Assert.That(str, Is.StringContaining("something"));
            Assert.That(str, Is.StringContaining("query"));
            Assert.That(str, Is.StringContaining("somevalue"));
            Assert.That(str, Is.StringContaining("variable1"));
            Assert.That(str, Is.StringContaining("keyvalue1"));
            Assert.That(err.Data.Message, Is.EqualTo("Message"));
        }

        [Test]
        public virtual void Should_get_latest_bulk()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(now.AddDays(i));
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

            ExpectOrderedResultASync(now);
            ExpectOrderedResultSync(now);
        }

        [Test]
        public virtual void Should_get_latest()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(now.AddDays(i));
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

            ExpectOrderedResultASync(now);
            ExpectOrderedResultSync(now);
            ExpectOrderedFacetResultASync();
            ExpectOrderedHistogramResultASync();
        }

        protected void ExpectOrderedFacetResultASync() 
        {
            ExpectedFacetResult(_appender.GetTimestampFacetAsync(null,now.AddDays(-1), now.AddDays(10), 0, 2).Result);
        }
        protected void ExpectOrderedHistogramResultASync() 
        {
            ExpectedHistogramResult(_appender.GetTimestampHistogramAsync(null, now.AddDays(-1), now.AddDays(10), 0, 2).Result);
        }
        protected static void ExpectedFacetResult(LogSearchFacetResult result) 
        {
            Assert.AreEqual(5, result.Count);
            //Assert.That(result.Hits.Select(l => l.Data.TimeStamp).ToArray(),
            //    Is.EquivalentTo(new[]{ 
            //        now.AddDays(4), now.AddDays(3)
            //    }));
        }
        protected static void ExpectedHistogramResult(LogSearchHistogramResult result) 
        {
            if (false)
            Assert.That(result.Histogram.Select(l => new KeyValuePair<DateTime,int>( l.Time,l.Count)).ToArray(),
                Is.EquivalentTo(new[]{ 
                    new KeyValuePair<DateTime,int>(AtTwelve(now.AddDays(0)),1),
                    new KeyValuePair<DateTime,int>(AtTwelve(now.AddDays(1)),1),
                    new KeyValuePair<DateTime,int>(AtTwelve(now.AddDays(2)),1),
                    new KeyValuePair<DateTime,int>(AtTwelve(now.AddDays(3)),1),
                    new KeyValuePair<DateTime,int>(AtTwelve(now.AddDays(4)),1),
                }));
        }
        protected static DateTime AtTwelve(DateTime t) 
        {
            return new DateTime(t.Year, t.Month, t.Day, 12, 0, 0, 0);
        }
        [Test]
        public virtual void Should_get_latest_with_property()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(now.AddDays(i));
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
            ExpectOrderedResultASync(s, now);
            ExpectOrderedResultSync(s, now);
            var s2 = new ElasticSearchRepository.SearchTerm { PropertyName = "LoggingEvent.properties.prop", Value = "msg2" };
            ExpectEmptyResultASync(s2);
            ExpectEmptyResultSync(s2);
        }

        [Test]
        public virtual void Should_get_latest_after_put_mapping()
        {
            var times = new List<DateTime>();
            for (int i = 0; i < 5; i++)
            {
                times.Add(now.AddDays(i));
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
            _appender.PutMapping();
            var s = new ElasticSearchRepository.SearchTerm { PropertyName = "LoggingEvent.properties.prop", Value = "msg" };
            ExpectOrderedResultASync(s, now);
            ExpectOrderedResultSync(s, now);
            var s2 = new ElasticSearchRepository.SearchTerm { PropertyName = "LoggingEvent.properties.prop", Value = "msg2" };
            ExpectEmptyResultASync(s2);
            ExpectEmptyResultSync(s2);
        }
    }
}
