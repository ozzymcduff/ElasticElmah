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
    public class ColorizationTests
    {
        [Test]
        public void Test() 
        {
            var html =new ColorizeStackTrace(TestData.AggregateException).Html();
            Assert.That(html, Is.StringContaining("System.AggregateException"));
            Assert.That(html, Is.StringContaining("System.Threading.Tasks.TaskFactory"));
            Assert.That(html, Is.StringContaining("Not Found"));
        }
    }
}
