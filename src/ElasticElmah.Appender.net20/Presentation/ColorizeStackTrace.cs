using System;
using System.Text;
using System.Web;

namespace ElasticElmah.Appender.Presentation
{
    public class ColorizeStackTrace
    {
        readonly ParseStackTrace _parser;
        readonly StringBuilder _writer;
        
        public ColorizeStackTrace(string stacktrace)
        {
            _parser = new ParseStackTrace(stacktrace);
            _parser.OnEnterStackFrame += OnEnterStackFrame;
            _parser.OnExitStackFrame += OnExitStackFrame;
            _parser.OnWhiteSpace += OnWhiteSpace;
            _writer = new StringBuilder();
            _parser.OnAccept += OnAccept;

        }

        void OnWhiteSpace(string obj)
        {
            _writer.Append(HtmlEncode(obj));
        }

        private string HtmlEncode(string val) 
        {
            return HttpUtility.HtmlEncode(val);
        }

        public string Html() 
        {
            _parser.Parse();
            return _writer.ToString();
        }

        void OnAccept(Token obj)
        {
            var classOf = GetClassOf(obj);
            if (string.IsNullOrEmpty(classOf))
            {
                _writer.Append(HtmlEncode(obj.Value));
            }
            else
            {
                _writer.AppendFormat("<span class=\"{0}\">",classOf);
                _writer.Append(HtmlEncode(obj.Value));
                _writer.Append("</span>");
            }
        }
        void OnEnterStackFrame()
        {
            _writer.Append("<span class=\"st-frame\">");
        }

        void OnExitStackFrame()
        {
            _writer.Append("</span>");
        }

        public string GetClassOf(Token token)
        {
            switch (token.Type)
            {
                case Symbols.At:
                case Symbols.In:
                case Symbols.Line:
                    return "st-keyword";

                case Symbols.Type:
                case Symbols.TypeMethodDelim:
                    return "st-type";
                case Symbols.Method:
                    return "st-method";
                case Symbols.Var:
                    return "st-variable";
                case Symbols.File:
                case Symbols.Colon:
                    return "st-file";
                default:
                    return String.Empty;
            }
        }
    }
}
