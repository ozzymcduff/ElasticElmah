using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ElasticElmah.Appender.Presentation
{
    public class StackTraceTransformer
    {
        List<Token> tokens=new List<Token>();
        int position = 0;
        void Add(string type, string value, int index, int length)
        {
            tokens.Add(new Token(type, value.Substring(index,length), position));
            position = index+ length;
        }
        void Add(string type, Group value)
        {
            tokens.Add(new Token(type, value.Value, value.Index));
            position = value.Index + value.Length;
        }
        
        void TokenizeLine(string str, int index, int length)
        {
            if (!TryTokenizeAtInLine(str,index,length))
            {
                //--- End of stack trace from previous location where exception was thrown ---
                // or message, can be random something
                Add("unreq", str, position, length);
                //throw new Error('Line not req '+str);
            }
        }
        
        Regex atTypeMethod = new Regex(@"^(\s*)(?<at>at)(\s+)(?<typenmethod>[^\(]+)(?<left>\()(?<params>[^\(]*)(?<right>\))(?<rest>.*)$");

        bool TryTokenizeAtInLine(string str, int index, int length)
        {
            var m = atTypeMethod.Match(str, index, length);
            if (m.Success)
            {
                Add("_at", m.Groups["at"]);
                TokenizeTypeAndMethod(str, m.Groups["typenmethod"].Index, m.Groups["typenmethod"].Length);
                Add("_(", m.Groups["left"]);
                if (!string.IsNullOrEmpty(m.Groups["params"].Value))
                {
                    TokenizeParams(str, m.Groups["params"].Index, m.Groups["params"].Length);
                }
                Add("_)", m.Groups["right"]);
                TryTokenizeIn(str,m.Groups["rest"].Index,m.Groups["rest"].Length);
                return true;
            }
            else
            {
                return false;
            }
        }
        Regex param = new Regex(@"^(\s*)(?<type>\w*)(?<ws2>\s*)(?<var>[^, ]*)(\s*)(?<comma>,?)");

        void TokenizeParams(string str, int index, int length)
        {
            var m = param.Match(str, index, length);
            if (m.Success)
            {
                Add("type", m.Groups["type"]);
                if (m.Groups["var"].Value.Length > 0)
                {
                    Add("var", m.Groups["var"]);
                }
                if (m.Groups["comma"].Success && m.Groups["comma"].Length>0)
                {
                    Add("_,", m.Groups["comma"]);
                    TokenizeParams(str, m.Index + m.Length, length - m.Length);
                }
            }
            else
            {
                throw new Exception("expected param expression, but found " + str);
            }
        }
        Regex inFileLine = new Regex(@"^(\s*)(?<in>in)(\s)(?<file>\w?\:?[^:]*)(?<delim>\:)(?<line>line)(\s)(?<linenum>\d*)$");

        void TryTokenizeIn(string str, int index, int length)
        {
            if (string.IsNullOrEmpty(str.Substring(index,length)))
            {
                return;
            }
            var m = inFileLine.Match(str,index,length);
            if (m.Success)
            {
                Add("_in", m.Groups["in"]);
                Add("file", m.Groups["file"]);
                Add("_:", m.Groups["delim"]);
                Add("_line", m.Groups["line"]);
                Add("line", m.Groups["linenum"]);
            }
        }
        Regex word = new Regex(@"^(?<word>\w*)$");
        Regex delimMethod = new Regex(@"(?<tdelim>\.)(?<method>\.?\w*)$");
        void TokenizeTypeAndMethod(string str, int index, int length)
        {
            var mm = word.Match(str,index,length);
            if (mm.Success)
            {
                Add("method", mm.Groups["word"]);
                return;
            }

            var m = delimMethod.Match(str,index,length);
            if (!m.Success)
            {
                throw new Exception("expected type and method but found: " + str);
            }
            //var type = str.Substring(0, str.Length - m.Length);
            Add("type", str, index, length-m.Length);
            Add("type_method_delim", m.Groups["tdelim"]);
            Add("method", m.Groups["method"]);
        }

        public static IEnumerable<Token> Tokenize(string str)
        {
            var transformer = new StackTraceTransformer();
            var tokens = transformer.tokens;
            var list = new List<Tuple<int, int>>();
            var i = 0; 
            while (i < str.Length)
            {
                var next = str.IndexOf(Environment.NewLine,i);
                if (next < 0)
                {
                    next = str.Length;
                }
                list.Add(new Tuple<int, int>(i, next));
                i=next;
            }
            foreach (var item in list)
            {
                transformer.TokenizeLine(str,item.Item1,item.Item2);
            }
                //
            return tokens;
        }
    }
}
