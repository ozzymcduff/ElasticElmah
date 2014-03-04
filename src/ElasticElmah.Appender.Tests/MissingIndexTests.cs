using System;
using NUnit.Framework;

namespace ElasticElmah.Appender.Tests
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
