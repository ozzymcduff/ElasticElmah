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
        public void Test() 
        {
            var str = "   at ElasticElmahMVC.Models.ErrorLogPage.OnLoad() in c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs:line 56";

            var p = new ParseStackTrace(str).Tap(t=>t.Parse());
        }
        [Test]
        public void Real() 
        {
            var p = new ParseStackTrace(TestData.AggregateException).Tap(t => t.Parse());
        }

    }
}
