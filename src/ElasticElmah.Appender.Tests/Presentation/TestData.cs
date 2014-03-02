namespace ElasticElmah.Appender.Tests.Presentation
{
    public class TestData
    {
        public static string AggregateException = @"System.AggregateException: One or more errors occurred. ---> System.Net.WebException: The remote server returned an error: (404) Not Found. at System.Net.HttpWebRequest.EndGetResponse(IAsyncResult asyncResult) at ElasticElmah.Appender.Web.JsonRequest.Response(HttpWebRequest request, IAsyncResult iar) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequest.cs:line 93 at ElasticElmah.Appender.Web.JsonRequest.<>c__DisplayClass6.<Async>b__5(IAsyncResult iar) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequest.cs:line 70 at ElasticElmah.Appender.Web.JsonRequestExtensions.<>c__DisplayClass1`1.<AsTask>b__0(IAsyncResult iar) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequestExtensions.cs:line 29 at System.Threading.Tasks.TaskFactory`1.FromAsyncCoreLogic(IAsyncResult iar, Func`2 endFunction, Action`1 endAction, Task`1 promise, Boolean requiresSynchronization) --- End of inner exception stack trace --- at System.Threading.Tasks.Task.ThrowIfExceptional(Boolean includeTaskCanceledExceptions) at System.Threading.Tasks.Task`1.GetResultCore(Boolean waitCompletionNotification) at System.Threading.Tasks.Task`1.get_Result() at ElasticElmah.Core.ErrorLog.ElasticErrorLog.<>c__DisplayClass5.<GetErrorsAsync>b__3(Task`1 t) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Core\ErrorLog\ElasticErrorLog.cs:line 57 at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke() at System.Threading.Tasks.Task.Execute() --- End of stack trace from previous location where exception was thrown --- at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task) at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult() at ElasticElmahMVC.Controllers.HomeController.<Index>d__2.MoveNext() in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmahMVC\Controllers\HomeController.cs:line 35 --- End of stack trace from previous location where exception was thrown --- at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task) at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) at lambda_method(Closure , Task ) at System.Threading.Tasks.TaskHelpersExtensions.ThrowIfFaulted(Task task) at System.Web.Mvc.Async.TaskAsyncActionDescriptor.EndExecute(IAsyncResult asyncResult) at System.Web.Mvc.Async.AsyncControllerActionInvoker.<>c__DisplayClass3f.<BeginInvokeAsynchronousActionMethod>b__3e(IAsyncResult asyncResult) at System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncResult`1.End() at System.Web.Mvc.Async.AsyncControllerActionInvoker.EndInvokeActionMethod(IAsyncResult asyncResult) at System.Web.Mvc.Async.AsyncControllerActionInvoker.<>c__DisplayClass37.<>c__DisplayClass39.<BeginInvokeActionMethodWithFilters>b__33() at System.Web.Mvc.Async.AsyncControllerActionInvoker.<>c__DisplayClass4f.<InvokeActionMethodFilterAsynchronously>b__49() ---> (Inner Exception #0) System.Net.WebException: The remote server returned an error: (404) Not Found. at System.Net.HttpWebRequest.EndGetResponse(IAsyncResult asyncResult) at ElasticElmah.Appender.Web.JsonRequest.Response(HttpWebRequest request, IAsyncResult iar) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequest.cs:line 93 at ElasticElmah.Appender.Web.JsonRequest.<>c__DisplayClass6.<Async>b__5(IAsyncResult iar) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequest.cs:line 70 at ElasticElmah.Appender.Web.JsonRequestExtensions.<>c__DisplayClass1`1.<AsTask>b__0(IAsyncResult iar) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequestExtensions.cs:line 29 at System.Threading.Tasks.TaskFactory`1.FromAsyncCoreLogic(IAsyncResult iar, Func`2 endFunction, Action`1 endAction, Task`1 promise, Boolean requiresSynchronization)<--- ";


        public static string JsException = @"Error: jlksdfajlk: 'EJLKJDFSKLJFLK'
 json: '{""id"":""534583409"",""url"":""/234234/Person/molgan"",""Name"":""Molgan""}'
    at Object.toModel (http://localhost/bundled_app.js/?ticks=560660:825:15)
    at _afterInitialize (http://localhost/bundled_app.js/?ticks=560660:15484:24)
    at self.initialize (http://localhost/bundled_app.js/?ticks=560660:1974:22)
    at new go.m.Todo (http://localhost/bundled_app.js/?ticks=560660:15496:14)
    at Object.toModel (http://localhost/bundled_app.js/?ticks=560660:823:20)
    at http://localhost/bundled_app.js/?ticks=560660:16498:39
    at Function.Et (http://localhost/js/lib/lodash.min.js?ticks=38133077:22:185)
    at loadItems (http://localhost/bundled_app.js/?ticks=560660:16497:28)
    at Object.success (http://localhost/bundled_app.js/?ticks=560660:16538:17)
    at Object.lo.execIfFunc (http://localhost/lib_bundle.js/?ticks=560660:1576:14)";

        public static string JsException2 = @" .load/<@http://localhost/bundled_app.js?ticks=63529:9
some_method/this.get/<.error@http://localhost/bundled_app.js?ticks=63529:3
x.Callbacks/l@https://cdnjs.cloudflare.com/ajax/libs/jquery/4.1.7/jquery.min.js:4
x.Callbacks/c.fireWith@https://cdnjs.cloudflare.com/ajax/libs/jquery/4.1.7/jquery.min.js:4
k@https://cdnjs.cloudflare.com/ajax/libs/jquery/4.1.7/jquery.min.js:6
.send/t/<@https://cdnjs.cloudflare.com/ajax/libs/jquery/4.1.7/jquery.min.js:6";
    }

}
