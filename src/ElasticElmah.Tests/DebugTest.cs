using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElasticElmah.Appender;
using log4net;
using log4net.Core;
using Assert = NUnit.Framework.Assert;
using System.Reflection;
using NUnit.Framework;
using System.Threading;
namespace ElasticElmah.Tests
{
    [TestClass]
    public class DebugTest
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Guid _index;
        private ElasticSearchRepository _appender;
        [TestInitialize]
        public void Init()
        {
            var fiddler = true;
            _index = Guid.NewGuid();
            _appender = new ElasticSearchRepository("Server="+(fiddler?Environment.MachineName:"localhost")+";Index=" + _index + ";Port=9200", 
                new ElasticElmah.Appender.Web.JsonRequestAsync());
            _appender.CreateIndex();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _appender.DeleteIndex();
        }

        [TestMethod]
        public void ReadAppend()
        {
            var res = _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                new LoggingEventData
                {
                    Level = Level.Alert,
                    Message = "Message"
                }), errors => { });
            res.AsyncWaitHandle.WaitOne();
            _appender.Flush();
            _appender.GetPaged(0, 10, result => {
                Assert.AreEqual(1, result.Total);

                Assert.That(result.Documents.Single().Data.Message, Is.EqualTo("Message"));
            }).AsyncWaitHandle.WaitOne();
        }
        [TestMethod]
        public void TestThread()
        {
            int n = 0;
            System.Func<string> main = () =>
            {
                n = 10;
                //Assert.Fail("This should fail!");
                n = 20;
                return "";
            };
            var asyncResult = main.BeginInvoke(null, null);
            var result = main.EndInvoke(asyncResult);
            Assert.AreEqual(10, n);
        }
    }
}
