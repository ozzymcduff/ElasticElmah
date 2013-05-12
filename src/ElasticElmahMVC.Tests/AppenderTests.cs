using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using log4net;
using Nest;
using System.Linq;
using ElasticElmah.Appender;
using log4net.Core;

namespace ElmahMVC.Tests
{
    [TestClass]
    public class AppenderTests
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Guid index;
        private ElasticSearchAppender appender;
        [TestMethod]
        public void Log()
        {
            log.Debug("test");
        }
        [TestInitialize]
        public void Init()
        {
            index = Guid.NewGuid();
            appender = new ElasticSearchAppender() { ConnectionString = "Server=localhost;Index=" + index + ";Port=9200" };
            appender.CreateIndex();
        }

        [TestCleanup]
        public void Cleanup()
        {
            appender.DeleteIndex();
        }

        [TestMethod]
        public void Test()
        {
            appender.DoAppend(new log4net.Core.LoggingEvent(this.GetType(), log.Logger.Repository,
                new log4net.Core.LoggingEventData()
                {
                    Level = Level.Alert,
                    Message = "Message"
                }));
            appender.Flush();
            var result = appender.All();
            Assert.AreEqual(1, result.Count());
        }

    }
}
