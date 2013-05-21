using System;
using NUnit.Framework;
using log4net.Core;
using log4net;
using System.Reflection;
namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class AppenderOnWrongHostTests
    {
        protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ElasticSearchAppender _appender;
        [SetUp]
        public virtual void Init()
        {
            var _index = Guid.NewGuid();
            var conn = "Server=" + Guid.NewGuid() + ";Index=" + _index + ";Port=9200";
            _appender = new ElasticSearchAppender() { ConnectionString = conn };
        }

        [TearDown]
        public virtual void Cleanup()
        {
            //_repo.DeleteIndexAsync();
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
        }

    }
}
