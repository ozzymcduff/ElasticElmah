using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ElasticElmah.Appender.Presentation
{
    public class ColorizeStackTrace
    {
        ParseStackTrace parser;
        StringBuilder sb;
        
        public ColorizeStackTrace(string stacktrace)
        {
            parser = new ParseStackTrace(stacktrace);
            parser.onEnterStackFrame += onEnterStackFrame;
            parser.onExitStackFrame += onExitStackFrame;
            parser.onWhiteSpace += onWhiteSpace;
            sb = new StringBuilder();
            parser.onAccept += onAccept;

        }

        void onWhiteSpace(string obj)
        {
            sb.Append(HtmlEncode(obj));
        }

        private string HtmlEncode(string val) 
        {
            return HttpUtility.HtmlEncode(val);
        }

        public string Html() 
        {
            parser.Parse();
            return sb.ToString();
        }

        void onAccept(Token obj)
        {
            var classOf = GetClassOf(obj);
            if (string.IsNullOrEmpty(classOf))
            {
                sb.Append(HtmlEncode(obj.Value));
            }
            else
            {
                sb.AppendFormat("<span class=\"{0}\">",classOf);
                sb.Append(HtmlEncode(obj.Value));
                sb.Append("</span>");
            }
        }
        void onEnterStackFrame()
        {
            sb.Append("<span class=\"st-frame\">");
        }

        void onExitStackFrame()
        {
            sb.Append("</span>");
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
