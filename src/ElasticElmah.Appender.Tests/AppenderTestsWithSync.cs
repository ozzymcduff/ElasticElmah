using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class AppenderTestsWithSync : AppenderTests
    {
        [SetUp]
        public void Init()
        {
            var fiddler = true;
            var _index = Guid.NewGuid();
            var conn = "Server=" + (fiddler ? Environment.MachineName : "localhost") + ";Index=" + _index + ";Port=9200";
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
