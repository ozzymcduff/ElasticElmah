using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Presentation
{
    public class ParseStackTrace
    {
        private Token[] tokens;
        private string stacktrace;
        public ParseStackTrace(string stacktrace)
        {
            this.stacktrace = stacktrace;
            tokens = TokenizeStackTrace.Tokenize(stacktrace).ToArray();
        }
        public event Action<Token> onAccept;
        public event Action onEnterLine;
        public event Action onExitLine;
        public event Action onEnterStackFrame;
        public event Action onExitStackFrame;
        public event Action<string> onWhiteSpace;
        private void whilemax(Func<bool> action, int max)
        {
            int count = 0;
            while (action() && count<max)
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
                whilemax(() => accept(Symbols.Unrecognized), 1000);
                if (accept(Symbols.None))
                {
                    return false;
                }
                onEnterLine.TapNotNull(a => a());
                if (accept(Symbols.At))
                {
                    onEnterStackFrame.TapNotNull(a => a());
                    if (accept(Symbols.Type))
                    {
                        expect(Symbols.TypeMethodDelim);
                        expect(Symbols.Method);
                    }
                    else
                    {
                        expect(Symbols.Method);
                    }
                    expect(Symbols.LeftParanthesis);
                    whilemax(() =>
                    {
                        if (accept(Symbols.Type))
                        {
                            if (accept(Symbols.Var))
                            {
                            }
                        }
                        if (accept(Symbols.Comma))
                        {
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    }, 1000);
                    expect(Symbols.RightParanthesis);
                    if (accept(Symbols.In))
                    {
                        expect(Symbols.File);
                        expect(Symbols.Colon);
                        expect(Symbols.Line);
                        expect(Symbols.LineNumber);
                    }
                    onExitStackFrame.TapNotNull(a => a());
                }
                onExitLine.TapNotNull(a => a());
                return true;
            }, 2000);
        }

        private int current = 0;
        public Token? CurrentToken
        {
            get
            {
                if (tokens.Length > current)
                    return tokens[current];
                return null;
            }
        }
        public Token? PreviousToken
        {
            get 
            {
                if (current - 1 >= 0) 
                {
                    return tokens[current - 1];
                }
                return null;
            }
        }
        private Symbols sym
        {
            get
            {
                if (tokens.Length > current)
                    return tokens[current].Type;
                return Symbols.None;
            }
        }
        private void getsym()
        {
            current++;
        }

        bool accept(Symbols s)
        {
            if (sym == s)
            {

                if (PreviousToken.HasValue && CurrentToken.HasValue && PreviousToken.Value.LastPosition()<CurrentToken.Value.Position-1)
                {
                    onWhiteSpace.TapNotNull(a => a(stacktrace.Substring(PreviousToken.Value.LastPosition() + 1, CurrentToken.Value.Position - PreviousToken.Value.LastPosition()-1)));
                }
                if (CurrentToken.HasValue)
                {
                    onAccept.TapNotNull(a => a(CurrentToken.Value)); 
                }
                getsym();
                return true;
            }
            return false;
        }

        bool expect(Symbols s)
        {
            if (accept(s))
                return true;
            throw new Exception("expect: unexpected symbol");
        }
    }
}
