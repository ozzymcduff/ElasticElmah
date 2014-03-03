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
        protected void ExpectMissingIndexResultSync()
        {
            Assert.Throws<IndexMissingException>(() => _appender.GetPaged(0, 2));
        }
#if ASYNC
        protected void ExpectMissingIndexResultASync()
        {
            try
            {
                var res = _appender.GetPagedAsync(0, 2).Result;
            }
            catch (AggregateException exception)
            {
                Assert.That(exception.UnWrapInnerExceptions().Any(ex => ex is IndexMissingException));
            }
        }
        protected void ExpectOrderedResultASync(DateTime now)
        {
            ExpectedOrderedResult(_appender.GetPagedAsync(0, 2).Result, now);
        }

        protected void ExpectOrderedResultASync(ElasticSearchRepository.SearchTerm search, DateTime now)
        {
            ExpectedOrderedResult(_appender.GetPagedAsync(search, 0, 2).Result, now);
        }
        protected void ExpectEmptyResultASync(ElasticSearchRepository.SearchTerm search)
        {
            ExpectedEmptyResult(_appender.GetPagedAsync(search, 0, 2).Result);
        }
        protected void ExpectEmptyResultASync()
        {
            ExpectedEmptyResult(_appender.GetPagedAsync(0, 2).Result);
        }

#endif
        protected void ExpectOrderedResultSync(DateTime now)
        {
            ExpectedOrderedResult(_appender.GetPaged(0, 2), now);
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

        protected void ExpectEmptyResultSync(ElasticSearchRepository.SearchTerm search)
        {
            ExpectedEmptyResult(_appender.GetPaged(search, 0, 2));
        }
        protected void ExpectEmptyResultSync()
        {
            ExpectedEmptyResult(_appender.GetPaged(0, 2));
        }
    }
}