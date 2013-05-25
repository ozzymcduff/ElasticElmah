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
using System.Text.RegularExpressions;
using ElasticElmah.Appender.Presentation;
using ElasticElmah.Appender.Tests.Presentation;

namespace ElasticElmah.Tests
{
    [TestFixture]
    public class DebugStackTraceTransformerTests : StackTraceTransformerTests
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
        [SetUp]
        public void Init()
        {
        }

        [TearDown]
        public void Cleanup()
        {
        }
        [Test]
        public override void Single_line_with_type_and_method()
        {
            base.Single_line_with_type_and_method();
        }
        [Test]
        public override void Line_with_ctor()
        {
            base.Line_with_ctor();
        }
        [Test]
        public override void Line_without_var_name_in_parameter()
        {
            base.Line_without_var_name_in_parameter();
        }
        [Test]
        public override void Lines()
        {
            base.Lines();
        }
        [Test]
        public void ReadAppend()
        {
            var tokens = ElasticElmah.Appender.Presentation.LexStackTrace.Tokenize(argumentnullexception);
            Console.WriteLine(tokens);
        }
        
        [Test]
        public override void Another_wierd_line()
        {
            base.Another_wierd_line();
        }

    }
}
