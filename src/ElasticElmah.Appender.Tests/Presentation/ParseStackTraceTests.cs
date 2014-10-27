using ElasticElmah.Appender.Presentation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET20
namespace ElasticElmah.Appender.net20.Tests.Presentation
#else
namespace ElasticElmah.Appender.Tests.Presentation
#endif

{
    [TestFixture]
    public class ParseStackTraceTests
    {
        [Test]
        public void Real_data_smoke_test() 
        {
            new ParseStackTrace(TestData.AggregateException).Parse();
        }
    }
}
