using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using log4net.Core;
using log4net;
using System.Reflection;
namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class AppenderTests
    {
        protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ElasticSearchAppender _appender;
        private ElasticSearchRepository _repo;
        [SetUp]
        public virtual void Init()
        {
            var fiddler = true;
            var _index = Guid.NewGuid();
            var conn = "Server=" + (fiddler ? Environment.MachineName : "localhost") + ";Index=" + _index + ";Port=9200";
            _appender = new ElasticSearchAppender() { ConnectionString = conn };
            _repo = new ElasticSearchRepository(conn);
        }

        [TearDown]
        public virtual void Cleanup()
        {
            _repo.DeleteIndexAsync();
        }

        [Test]
        public virtual void Test() 
        {
            _appender.Async = true;// not needed
            _appender.AppendAsync(new LoggingEvent(GetType(), _log.Logger.Repository,
                    new LoggingEventData
                    {
                        TimeStamp = DateTime.Now,
                        Level = Level.Error,
                        Message = "Message",
                        Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                        {
                            d["prop"] = "msg";
                        })
                    })).Wait();
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(1));
        }
    }
}
