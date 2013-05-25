using ElasticElmah.Appender.Presentation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticElmah.Appender.Tests.Presentation
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
