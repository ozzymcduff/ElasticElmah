using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ElasticElmah.Appender.Presentation
{
    public class LexStackTrace
    {
        readonly List<Token> _tokens = new List<Token>();
        public IEnumerable<Token> Tokens
        {
            get
            {
                return _tokens;
            }
        }
        int _position;
        readonly string _str;
        public LexStackTrace(string str)
        {
            _str = str;
        }
        void AddStr(Symbols type, int index, int length)
        {
            CheckForWhiteSpace(_str, index);
            _tokens.Add(new Token(type, _str.Substring(index, length), index));
            _position = index + length;
        }
        void Add(Symbols type, Group value)
        {
            CheckForWhiteSpace(_str, value.Index);
            _tokens.Add(new Token(type, value.Value, value.Index));
            _position = value.Index + value.Length;
        }

        readonly char[] _whitespaces = new[] { '\n', '\r', '\t', ' ' };
        void CheckForWhiteSpace(string str, int index)
        {
            if (_position < index)
            {
                for (int i = _position + 1; i < index; i++)
                {
                    if (!_whitespaces.Contains(str[i]))
                    {
                        throw new Exception(string.Format("Not whitespace char '{0}', at '{1}'", str[i], str.Substring(i)));
                    }
                }
                _tokens.Add(new Token(Symbols.Whitespace, str.Substring(_position, index - _position), _position));
            }
        }

        void TokenizeLine(int index, int length)
        {
            if (!TryTokenizeAtInLine(index, length))
            {
                AddStr(Symbols.Unrecognized, index, length);
            }
        }

        readonly Regex _atTypeMethod = new Regex(@"^(\s*)(?<at>at)(\s+)(?<typenmethod>[^\(]+)(?<left>\()(?<params>[^\(]*)(?<right>\))(?<rest>.*)$");

        bool TryTokenizeAtInLine(int index, int length)
        {
            var m = _atTypeMethod.Match(_str, index, length);
            if (m.Success)
            {
                Add(Symbols.At, m.Groups["at"]);
                TokenizeTypeAndMethod(_str, m.Groups["typenmethod"].Index, m.Groups["typenmethod"].Length);
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

        void TokenizeParams(int index, int length)
        {
            var i = index;//todo:regex instead
            var chars = new[] { ',', ' ' };
            for (int j = index; j < index + length; j++)
            {
                if (_str[j] == chars[0] && _str[j + 1] == chars[1])
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

        readonly Regex _param = new Regex(@"(?:\s*)(?<type>\S*)(?:\s*)(?<var>\S*)(?:\s*)");

        private void TokenizeParam(int index, int length)
        {
            var m = _param.Match(_str, index, length);
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

""" + _str.Substring(index, length) + @"""

");
            }
        }

        readonly Regex _inFileLine = new Regex(@"^(\s*)(?<in>in)(\s)(?<file>\w?\:?[^:]*)(?<delim>\:)(?<line>line)(\s)(?<linenum>\d*)$");

        void TryTokenizeIn(int index, int length)
        {
            if (string.IsNullOrEmpty(_str.Substring(index, length)))
            {
                return;
            }
            var m = _inFileLine.Match(_str, index, length);
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

        readonly Regex _word = new Regex(@"^(?<word>\w*)$");
        readonly Regex _delimMethod = new Regex(@"(?<tdelim>\.)?(?<method>\.?[^\.]*)$");
        void TokenizeTypeAndMethod(string str, int index, int length)
        {
            var mm = _word.Match(str, index, length);
            if (mm.Success)
            {
                Add(Symbols.Method, mm.Groups["word"]);
                return;
            }

            var m = _delimMethod.Match(str, index, length);
            if (!m.Success)
            {
                throw new Exception(@"#expected type and method but found: 

""" + str.Substring(index, length) + @"""

");
            }

            AddStr(Symbols.Type, index, length - m.Length);
            if (m.Groups["tdelim"].Success)
            {
            Add(Symbols.TypeMethodDelim, m.Groups["tdelim"]);
            }
            Add(Symbols.Method, m.Groups["method"]);
        }

        public static IEnumerable<Token> Tokenize(string str)
        {
            var transformer = new LexStackTrace(str);
            transformer.TokenizeLines();
            return transformer._tokens
                .Where(t => t.Type != Symbols.Whitespace)
                .ToArray();
        }

        public void TokenizeLines()
        {
            var i = 0;//todo:regex instead
            var chars = new[] { '\r', '\n' };
            for (int j = 0; j < _str.Length; j++)
            {
                if (chars.Contains(_str[j]) && i + 1 < j)
                {
                    TokenizeLine(i, j - i);
                    i = j;
                }
            }
            if (i < _str.Length - 1)
            {
                TokenizeLine(i, _str.Length - i);
            }
        }
    }
}
