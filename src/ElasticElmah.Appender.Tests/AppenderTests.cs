using System;
using NUnit.Framework;
using log4net.Core;
using log4net;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            var fiddler = Global.UseFiddler;
            var index = Guid.NewGuid();
            var conn = "Server=" + (fiddler ? Environment.MachineName : "localhost") + ";Index=" + index + ";Port=" + Global.Port;
            _appender = new ElasticSearchAppender() { ConnectionString = conn };
            _repo = new ElasticSearchRepository(conn);
        }

        [TearDown]
        public virtual void Cleanup()
        {
            _repo.DeleteIndex();
        }

        [Test]
        public virtual void Test()
        {
            _appender.DoAppend(new LoggingEvent(GetType(), _log.Logger.Repository, TestData()));
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(1));
        }

        [Test]
        public virtual void Test_generated_from_logging_event_data()
        {
            _appender.DoAppend(new LoggingEvent(
                new LoggingEventData
                {
                    Message = "message",
                    Level = Level.Error,
                    LocationInfo = new LocationInfo("?", "?", "http://localhost:1341243/dsfaf", "21")
                }));
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(1));
        }

        [Test]
        public virtual void Several_logs()
        {
            _appender.Async = false;
            for (int i = 0; i < 5; i++)
            {
                _appender.DoAppend(
                    new LoggingEvent(GetType(), _log.Logger.Repository, TestData()));
            }
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(5));
        }

        [Test]
        public virtual void Using_doappend()
        {
            _appender.Async = false;
            _appender.DoAppend(new LoggingEvent(TestData()));
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(1));
        }
#if ASYNC
        [Test]
        public virtual void Using_doappend_async()
        {
            _appender.Async = true;
            _appender.DoAppend(new LoggingEvent(TestData()));
            _repo.Refresh();
            var paged = _repo.GetPaged(0, 10);
            Assert.That(paged.Total, Is.EqualTo(1));
        }
#endif

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
