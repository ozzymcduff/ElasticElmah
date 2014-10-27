using System;
using NUnit.Framework;

#if NET20
namespace ElasticElmah.Appender.net20.Tests
#else
namespace ElasticElmah.Appender.Tests
#endif
{
    [TestFixture]
    public class MissingIndexTests:Assertions
    {
        [SetUp]
        public void Init()
        {
            var fiddler = Global.UseFiddler;
            var _index = Guid.NewGuid();
            var conn = "Server=" + (fiddler ? Environment.MachineName : "localhost") + ";Index=" + _index + ";Port="+Global.Port;
            _appender = new ElasticSearchRepository(conn,
                new Web.JsonRequest(),
                new DefaultJsonSerializer());
        }

        [TearDown]
        public void Cleanup()
        {
            _appender = null;
        }

        [Test]
        public void Should_give_missing_index_result_when_no_index()
        {
            ExpectMissingIndexResultSync();
		}
    }
}
