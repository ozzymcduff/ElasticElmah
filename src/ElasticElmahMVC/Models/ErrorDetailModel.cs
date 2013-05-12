

namespace Elmah
{
    #region Imports

    using ElasticElmahMVC.Code;
    using log4net.Util;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using NameValueCollection = System.Collections.Specialized.NameValueCollection;

    #endregion

    /// <summary>
    /// Renders an HTML page displaying details about an error from the 
    /// error log.
    /// </summary>

    public class ErrorDetailModel
    {
        Environment environment;
        public ErrorDetailModel(Error errorLogEntry, Environment environment)
        {
            this.environment = environment;
            this._errorEntry = errorLogEntry;
            this.PageTitle = string.Format("Error: {0} [{1}]", _errorEntry.Type, _errorEntry.Id);
        }
        public HtmlString Render(HtmlHelper helper)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new HtmlTextWriter(new StreamWriter(stream));
                RenderContents(writer, helper);
                writer.Flush();
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return new HtmlString(reader.ReadToEnd());
            }
        }
        private Error _errorEntry;
        
        protected void RenderContents(HtmlTextWriter writer, HtmlHelper helper)
        {
            RenderError(writer);

        }


        //
        // Do we have details, like the stack trace? If so, then write 
        // them out in a pre-formatted (pre) element. 
        // NOTE: There is an assumption here that detail will always
        // contain a stack trace. If it doesn't then pre-formatting 
        // might not be the right thing to do here.
        //
        public string Detail { get { return _errorEntry.Detail; } }
        // Write out the error log time. This will be in the local
        // time zone of the server. Would be a good idea to indicate
        // it here for the user.
        //
        public DateTime Time { get { return _errorEntry.Time; } }
        private void RenderError(HtmlTextWriter writer)
        {
            Error error = _errorEntry;

            //
            // If this error has context, then write it out.
            // ServerVariables are good enough for most purposes, so
            // we only write those out at this time.
            //

            RenderCollection(writer, error.Properties,
                "ServerVariables", "Server Variables");
        }

        private void RenderCollection(HtmlTextWriter writer,
            PropertiesDictionary collection, string id, string title)
        {
            //
            // If the collection isn't there or it's empty, then bail out.
            //

            if (collection == null || collection.Count == 0)
                return;

            //
            // Surround the entire section with a <div> element.
            //

            writer.AddAttribute(HtmlTextWriterAttribute.Id, id);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //
            // Write out the table caption.
            //

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "table-caption");
            writer.RenderBeginTag(HtmlTextWriterTag.P);
            this.HtmlEncode(title, writer);
            writer.RenderEndTag(); // </p>
            writer.WriteLine();

            //
            // Some values can be large and add scroll bars to the page
            // as well as ruin some formatting. So we encapsulate the
            // table into a scrollable view that is controlled via the 
            // style sheet.
            //

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "scroll-view");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //
            // Create a table to display the name/value pairs of the
            // collection in 2 columns.
            //

            Table table = new Table();
            table.CellSpacing = 0;

            //
            // Create the header row and columns.
            //

            TableRow headRow = new TableRow();

            TableHeaderCell headCell;

            headCell = new TableHeaderCell();
            headCell.Wrap = false;
            headCell.Text = "Name";
            headCell.CssClass = "name-col";

            headRow.Cells.Add(headCell);

            headCell = new TableHeaderCell();
            headCell.Wrap = false;
            headCell.Text = "Value";
            headCell.CssClass = "value-col";

            headRow.Cells.Add(headCell);

            table.Rows.Add(headRow);

            //
            // Create a row for each entry in the collection.
            //

            string[] keys = collection.GetKeys();
            InvariantStringArray.Sort(keys);

            for (int keyIndex = 0; keyIndex < keys.Length; keyIndex++)
            {
                string key = keys[keyIndex];

                TableRow bodyRow = new TableRow();
                bodyRow.CssClass = keyIndex % 2 == 0 ? "even-row" : "odd-row";

                TableCell cell;

                //
                // Create the key column.
                //

                cell = new TableCell();
                cell.Text = HtmlEncode(key);
                cell.CssClass = "key-col";

                bodyRow.Cells.Add(cell);

                //
                // Create the value column.
                //

                cell = new TableCell();
                cell.Text = HtmlEncode(collection[key].ToString());
                cell.CssClass = "value-col";

                bodyRow.Cells.Add(cell);

                table.Rows.Add(bodyRow);
            }

            //
            // Write out the table and close container tags.
            //

            table.RenderControl(writer);

            writer.RenderEndTag(); // </div>
            writer.WriteLine();

            writer.RenderEndTag(); // </div>
            writer.WriteLine();
        }

        private static readonly Regex _reStackTrace = new Regex(@"
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
        private string PageTitle;

        private void MarkupStackTrace(string text, TextWriter writer)
        {
            int anchor = 0;

            foreach (Match match in _reStackTrace.Matches(text))
            {
                HtmlEncode(text.Substring(anchor, match.Index - anchor), writer);
                MarkupStackFrame(text, match, writer);
                anchor = match.Index + match.Length;
            }

            HtmlEncode(text.Substring(anchor), writer);
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
        public string BasePageName
        {
            get { return environment.BasePageName; }
        }

        private int StackFrameSpan(string text, int anchor, string klass, Group group, TextWriter writer)
        {
            return group.Success
                 ? StackFrameSpan(text, anchor, klass, group.Value, group.Index, group.Length, writer)
                 : anchor;
        }

        private int StackFrameSpan(string text, int anchor, string klass, string value, int index, int length, TextWriter writer)
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

        public string Title { get { return _errorEntry.Message; } }
        public string Type { get { return _errorEntry.Type; } }
        public string Message { get { return _errorEntry.Message; } }

    }
}
