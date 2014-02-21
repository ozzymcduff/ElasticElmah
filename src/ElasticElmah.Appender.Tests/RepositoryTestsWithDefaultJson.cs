using NUnit.Framework;
using System;

namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class RepositoryTestsWithDefaultJson:RepositoryTests
    {
        [SetUp]
        public void Init()
        {
            var fiddler = true;
            var _index = Guid.NewGuid();
            var conn = "Server=" + (fiddler ? Environment.MachineName : "localhost") + ";Index=" + _index + ";Port=" + Global.Port;
            _appender = new ElasticSearchRepository(conn,
                new Web.JsonRequest(),
                new DefaultJsonSerializer());
            _appender.CreateIndex();
        }

        [TearDown]
        public void Cleanup()
        {
            _appender.DeleteIndex();
        }
    }
}
