using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ElasticElmah.Appender.Presentation
{
    public class LexStackTrace
    {
        List<Token> tokens = new List<Token>();
        public IEnumerable<Token> Tokens
        {
            get
            {
                return tokens;
            }
        }
        int position = 0;
        string str;
        public LexStackTrace(string str)
        {
            this.str = str;
        }
        void AddStr(Symbols type, int index, int length)
        {
            CheckForWhiteSpace(str, index);
            tokens.Add(new Token(type, str.Substring(index, length), index));
            position = index + length;
        }
        void Add(Symbols type, Group value)
        {
            CheckForWhiteSpace(str, value.Index);
            tokens.Add(new Token(type, value.Value, value.Index));
            position = value.Index + value.Length;
        }
        char[] whitespaces = new char[] { '\n', '\r', '\t', ' ' };
        void CheckForWhiteSpace(string str, int index)
        {
            if (position < index)
            {
                for (int i = position + 1; i < index; i++)
                {
                    if (!whitespaces.Contains(str[i]))
                    {
                        throw new Exception(string.Format("'{0}'", str[i]));
                    }
                }
                tokens.Add(new Token(Symbols.Whitespace, str.Substring(position, index - position), position));
            }
        }

        void TokenizeLine(int index, int length)
        {
            if (!TryTokenizeAtInLine(index, length))
            {
                AddStr(Symbols.Unrecognized, index, length);
            }
        }

        Regex atTypeMethod = new Regex(@"^(\s*)(?<at>at)(\s+)(?<typenmethod>[^\(]+)(?<left>\()(?<params>[^\(]*)(?<right>\))(?<rest>.*)$");

        bool TryTokenizeAtInLine(int index, int length)
        {
            var m = atTypeMethod.Match(str, index, length);
            if (m.Success)
            {
                Add(Symbols.At, m.Groups["at"]);
                TokenizeTypeAndMethod(str, m.Groups["typenmethod"].Index, m.Groups["typenmethod"].Length);
                Add(Symbols.LeftParanthesis, m.Groups["left"]);
                if (!string.IsNullOrEmpty(m.Groups["params"].Value))
                {
                    TokenizeParams(m.Groups["params"].Index, m.Groups["params"].Length);
                }
                Add(Symbols.RightParanthesis, m.Groups["right"]);
                TryTokenizeIn(m.Groups["rest"].Index, m.Groups["rest"].Length);
                return true;
            }
            else
            {
                return false;
            }
        }
        Regex param = new Regex(@"^(\s*)(?<type>[^ ]*)(\s*)(?<var>[^ ]*)(\s*)");

        void TokenizeParams(int index, int length)
        {
            var i = index;//todo:regex instead
            var chars = new[] { ',', ' ' };
            for (int j = index; j < index + length; j++)
            {
                if (str[j] == chars[0] && str[j + 1] == chars[1])
                {
                    TokenizeParam(i, j - i);
                    AddStr(Symbols.Comma, j, 1);
                    i = j + 2;
                }
            }
            if (i < index + length)
            {
                TokenizeParam(i, index + length - i);
            }
        }

        private void TokenizeParam(int index, int length)
        {
            var m = param.Match(str, index, length);
            if (m.Success)
            {
                Add(Symbols.Type, m.Groups["type"]);
                if (m.Groups["var"].Value.Length > 0)
                {
                    Add(Symbols.Var, m.Groups["var"]);
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

        void TryTokenizeIn(int index, int length)
        {
            if (string.IsNullOrEmpty(str.Substring(index, length)))
            {
                return;
            }
            var m = inFileLine.Match(str, index, length);
            if (m.Success)
            {
                Add(Symbols.In, m.Groups["in"]);
                Add(Symbols.File, m.Groups["file"]);
                Add(Symbols.Colon, m.Groups["delim"]);
                Add(Symbols.Line, m.Groups["line"]);
                Add(Symbols.LineNumber, m.Groups["linenum"]);
            }
            else
            {
                AddStr(Symbols.Unrecognized, index, length);
            }
        }
        Regex word = new Regex(@"^(?<word>\w*)$");
        Regex delimMethod = new Regex(@"(?<tdelim>\.)(?<method>\.?[^\.]*)$");
        void TokenizeTypeAndMethod(string str, int index, int length)
        {
            var mm = word.Match(str, index, length);
            if (mm.Success)
            {
                Add(Symbols.Method, mm.Groups["word"]);
                return;
            }

            var m = delimMethod.Match(str, index, length);
            if (!m.Success)
            {
                throw new Exception(@"#expected type and method but found: 

""" + str.Substring(index, length) + @"""

");
            }

            AddStr(Symbols.Type, index, length - m.Length);
            Add(Symbols.TypeMethodDelim, m.Groups["tdelim"]);
            Add(Symbols.Method, m.Groups["method"]);
        }

        public static IEnumerable<Token> Tokenize(string str)
        {
            var transformer = new LexStackTrace(str);
            transformer.TokenizeLines();
            return transformer.tokens
                .Where(t => t.Type != Symbols.Whitespace)
                .ToArray();
        }

        public void TokenizeLines()
        {
            var i = 0;//todo:regex instead
            var chars = new[] { '\r', '\n' };
            for (int j = 0; j < str.Length; j++)
            {
                if (chars.Contains(str[j]) && i + 1 < j)
                {
                    TokenizeLine(i, j - i);
                    i = j;
                }
            }
            if (i < str.Length - 1)
            {
                TokenizeLine(i, str.Length - i);
            }
        }
    }
}
