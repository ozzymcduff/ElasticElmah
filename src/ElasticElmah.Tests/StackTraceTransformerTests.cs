using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ElasticElmah.Appender.Tests;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ElasticElmah.Core;
using System.Text.RegularExpressions;

namespace ElasticElmah.Tests
{
    [TestFixture]
    public class StackTraceTransformerTests
    {
        string argumentnullexception = @"System.ArgumentNullException: Value cannot be null.
Parameter name: httpContext
   at System.Web.HttpContextWrapper..ctor(HttpContext httpContext)
   at ElasticElmahMVC.Models.ErrorLogPage.OnLoad() in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmahMVC\Models\ErrorLogPage.cs:line 56
   at ElasticElmahMVC.Controllers.HomeController.<Index>d__2.MoveNext() in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmahMVC\Controllers\HomeController.cs:line 34
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.GetResult()
   at lambda_method(Closure , Task )
   at System.Threading.Tasks.TaskHelpersExtensions.ThrowIfFaulted(Task task)
   at System.Web.Mvc.Async.TaskAsyncActionDescriptor.EndExecute(IAsyncResult asyncResult)
   at System.Web.Mvc.Async.AsyncControllerActionInvoker.<>c__DisplayClass3f.<BeginInvokeAsynchronousActionMethod>b__3e(IAsyncResult asyncResult)
   at System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncResult`1.End()
   at System.Web.Mvc.Async.AsyncControllerActionInvoker.EndInvokeActionMethod(IAsyncResult asyncResult)
   at System.Web.Mvc.Async.AsyncControllerActionInvoker.<>c__DisplayClass37.<>c__DisplayClass39.<BeginInvokeActionMethodWithFilters>b__33()
   at System.Web.Mvc.Async.AsyncControllerActionInvoker.<>c__DisplayClass4f.<InvokeActionMethodFilterAsynchronously>b__49()";
        private StackTraceTransformer transfomer;
        [SetUp]
        public void Init()
        {
            transfomer = new StackTraceTransformer();
        }

        [TearDown]
        public void Cleanup()
        {
        }

        [Test]
        public void ReadAppend()
        {
            var matches = transfomer.Match(argumentnullexception);
            foreach (var item in matches)
            {
                Console.WriteLine("m:");
                Console.WriteLine(item.First);
                /*                (?<type> .+ ) \.
                (?<method> .+? ) 
                (?<params> \( (?<params> .*? ) \) )
                ( \s+ 
                \w+ \s+ 
                  (?<file> [a-z] \: .+? ) 
                  \: \w+ \s+ 
                  (?<line> [0-9]+ ) \p{P}? )?
*/
                WriteGroup(item,"type");
                WriteGroup(item, "method");
                WriteGroup(item, "params");
                WriteGroup(item, "file");
                WriteGroup(item, "line");
                //foreach (Group g in item.Groups)
                //{
                //    Console.WriteLine(g);
                //    Console.WriteLine(g.Value);
                //}
            }
        }

        private static void WriteGroup(StackTraceTransformer.MyClass item, string name)
        {
            Console.WriteLine(name+":");
            Console.WriteLine(item.Groups[name]);
        }
    }
}
