using ElasticElmah.Appender.Presentation;
using NUnit.Framework;

namespace ElasticElmah.Appender.Tests.Presentation
{
    [TestFixture]
    public class ColorizationJsTests
    {
        [Test]
        public void Test() 
        {
            var html =new ColorizeStackTrace(TestData.JsException).Html();
            Assert.That(html, Is.StringContaining("/234234/Person/molgan"));
            Assert.That(html, Is.StringContaining("toModel"));
            Assert.That(html, Is.StringContaining("_afterInitialize"));
        }
        [Test]
        public void Test2()
        {
            var html = new ColorizeStackTrace(TestData.JsException2).Html();
            Assert.That(html, Is.StringContaining("some_method"));
            Assert.That(html, Is.StringContaining("Callbacks"));
        }

    }
}
