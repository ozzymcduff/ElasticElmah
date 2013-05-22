using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ElasticElmah.Appender.Presentation;
namespace ElasticElmah.Appender.Tests
{
    [TestFixture]
    public class StackTraceTransformerTests
    {
        private static void Write(IEnumerable<Token> res)
        {
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine(string.Join(", ", res.Select(r => r.ToString())));
            Console.WriteLine("----------------------------------------------------");
        }
        
        [Test]
        public virtual void Single_line_with_type_and_method()
        {
            var str = "   at ElasticElmahMVC.Models.ErrorLogPage.OnLoad() in c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs:line 56";
  
            var tokenized = new[]{
                new{ t= "_at", s= "at" ,p=3},
                new{ t= "type", s= "ElasticElmahMVC.Models.ErrorLogPage", p= 5},
                new{ t= "type_method_delim", s= ".", p=41},
                new{ t= "method", s= "OnLoad" , p=42},
                new{ t= "_(",s="(", p=48},
                new{ t= "_)",s=")", p=49},
                new{ t= "_in", s= "in", p=51 },
                new{ t= "file", s= "c:\\Users\\Oskar\\Documents\\GitHub\\ElasticElmah\\src\\ElasticElmahMVC\\Models\\ErrorLogPage.cs" , p=54},
                new{ t= "_:", s= ":" , p=141},
                new{ t= "_line", s= "line" , p=142},
                new{ t= "line", s= "56" , p=147}
            }.Select(t => new Token(t.t, t.s, t.p));

            var res = StackTraceTransformer.Tokenize(str);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Line_with_ctor()
        {
            var str = "     at System.Web.HttpContextWrapper..ctor(HttpContext httpContext)";
            var tokenized = new[]{ 
                new{ t= "_at", s= "at", p=5 },
                new{ t= "type", s= "System.Web.HttpContextWrapper" , p=7},
                new{ t= "type_method_delim", s= ".", p=37 },
                new{ t= "method", s= ".ctor", p=38 },
                new{ t= "_(", s= "(" , p=43},
                new{ t= "type", s= "HttpContext", p=44 },
                new{ t= "var", s= "httpContext", p=56 },
                new{ t= "_)", s= ")" , p=67} 
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = StackTraceTransformer.Tokenize(str);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

        [Test]
        public virtual void Line_without_var_name_in_parameter()
        {
            var str = "   at lambda_method(Closure , Task )";
            var tokenized = new[]{
                new{ t= "_at", s= "at",p=3 },
                new{ t= "method", s= "lambda_method",p=6 },
                new{ t= "_(", s= "(",p=19 },
                new{ t= "type", s= "Closure",p=20 },
                new{ t= "_,", s= ",",p=28 },
                new{ t= "type", s= "Task",p=30 },
                new{ t= "_)", s= ")",p=35 }
            }.Select(t => new Token(t.t, t.s, t.p));
            var res = StackTraceTransformer.Tokenize(str);
            Assert.That(res, Is.EquivalentTo(tokenized));
        }

    }
}
