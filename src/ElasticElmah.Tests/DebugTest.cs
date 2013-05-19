using System;
using System.Linq;
using ElasticElmah.Appender;
using ElasticElmah.Appender.Web;
using log4net;
using log4net.Core;
using System.Reflection;
using NUnit.Framework;
using ElasticElmah.Appender.Tests;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;

namespace ElasticElmah.Tests
{
    [TestFixture]
    public class DebugTest : AppenderTests
    {
        [SetUp]
        public void Init()
        {
            var fiddler = true;
            var _index = Guid.NewGuid();
            _appender = new ElasticSearchRepository("Server="+(fiddler?Environment.MachineName:"localhost")+";Index=" + _index + ";Port=9200", 
                new ElasticElmah.Appender.Web.JsonRequest());
            _appender.CreateIndex();
        }

        [TearDown]
        public void Cleanup()
        {
            _appender.DeleteIndex();
        }

        [Test]
        public void ReadAppend()
        {
            _appender.Add(new LoggingEvent(GetType(), _log.Logger.Repository,
                new LoggingEventData
                {
                    Level = Level.Alert,
                    Message = "Message"
                }));
            _appender.Refresh();
            var result= _appender.GetPagedAsync(0, 10).AwaitOne();
            Assert.AreEqual(1, result.Total);

            Assert.That(result.Hits.Single().Data.Message, Is.EqualTo("Message"));
        }
        [Test]
        public override void Can_log_properties()
        {
            base.Can_log_properties();
        }
        [Test]
        public override void Should_get_latest()
        {
            base.Should_get_latest();
        }

        [Test,Ignore]
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
