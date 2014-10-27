using NUnit.Framework;
using System;
#if NET20
namespace ElasticElmah.Appender.net20.Tests
#else
namespace ElasticElmah.Appender.Tests
#endif
{
    [TestFixture]
    public class RepositoryTestsWithDefaultJson:RepositoryTests
    {


        [SetUp]
        public void Init()
        {
            var fiddler = Global.UseFiddler;
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
