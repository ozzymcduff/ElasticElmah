using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ElasticElmah.Appender.Presentation
{
    public enum Symbols
    {
        none,
        unreq,
        _leftParanthesis,
        _rightParanthesis,
        type,
        var,
        comma,
        _in,
        file,
        _colon,
        _line,
        line,
        method,
        type_method_delim,
        _at,

    }
    public class TokenizeStackTrace
    {
        List<Token> tokens=new List<Token>();
        void AddStr(Symbols type, string value, int index, int length)
        {
            tokens.Add(new Token(type, value.Substring(index,length), index));
        }
        void Add(Symbols type, Group value)
        {
            tokens.Add(new Token(type, value.Value, value.Index));
        }
        
        void TokenizeLine(string str, int index, int length)
        {
            if (!TryTokenizeAtInLine(str,index,length))
            {
                AddStr(Symbols.unreq, str, index, length);
            }
        }
        
        Regex atTypeMethod = new Regex(@"^(\s*)(?<at>at)(\s+)(?<typenmethod>[^\(]+)(?<left>\()(?<params>[^\(]*)(?<right>\))(?<rest>.*)$");

        bool TryTokenizeAtInLine(string str, int index, int length)
        {
            var m = atTypeMethod.Match(str, index, length);
            if (m.Success)
            {
                Add(Symbols._at, m.Groups["at"]);
                TokenizeTypeAndMethod(str, m.Groups["typenmethod"].Index, m.Groups["typenmethod"].Length);
                Add(Symbols._leftParanthesis, m.Groups["left"]);
                if (!string.IsNullOrEmpty(m.Groups["params"].Value))
                {
                    TokenizeParams(str, m.Groups["params"].Index, m.Groups["params"].Length);
                }
                Add(Symbols._rightParanthesis, m.Groups["right"]);
                TryTokenizeIn(str,m.Groups["rest"].Index,m.Groups["rest"].Length);
                return true;
            }
            else
            {
                return false;
            }
        }
        Regex param = new Regex(@"^(\s*)(?<type>[^ ]*)(\s*)(?<var>[^ ]*)(\s*)");

        void TokenizeParams(string str, int index, int length)
        {
            var i = index;//todo:regex instead
            var chars = new[] { ',', ' ' };
            for (int j = index; j < index+length; j++)
            {
                if (str[j]==chars[0] && str[j+1]==chars[1])
                {
                    TokenizeParam(str, i, j-i);
                    AddStr(Symbols.comma,str,j,1);
                    i = j+2;
                }
            }
            if (i < index+length)
            {
                TokenizeParam(str, i, index+length-i);
            }
        }

        private void TokenizeParam(string str, int index, int length)
        {
            var m = param.Match(str, index, length);
            if (m.Success)
            {
                Add(Symbols.type, m.Groups["type"]);
                if (m.Groups["var"].Value.Length > 0)
                {
                    Add(Symbols.var, m.Groups["var"]);
                }
            }
            else
            {
                throw new Exception(@"#expected param expression, but found :

""" + str.Substring(index, length) + @"""

");
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
                Add(Symbols._in, m.Groups["in"]);
                Add(Symbols.file, m.Groups["file"]);
                Add(Symbols._colon, m.Groups["delim"]);
                Add(Symbols._line, m.Groups["line"]);
                Add(Symbols.line, m.Groups["linenum"]);
            }
            else 
            {
                AddStr(Symbols.unreq, str, index, length);
            }
        }
        Regex word = new Regex(@"^(?<word>\w*)$");
        Regex delimMethod = new Regex(@"(?<tdelim>\.)(?<method>\.?[^\.]*)$");
        void TokenizeTypeAndMethod(string str, int index, int length)
        {
            var mm = word.Match(str,index,length);
            if (mm.Success)
            {
                Add(Symbols.method, mm.Groups["word"]);
                return;
            }

            var m = delimMethod.Match(str,index,length);
            if (!m.Success)
            {
                throw new Exception(@"#expected type and method but found: 

""" + str.Substring(index,length) + @"""

");
            }

            AddStr(Symbols.type, str, index, length-m.Length);
            Add(Symbols.type_method_delim, m.Groups["tdelim"]);
            Add(Symbols.method, m.Groups["method"]);
        }

        public static IEnumerable<Token> Tokenize(string str)
        {
            var transformer = new TokenizeStackTrace();
            var tokens = transformer.tokens;
            var i = 0;//todo:regex instead
            var chars = new[]{'\r','\n'};
            for (int j = 0; j < str.Length; j++)
            {
                if (chars.Contains(str[j]) && i+1<j)
                {
                    transformer.TokenizeLine(str, i, j-i);
                    i = j;
                }
            }
            if (i < str.Length - 1) 
            {
                transformer.TokenizeLine(str, i, str.Length - i);
            }
                //
            return tokens;
        }
    }
}
