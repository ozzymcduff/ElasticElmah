using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ElasticElmah.Core.ErrorLog;
using ElasticElmah.Core.Infrastructure;
using log4net.Util;
using Environment = ElasticElmahMVC.Code.Environment;
using log4net.Core;
using System.Collections.Generic;

namespace ElasticElmahMVC.Models
{
    #region Imports

    

    #endregion

    /// <summary>
    /// Renders an HTML page displaying details about an error from the 
    /// error log.
    /// </summary>
    public class ErrorDetailModel
    {
        private static readonly Regex _reStackTrace =
            new Regex(
                @"
                ^
                \s*
                \w+ \s+ 
                (?<type> .+ ) \.
                (?<method> .+? ) 
                (?<params> \( (?<params> .*? ) \) )
                ( \s+ 
                \w+ \s+ 
                  (?<file> [a-z] \: .+? ) 
                  \: \w+ \s+ 
                  (?<line> [0-9]+ ) \p{P}? )?
                \s*
                $",
                RegexOptions.IgnoreCase
                | RegexOptions.Multiline
                | RegexOptions.ExplicitCapture
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Compiled);

        private readonly Error _errorEntry;

        private readonly Environment environment;
        private string PageTitle;
        public Dictionary<string, string> Properties { get { return _errorEntry.Properties; } }
        public ErrorDetailModel(Error errorLogEntry, Environment environment)
        {
            this.environment = environment;
            _errorEntry = errorLogEntry;
            PageTitle = string.Format("Error: {0} [{1}]", _errorEntry.Type, _errorEntry.Id);
        }


        //
        // Do we have details, like the stack trace? If so, then write 
        // them out in a pre-formatted (pre) element. 
        // NOTE: There is an assumption here that detail will always
        // contain a stack trace. If it doesn't then pre-formatting 
        // might not be the right thing to do here.
        //
        public string Detail
        {
            get { return MarkupStackTrace(_errorEntry.Detail); }
        }

        // Write out the error log time. This will be in the local
        // time zone of the server. Would be a good idea to indicate
        // it here for the user.
        //
        public DateTime Time
        {
            get { return _errorEntry.Time; }
        }

        public string BasePageName
        {
            get { return environment.BasePageName; }
        }

        public string Title
        {
            get { return _errorEntry.Message; }
        }

        public string Type
        {
            get { return _errorEntry.Type; }
        }

        public string Message
        {
            get { return _errorEntry.Message; }
        }

        private string MarkupStackTrace(string text)
        {
            var writer = new StringWriter();
            int anchor = 0;

            foreach (Match match in _reStackTrace.Matches(text))
            {
                HtmlEncode(text.Substring(anchor, match.Index - anchor), writer);
                MarkupStackFrame(text, match, writer);
                anchor = match.Index + match.Length;
            }

            HtmlEncode(text.Substring(anchor), writer);
            return writer.ToString();
        }

        private void MarkupStackFrame(string text, Match match, TextWriter writer)
        {
            int anchor = match.Index;
            GroupCollection groups = match.Groups;

            //
            // Type + Method
            //

            Group type = groups["type"];
            HtmlEncode(text.Substring(anchor, type.Index - anchor), writer);
            anchor = type.Index;
            writer.Write("<span class='st-frame'>");
            anchor = StackFrameSpan(text, anchor, "st-type", type, writer);
            anchor = StackFrameSpan(text, anchor, "st-method", groups["method"], writer);

            //
            // Parameters
            //

            Group parameters = groups["params"];
            HtmlEncode(text.Substring(anchor, parameters.Index - anchor), writer);
            writer.Write("<span class='st-params'>(");
            int position = 0;
            foreach (string parameter in parameters.Captures[0].Value.Split(','))
            {
                int spaceIndex = parameter.LastIndexOf(' ');
                if (spaceIndex <= 0)
                {
                    Span(writer, "st-param", parameter.Trim());
                }
                else
                {
                    if (position++ > 0)
                        writer.Write(", ");
                    string argType = parameter.Substring(0, spaceIndex).Trim();
                    Span(writer, "st-param-type", argType);
                    writer.Write(' ');
                    string argName = parameter.Substring(spaceIndex + 1).Trim();
                    Span(writer, "st-param-name", argName);
                }
            }
            writer.Write(")</span>");
            anchor = parameters.Index + parameters.Length;

            //
            // File + Line
            //

            anchor = StackFrameSpan(text, anchor, "st-file", groups["file"], writer);
            anchor = StackFrameSpan(text, anchor, "st-line", groups["line"], writer);

            writer.Write("</span>");

            //
            // Epilogue
            //

            int end = match.Index + match.Length;
            HtmlEncode(text.Substring(anchor, end - anchor), writer);
        }

        private int StackFrameSpan(string text, int anchor, string klass, Group group, TextWriter writer)
        {
            return group.Success
                       ? StackFrameSpan(text, anchor, klass, group.Value, group.Index, group.Length, writer)
                       : anchor;
        }

        private int StackFrameSpan(string text, int anchor, string klass, string value, int index, int length,
                                   TextWriter writer)
        {
            HtmlEncode(text.Substring(anchor, index - anchor), writer);
            Span(writer, klass, value);
            return index + length;
        }

        private void Span(TextWriter writer, string klass, string value)
        {
            writer.Write("<span class='");
            writer.Write(klass);
            writer.Write("'>");
            HtmlEncode(value, writer);
            writer.Write("</span>");
        }

        private string HtmlEncode(string text)
        {
            return HttpContext.Current.Server.HtmlEncode(text);
        }

        private void HtmlEncode(string text, TextWriter writer)
        {
            HttpContext.Current.Server.HtmlEncode(text, writer);
        }
    }
}