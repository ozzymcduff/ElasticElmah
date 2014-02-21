using System;
using NUnit.Framework;
using log4net.Core;
using log4net;
using System.Reflection;
using System.Threading.Tasks;
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
#if ASYNC
        [Test, Ignore("throws in different ways depending on proxy or not")]
        public virtual void Should_throw_exception_on_wait()
        {
            _appender.Async = true;// not needed
            Assert.Throws<AggregateException>(() =>
            {
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
                        })).Wait(100);
            });
        }
        [Test,Ignore("throws in different ways depending on proxy or not")]
        public virtual void Should_throw()
        {
            var faulted = false;
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
                    })).ContinueWith(t => { faulted = true; }, 
                    TaskContinuationOptions.OnlyOnFaulted).Wait();
            Assert.That(faulted);
        }
#endif
    }
}
