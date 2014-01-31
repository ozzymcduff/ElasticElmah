using System;
using System.Linq;

namespace ElasticElmah.Appender.Presentation
{
    public class ParseJsStackTrace
    {
        private readonly Token[] _tokens;

        public ParseJsStackTrace(string stacktrace)
        {
            _tokens = new LexJsStackTrace(stacktrace).Tap(t => t.TokenizeLines()).Tokens.ToArray();
        }
        public event Action<Token> OnAccept;
        public event Action OnEnterLine;
        public event Action OnExitLine;
        public event Action OnEnterStackFrame;
        public event Action OnExitStackFrame;
        public event Action<string> OnWhiteSpace;
        private void whilemax(Func<bool> action, int max)
        {
            int count = 0;
            while (action() && count < max)
            {
                count++;
            }
            if (count >= max)
                throw new Exception();
        }
        public void Parse()
        {
            whilemax(() =>
            {
                whilemax(() => Accept(Symbols.Unrecognized), 1000);
                if (Accept(Symbols.None))
                {
                    return false;
                }
                OnEnterLine.TapNotNull(a => a());
                if (Accept(Symbols.At))
                {
                    OnEnterStackFrame.TapNotNull(a => a());
                    if (Accept(Symbols.Type))
                    {
                        Accept(Symbols.TypeMethodDelim);
                        Expect(Symbols.Method);
                    }
                    else
                    {
                        Expect(Symbols.Method);
                    }
                    Expect(Symbols.LeftParanthesis);
                    whilemax(() =>
                    {
                        if (Accept(Symbols.Type))
                        {
                            if (Accept(Symbols.Var))
                            {
                            }
                        }
                        if (Accept(Symbols.Comma))
                        {
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    }, 1000);
                    Expect(Symbols.RightParanthesis);
                    if (Accept(Symbols.In))
                    {
                        Expect(Symbols.File);
                        Expect(Symbols.Colon);
                        Expect(Symbols.Line);
                        Expect(Symbols.LineNumber);
                    }
                    OnExitStackFrame.TapNotNull(a => a());
                }
                OnExitLine.TapNotNull(a => a());
                return true;
            }, 2000);
        }

        private int _current;
        public Token? CurrentToken
        {
            get
            {
                if (_tokens.Length > _current)
                    return _tokens[_current];
                return null;
            }
        }
        public Token? PreviousToken
        {
            get
            {
                if (_current - 1 >= 0)
                {
                    return _tokens[_current - 1];
                }
                return null;
            }
        }
        private Symbols Sym
        {
            get
            {
                if (_tokens.Length > _current)
                    return _tokens[_current].Type;
                return Symbols.None;
            }
        }
        private void GetSym()
        {
            _current++;
        }

        bool Accept(Symbols s)
        {
            while (Sym == Symbols.Whitespace)
            {
                OnWhiteSpace.TapNotNull(a => a(CurrentToken.Value.Value));
                GetSym();
            }

            if (Sym == s)
            {
                if (CurrentToken.HasValue)
                {
                    OnAccept.TapNotNull(a => a(CurrentToken.Value));
                }
                GetSym();
                return true;
            }
            return false;
        }

        void Expect(Symbols s)
        {
            if (Accept(s))
                return;
            throw new Exception("expect: unexpected symbol");
        }
    }
}