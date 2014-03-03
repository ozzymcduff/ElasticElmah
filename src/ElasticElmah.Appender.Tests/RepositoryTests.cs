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
#if ASYNC
            ExpectEmptyResultASync();
#endif
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

            _appender.GetTimestampRange(null,
                now.AddMilliseconds(-1),
                now.AddMilliseconds(1), 0, 10).Tap(ExpectedPagingResult);

        }

        protected void Can_read_property_when_get(string id)
        {
            var err = _appender.Get(id);
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
            ExpectedPagingResult(_appender.GetPaged(0, 10));
#if ASYNC
			_appender.GetPagedAsync(0, 10).Result.Tap(ExpectedPagingResult);
#endif
        }


        [Test
#if !HTTPSIMULATOR
        ,Ignore("HttpSimulator does not work everywhere")
#endif
        ]
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
#if ASYNC
            ExpectOrderedResultASync(now);
#endif
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
#if ASYNC
            ExpectOrderedResultASync(now);
#endif
			ExpectOrderedResultSync(now);

			ExpectOrderedFacetResult();
			ExpectOrderedHistogramResult();

#if ASYNC
            ExpectOrderedFacetResultASync();
            ExpectOrderedHistogramResultASync();
#endif
		}
#if ASYNC
        protected void ExpectOrderedFacetResultASync() 
        {
            ExpectedFacetResult(_appender.GetTimestampFacetAsync(null,now.AddDays(-1), now.AddDays(10), 0, 2).Result);
        }
        protected void ExpectOrderedHistogramResultASync() 
        {
            ExpectedHistogramResult(_appender.GetTimestampHistogramAsync(null, now.AddDays(-1), now.AddDays(10), 0, 2).Result);
        }
#endif
		protected void ExpectOrderedFacetResult() 
		{
			ExpectedFacetResult(_appender.GetTimestampFacet(null,now.AddDays(-1), now.AddDays(10), 0, 2));
		}
		protected void ExpectOrderedHistogramResult() 
		{
			ExpectedHistogramResult(_appender.GetTimestampHistogram(null, now.AddDays(-1), now.AddDays(10), 0, 2));
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
#if ASYNC
			ExpectOrderedResultASync(s, now);
#endif
			ExpectOrderedResultSync(s, now);
            var s2 = new ElasticSearchRepository.SearchTerm { PropertyName = "LoggingEvent.properties.prop", Value = "msg2" };
#if ASYNC
			ExpectEmptyResultASync(s2);
#endif
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
#if ASYNC
			ExpectOrderedResultASync(s, now);
#endif
			ExpectOrderedResultSync(s, now);
            var s2 = new ElasticSearchRepository.SearchTerm { PropertyName = "LoggingEvent.properties.prop", Value = "msg2" };
#if ASYNC
			ExpectEmptyResultASync(s2);
#endif
            ExpectEmptyResultSync(s2);
        }
    }
}
