using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElasticElmah.Appender.Tests;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;

namespace ElasticElmah.Tests
{

    [TestFixture]
    public class DebugAppenderOnWrongHost:AppenderOnWrongHostTests
    {
        [SetUp]
        public override void Init()
        {
            base.Init();
        }

        [TearDown]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [Test,IgnoreAttribute]
        public override void Should_throw()
        {
            base.Should_throw();
        }
    }
}
