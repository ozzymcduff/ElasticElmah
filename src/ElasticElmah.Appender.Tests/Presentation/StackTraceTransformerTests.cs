using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                string.Format("new{{ t= Symbols.{0}, s= \"{1}\",p={2} }},",r.Type.ToString(),r.Value,r.Position)
                )));
            Console.WriteLine("----------------------------------------------------");
        }
        
        [Test]
        public virtual void Single_line_with_type_and_method()
        {
            var str = "   at ElasticElmahMVC.Models.ErrorLogPage.OnLoad() in c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs:line 56";
  
            var tokenized = new[]{
                new{ t= Symbols._at, s= "at" ,p=3},
                new{ t= Symbols.type, s= "ElasticElmahMVC.Models.ErrorLogPage", p= 6},
                new{ t= Symbols.type_method_delim, s= ".", p=41},
                new{ t= Symbols.method, s= "OnLoad" , p=42},
                new{ t= Symbols._leftParanthesis,s="(", p=48},
                new{ t= Symbols._rightParanthesis,s=")", p=49},
                new{ t= Symbols._in, s= "in", p=51 },
                new{ t= Symbols.file, s= "c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs" , p=54},
                new{ t= Symbols._colon, s= ":" , p=141},
                new{ t= Symbols._line, s= "line" , p=142},
                new{ t= Symbols.line, s= "56" , p=147}
            }.Select(t => new Token(t.t, t.s, t.p)).ToArray();

            var res = TokenizeStackTrace.Tokenize(str).ToArray();
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Line_with_ctor()
        {
            var str = "     at System.Web.HttpContextWrapper..ctor(HttpContext httpContext)";
            var tokenized = new[]{ 
                new{ t= Symbols._at, s= "at", p=5 },
                new{ t= Symbols.type, s= "System.Web.HttpContextWrapper" , p=8},
                new{ t= Symbols.type_method_delim, s= ".", p=37 },
                new{ t= Symbols.method, s= ".ctor", p=38 },
                new{ t= Symbols._leftParanthesis, s= "(" , p=43},
                new{ t= Symbols.type, s= "HttpContext", p=44 },
                new{ t= Symbols.var, s= "httpContext", p=56 },
                new{ t= Symbols._rightParanthesis, s= ")" , p=67} 
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = TokenizeStackTrace.Tokenize(str);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Line_without_var_name_in_parameter()
        {
            var str = "   at lambda_method(Closure , Task )";
            var tokenized = new[]{
                new{ t= Symbols._at, s= "at",p=3 },
                new{ t= Symbols.method, s= "lambda_method",p=6 },
                new{ t= Symbols._leftParanthesis, s= "(",p=19 },
                new{ t= Symbols.type, s= "Closure",p=20 },
                new{ t= Symbols.comma, s= ",",p=28 },
                new{ t= Symbols.type, s= "Task",p=30 },
                new{ t= Symbols._rightParanthesis, s= ")",p=35 }
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = TokenizeStackTrace.Tokenize(str);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Wierd_looking_line() 
        {
            var str = @" at ElasticElmah.Appender.Web.JsonRequest.<>c__DisplayClass6.<Async>b__5(IAsyncResult iar) in c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequest.cs:line 70";
            var tokenized = new[]{
                new{ t= Symbols._at, s= "at",p=1 },
                new{ t=Symbols.type, s="ElasticElmah.Appender.Web.JsonRequest.<>c__DisplayClass6",p=4},
                new{ t= Symbols.type_method_delim, s= ".",p=60},
                new{ t= Symbols.method, s= "<Async>b__5",p=61 },
                new{ t= Symbols._leftParanthesis, s= "(",p=72 },
                new{ t= Symbols.type, s= "IAsyncResult",p=73 },
                new{ t= Symbols.var, s= "iar",p=86 },
                new{ t= Symbols._rightParanthesis, s= ")",p=89 },
                new{ t= Symbols._in, s= "in",p=91 },
                new{ t= Symbols.file, s=@"c:\Users\Oskar\Documents\GitHub\ElasticElmah\src\ElasticElmah.Appender\Web\JsonRequest.cs", p=94},
                new{ t= Symbols._colon, s= ":",p=183 },
                new{ t= Symbols._line, s= "line",p=184 },
                new{ t= Symbols.line, s= "70",p=189 }

            }.Select(t => new Token(t.t, t.s, t.p));
            var res = TokenizeStackTrace.Tokenize(str);

            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Another_wierd_line() 
        {
            var str = @" at System.Threading.Tasks.TaskFactory`1.FromAsyncCoreLogic(IAsyncResult iar, Func`2 endFunction, Action`1 endAction, Task`1 promise, Boolean requiresSynchronization)<---";
            var tokenized = new[]{
new{ t= Symbols._at, s= "at",p=1 },
new{ t= Symbols.type, s= "System.Threading.Tasks.TaskFactory`1",p=4 },
new{ t= Symbols.type_method_delim, s= ".",p=40 },
new{ t= Symbols.method, s= "FromAsyncCoreLogic",p=41 },
new{ t= Symbols._leftParanthesis, s= "(",p=59 },
new{ t= Symbols.type, s= "IAsyncResult",p=60 },
new{ t= Symbols.var, s= "iar",p=73 },
new{ t= Symbols.comma, s= ",",p=76 },
new{ t= Symbols.type, s= "Func`2",p=78 },
new{ t= Symbols.var, s= "endFunction",p=85 },
new{ t= Symbols.comma, s= ",",p=96 },
new{ t= Symbols.type, s= "Action`1",p=98 },
new{ t= Symbols.var, s= "endAction",p=107 },
new{ t= Symbols.comma, s= ",",p=116 },
new{ t= Symbols.type, s= "Task`1",p=118 },
new{ t= Symbols.var, s= "promise",p=125 },
new{ t= Symbols.comma, s= ",",p=132 },
new{ t= Symbols.type, s= "Boolean",p=134 },
new{ t= Symbols.var, s= "requiresSynchronization",p=142 },
new{ t= Symbols._rightParanthesis, s= ")",p=165 },
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = TokenizeStackTrace.Tokenize(str);
            //Write(res);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Lines() 
        {
            var lines = TestData.AggregateException.Split(new []{Environment.NewLine},StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                TokenizeStackTrace.Tokenize(line);
            }
        }
    }
}
