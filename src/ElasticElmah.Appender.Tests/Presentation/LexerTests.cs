using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ElasticElmah.Appender.Presentation;
using ElasticElmah.Appender.Tests.Presentation;
namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class StackTraceTransformerTests
    {
        private static void Write(IEnumerable<Token> res)
        {
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine(string.Join(Environment.NewLine, res.Select(r =>
                string.Format("new{{ t= Symbols.{0}, s= \"{1}\",p={2} }},", r.Type.ToString(), r.Value, r.Position)
                )));
            Console.WriteLine("----------------------------------------------------");
        }

        [Test]
        public virtual void Single_line_with_type_and_method()
        {
            var str = "   at ElasticElmahMVC.Models.ErrorLogPage.OnLoad() in c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs:line 56";

            var tokenized = new[]{
                new{ t= Symbols.At, s= "at" ,p=3},
                new{ t= Symbols.Type, s= "ElasticElmahMVC.Models.ErrorLogPage", p= 6},
                new{ t= Symbols.TypeMethodDelim, s= ".", p=41},
                new{ t= Symbols.Method, s= "OnLoad" , p=42},
                new{ t= Symbols.LeftParanthesis,s="(", p=48},
                new{ t= Symbols.RightParanthesis,s=")", p=49},
                new{ t= Symbols.In, s= "in", p=51 },
                new{ t= Symbols.File, s= "c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs" , p=54},
                new{ t= Symbols.Colon, s= ":" , p=141},
                new{ t= Symbols.Line, s= "line" , p=142},
                new{ t= Symbols.LineNumber, s= "56" , p=147}
            }.Select(t => new Token(t.t, t.s, t.p));

            var res = LexStackTrace.Tokenize(str);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Line_with_ctor()
        {
            var str = "     at System.Web.HttpContextWrapper..ctor(HttpContext httpContext)";
            var tokenized = new[]{ 
                new{ t= Symbols.At, s= "at", p=5 },
                new{ t= Symbols.Type, s= "System.Web.HttpContextWrapper" , p=8},
                new{ t= Symbols.TypeMethodDelim, s= ".", p=37 },
                new{ t= Symbols.Method, s= ".ctor", p=38 },
                new{ t= Symbols.LeftParanthesis, s= "(" , p=43},
                new{ t= Symbols.Type, s= "HttpContext", p=44 },
                new{ t= Symbols.Var, s= "httpContext", p=56 },
                new{ t= Symbols.RightParanthesis, s= ")" , p=67} 
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = LexStackTrace.Tokenize(str);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Line_without_var_name_in_parameter()
        {
            var str = "   at lambda_method(Closure , Task )";
            var tokenized = new[]{
                new{ t= Symbols.At, s= "at",p=3 },
                new{ t= Symbols.Method, s= "lambda_method",p=6 },
                new{ t= Symbols.LeftParanthesis, s= "(",p=19 },
                new{ t= Symbols.Type, s= "Closure",p=20 },
                new{ t= Symbols.Comma, s= ",",p=28 },
                new{ t= Symbols.Type, s= "Task",p=30 },
                new{ t= Symbols.RightParanthesis, s= ")",p=35 }
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = LexStackTrace.Tokenize(str);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Line_without_var_name_in_parameter_whitespaces()
        {
            var str = "   at lambda_method(Closure , Task )";
            var tokenized = new[]{
                new{ t= Symbols.Whitespace, s= "   ",p=0 },
                new{ t= Symbols.At, s= "at",p=3 },
                new{ t= Symbols.Whitespace, s= " ", p=5 },
                new{ t= Symbols.Method, s= "lambda_method", p=6 },
                new{ t= Symbols.LeftParanthesis, s= "(",p=19 },
                new{ t= Symbols.Type, s= "Closure",p=20 },
                new{ t= Symbols.Whitespace, s= " ", p=27 },
                new{ t= Symbols.Comma, s= ",",p=28 },
                new{ t= Symbols.Whitespace, s= " ", p=29 },
                new{ t= Symbols.Type, s= "Task",p=30 },
                new{ t= Symbols.Whitespace, s= " ", p=34 },
                new{ t= Symbols.RightParanthesis, s= ")",p=35 },
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = new LexStackTrace(str).Tap(t => t.TokenizeLines()).Tokens;
            //Write(res);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Wierd_looking_line()
        {
            var str = @" at ElasticElmah.Appender.Web.JsonRequest.<>c__DisplayClass6.<Async>b__5(IAsyncResult iar) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequest.cs:line 70";
            var tokenized = new[]{
                new{ t= Symbols.At, s= "at",p=1 },
                new{ t=Symbols.Type, s="ElasticElmah.Appender.Web.JsonRequest.<>c__DisplayClass6",p=4},
                new{ t= Symbols.TypeMethodDelim, s= ".",p=60},
                new{ t= Symbols.Method, s= "<Async>b__5",p=61 },
                new{ t= Symbols.LeftParanthesis, s= "(",p=72 },
                new{ t= Symbols.Type, s= "IAsyncResult",p=73 },
                new{ t= Symbols.Var, s= "iar",p=86 },
                new{ t= Symbols.RightParanthesis, s= ")",p=89 },
                new{ t= Symbols.In, s= "in",p=91 },
                new{ t= Symbols.File, s=@"c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequest.cs", p=94},
                new{ t= Symbols.Colon, s= ":",p=183 },
                new{ t= Symbols.Line, s= "line",p=184 },
                new{ t= Symbols.LineNumber, s= "70",p=189 }

            }.Select(t => new Token(t.t, t.s, t.p));
            var res = LexStackTrace.Tokenize(str);

            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Another_wierd_line()
        {
            var str = @" at System.Threading.Tasks.TaskFactory`1.FromAsyncCoreLogic(IAsyncResult iar, Func`2 endFunction, Action`1 endAction, Task`1 promise, Boolean requiresSynchronization)<---";
            var tokenized = new[]{
new{ t= Symbols.At, s= "at",p=1 },
new{ t= Symbols.Type, s= "System.Threading.Tasks.TaskFactory`1",p=4 },
new{ t= Symbols.TypeMethodDelim, s= ".",p=40 },
new{ t= Symbols.Method, s= "FromAsyncCoreLogic",p=41 },
new{ t= Symbols.LeftParanthesis, s= "(",p=59 },
new{ t= Symbols.Type, s= "IAsyncResult",p=60 },
new{ t= Symbols.Var, s= "iar",p=73 },
new{ t= Symbols.Comma, s= ",",p=76 },
new{ t= Symbols.Type, s= "Func`2",p=78 },
new{ t= Symbols.Var, s= "endFunction",p=85 },
new{ t= Symbols.Comma, s= ",",p=96 },
new{ t= Symbols.Type, s= "Action`1",p=98 },
new{ t= Symbols.Var, s= "endAction",p=107 },
new{ t= Symbols.Comma, s= ",",p=116 },
new{ t= Symbols.Type, s= "Task`1",p=118 },
new{ t= Symbols.Var, s= "promise",p=125 },
new{ t= Symbols.Comma, s= ",",p=132 },
new{ t= Symbols.Type, s= "Boolean",p=134 },
new{ t= Symbols.Var, s= "requiresSynchronization",p=142 },
new{ t= Symbols.RightParanthesis, s= ")",p=165 },
new{ t= Symbols.Unrecognized, s="<---",p=166}
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = LexStackTrace.Tokenize(str);
            //Write(res);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Lines()
        {
            var lines = TestData.AggregateException.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                LexStackTrace.Tokenize(line);
            }
        }
    }
}
