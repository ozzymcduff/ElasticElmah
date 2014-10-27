using System;
using NUnit.Framework;
using log4net.Core;
using log4net;
using System.Reflection;

#if NET20
namespace ElasticElmah.Appender.net20.Tests
#else
namespace ElasticElmah.Appender.Tests
#endif
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
            var fiddler = Global.UseFiddler;
            var index = Guid.NewGuid();
            var conn = "Server=" + (fiddler ? Environment.MachineName : "localhost") + ";Index=" + index + ";Port=" + Global.Port;
            _repo = new ElasticSearchRepository(conn, serializer: new DefaultJsonSerializer());
            _appender = new ElasticSearchAppender() { ConnectionString = conn};
        }

        [TearDown]
        public virtual void Cleanup()
        {
            _repo.DeleteIndex();
        }

        [Test]
        public virtual void Test()
        {
            _appender.CreateIndexIfNeededAndAppend(new LoggingEvent(GetType(), _log.Logger.Repository, TestData()));
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(1));
        }

        [Test, Ignore("Does not work on mono3.2.3")]
        public virtual void Test_generated_from_logging_event_data()
        {
            _appender.CreateIndexIfNeededAndAppend(new LoggingEvent(
                new LoggingEventData
                {
                    Message = "message",
                    Level = Level.Error,
                    LocationInfo = new LocationInfo("?", "?", "http://localhost:1341243/dsfaf", "21")
                }));
            //System.Threading.Thread.Sleep(10000);
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(1));
        }

        [Test]
        public virtual void Several_logs()
        {
            for (int i = 0; i < 5; i++)
            {
                _appender.CreateIndexIfNeededAndAppend(
                    new LoggingEvent(GetType(), _log.Logger.Repository, TestData()));
            }
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(5));
        }

        [Test]
        public virtual void Using_doappend()
        {
            _appender.DoAppend(new LoggingEvent(TestData()));
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(1));
        }

        private static LoggingEventData TestData()
        {
            return new LoggingEventData
            {
                TimeStamp = DateTime.Now,
                Level = Level.Error,
                Message = "Message",
                Properties = new log4net.Util.PropertiesDictionary().Tap(d =>
                {
                    d["prop"] = "msg";
                })
            };
        }
    }
}
