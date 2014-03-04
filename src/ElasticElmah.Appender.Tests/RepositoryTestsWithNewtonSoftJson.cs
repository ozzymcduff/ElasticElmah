using NUnit.Framework;
using System;

namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class RepositoryTestsWithNewtonSoftJson : RepositoryTests
    {
        [SetUp]
        public void Init()
        {
            var fiddler = Global.UseFiddler;
            var _index = Guid.NewGuid();
            var conn = "Server=" + (fiddler ? Environment.MachineName : "localhost") + ";Index=" + _index + ";Port=" + Global.Port;
            _appender = new ElasticSearchRepository(conn,
                new Web.JsonRequest(),
                new WrappedNewtonsoft());
            _appender.CreateIndex();
        }

        [TearDown]
        public void Cleanup()
        {
            _appender.DeleteIndex();
        }
    }
}
