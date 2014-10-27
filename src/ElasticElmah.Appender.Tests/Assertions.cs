using System;
using NUnit.Framework;
using System.Linq;
using ElasticElmah.Appender.Search;

#if NET20
namespace ElasticElmah.Appender.net20.Tests
#else
namespace ElasticElmah.Appender.Tests
#endif
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