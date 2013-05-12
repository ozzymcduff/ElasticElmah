
namespace Elmah
{
    #region Imports

    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using CultureInfo = System.Globalization.CultureInfo;
    using ArrayList = System.Collections.ArrayList;
    using System.IO;
    using ElasticElmahMVC.Code;

    #endregion

    /// <summary>
    /// Renders an HTML page displaying a page of errors from the error log.
    /// </summary>

    public class ErrorLogPage
    {
        Environment env;
        public ErrorLogPage(Environment env, ErrorLog.Errors errors)
        {
            this.env = env;
            this.errors = errors;
        }

        private string PageTitle;
        private ErrorLog.Errors errors;
        public string BasePageName
        {
            get { return env.BasePageName; }
        }
        public ErrorLogPage OnLoad()
        {
            //
            // Get the page index and size parameters within their bounds.
            //


            //
            // Read the error records.
            //


            //
            // Set the title of the page.
            //

            string hostName = Environment.TryGetMachineName(new HttpContextWrapper(HttpContext.Current));
            this.PageTitle = string.Format(
                hostName.Length > 0
                ? "Error log for {0} on {2} (Page #{1})"
                : "Error log for {0} (Page #{1})",
                env.ApplicationName, (errors.pageIndex + 1).ToString("N0"), hostName);
            return this;
        }
        public string RssLink()
        {
            return BasePageName + "/rss";
        }

        public HtmlString Render()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new HtmlTextWriter(new StreamWriter(stream));
                RenderContents(writer);
                writer.Flush();
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return new HtmlString(reader.ReadToEnd());
            }
        }

        protected void RenderContents(HtmlTextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            //
            // Write out the page title and speed bar in the body.
            //

            RenderTitle(writer);

            writer.Write(SpeedBar.Render(
                SpeedBar.RssFeed.Format(BasePageName),
                SpeedBar.RssDigestFeed.Format(BasePageName),
                SpeedBar.Help,
                SpeedBar.About.Format(BasePageName)));

            if (errors.Entries.Count != 0)
            {
                //
                // Write error number range displayed on this page and the
                // total available in the log, followed by stock
                // page sizes.
                //

                writer.RenderBeginTag(HtmlTextWriterTag.P);

                RenderStats(writer);
                RenderStockPageSizes(writer);

                writer.RenderEndTag(); // </p>
                writer.WriteLine();

                //
                // Write out the main table to display the errors.
                //

                RenderErrors(writer);

                //
                // Write out page navigation links.
                //

                RenderPageNavigators(writer);
            }
            else
            {
                //
                // No errors found in the log, so display a corresponding
                // message.
                //

                RenderNoErrors(writer);
            }

            //base.RenderContents(writer);
        }

        private void RenderPageNavigators(HtmlTextWriter writer)
        {
            //
            // If not on the last page then render a link to the next page.
            //

            writer.RenderBeginTag(HtmlTextWriterTag.P);

            int nextPageIndex = errors.pageIndex + 1;
            bool moreErrors = nextPageIndex * errors.pageSize < errors.Total;

            if (moreErrors)
                RenderLinkToPage(writer, HtmlLinkType.Next, "Next errors", nextPageIndex);

            //
            // If not on the first page then render a link to the firs page.
            //

            if (errors.pageIndex > 0 && errors.Total > 0)
            {
                if (moreErrors)
                    writer.Write("; ");

                RenderLinkToPage(writer, HtmlLinkType.Start, "Back to first page", 0);
            }

            writer.RenderEndTag(); // </p>
            writer.WriteLine();
        }

        private void RenderStockPageSizes(HtmlTextWriter writer)
        {
            //
            // Write out a set of stock page size choices. Note that
            // selecting a stock page size re-starts the log 
            // display from the first page to get the right paging.
            //

            writer.Write("Start with ");

            int[] stockSizes = new int[] { 10, 15, 20, 25, 30, 50, 100 };

            for (int stockSizeIndex = 0; stockSizeIndex < stockSizes.Length; stockSizeIndex++)
            {
                int stockSize = stockSizes[stockSizeIndex];

                if (stockSizeIndex > 0)
                    writer.Write(stockSizeIndex + 1 < stockSizes.Length ? ", " : " or ");

                RenderLinkToPage(writer, HtmlLinkType.Start, stockSize.ToString(), 0, stockSize);
            }

            writer.Write(" errors per page.");
        }

        private void RenderStats(HtmlTextWriter writer)
        {
            int firstErrorNumber = errors.pageIndex * errors.pageSize + 1;
            int lastErrorNumber = firstErrorNumber + errors.Entries.Count - 1;
            int totalPages = (int)Math.Ceiling((double)errors.Total / errors.pageSize);

            writer.Write("Errors {0} to {1} of total {2} (page {3} of {4}). ",
                         firstErrorNumber.ToString("N0"),
                         lastErrorNumber.ToString("N0"),
                         errors.Total.ToString("N0"),
                         (errors.pageIndex + 1).ToString("N0"),
                         totalPages.ToString("N0"));
        }

        private void RenderTitle(HtmlTextWriter writer)
        {
            //
            // If the application name matches the APPL_MD_PATH then its
            // of the form /LM/W3SVC/.../<name>. In this case, use only the 
            // <name> part to reduce the noise. The full application name is 
            // still made available through a tooltip.
            //

            string simpleName = env.ApplicationName;

            if (string.Compare(simpleName, HttpContext.Current.Request.ServerVariables["APPL_MD_PATH"],
                true, CultureInfo.InvariantCulture) == 0)
            {
                int lastSlashIndex = simpleName.LastIndexOf('/');

                if (lastSlashIndex > 0)
                    simpleName = simpleName.Substring(lastSlashIndex + 1);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Id, "PageTitle");
            writer.RenderBeginTag(HtmlTextWriterTag.H1);
            writer.Write("Error Log for ");

            writer.AddAttribute(HtmlTextWriterAttribute.Id, "ApplicationName");
            writer.AddAttribute(HtmlTextWriterAttribute.Title, HttpContext.Current.Server.HtmlEncode(env.ApplicationName));
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            HttpContext.Current.Server.HtmlEncode(simpleName, writer);
            if (env.HostName.Length > 0)
            {
                writer.Write(" on ");
                HttpContext.Current.Server.HtmlEncode(env.HostName, writer);
            }
            writer.RenderEndTag(); // </span>

            writer.RenderEndTag(); // </h1>
            writer.WriteLine();
        }

        private void RenderNoErrors(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.P);

            writer.Write("No errors found. ");

            //
            // It is possible that there are no error at the requested 
            // page in the log (especially if it is not the first page).
            // However, if there are error in the log
            //

            if (errors.pageIndex > 0 && errors.Total > 0)
            {
                RenderLinkToPage(writer, HtmlLinkType.Start, "Go to first page", 0);
                writer.Write(". ");
            }

            writer.RenderEndTag();
            writer.WriteLine();
        }

        private void RenderErrors(HtmlTextWriter writer)
        {

            //
            // Create a table to display error information in each row.
            //

            Table table = new Table();
            table.ID = "ErrorLog";
            table.CellSpacing = 0;

            //
            // Create the table row for headings.
            //

            TableRow headRow = new TableRow();

            headRow.Cells.Add(FormatCell(new TableHeaderCell(), "Host", "host-col"));
            //  headRow.Cells.Add(FormatCell(new TableHeaderCell(), "Code", "code-col"));
            headRow.Cells.Add(FormatCell(new TableHeaderCell(), "Type", "type-col"));
            headRow.Cells.Add(FormatCell(new TableHeaderCell(), "Error", "error-col"));
            headRow.Cells.Add(FormatCell(new TableHeaderCell(), "User", "user-col"));
            headRow.Cells.Add(FormatCell(new TableHeaderCell(), "Date", "date-col"));
            headRow.Cells.Add(FormatCell(new TableHeaderCell(), "Time", "time-col"));

            table.Rows.Add(headRow);

            //
            // Generate a table body row for each error.
            //

            for (int errorIndex = 0; errorIndex < errors.Entries.Count; errorIndex++)
            {
                ErrorLogEntry errorEntry = errors.Entries[errorIndex];
                Error error = errorEntry.Error;

                TableRow bodyRow = new TableRow();
                bodyRow.CssClass = errorIndex % 2 == 0 ? "even-row" : "odd-row";

                //
                // Format host and status code cells.
                //

                bodyRow.Cells.Add(FormatCell(new TableCell(), error.HostName, "host-col"));
                //bodyRow.Cells.Add(FormatCell(new TableCell(), error.StatusCode.ToString(), "code-col", Mask.NullString(HttpWorkerRequest.GetStatusDescription(error.StatusCode))));
                bodyRow.Cells.Add(FormatCell(new TableCell(), ErrorDisplay.HumaneExceptionErrorType(error), "type-col", error.Type));

                //
                // Format the message cell, which contains the message 
                // text and a details link pointing to the page where
                // all error details can be viewed.
                //

                TableCell messageCell = new TableCell();
                messageCell.CssClass = "error-col";

                Label messageLabel = new Label();
                messageLabel.Text = HttpContext.Current.Server.HtmlEncode(error.Message);

                HyperLink detailsLink = new HyperLink();
                detailsLink.NavigateUrl = BasePageName + "/detail?id=" + HttpUtility.UrlEncode(errorEntry.Id);
                detailsLink.Text = "Details&hellip;";

                messageCell.Controls.Add(messageLabel);
                messageCell.Controls.Add(new LiteralControl(" "));
                messageCell.Controls.Add(detailsLink);

                bodyRow.Cells.Add(messageCell);

                //
                // Format the user, date and time cells.
                //

                bodyRow.Cells.Add(FormatCell(new TableCell(), error.User, "user-col"));
                bodyRow.Cells.Add(FormatCell(new TableCell(), error.Time.ToShortDateString(), "date-col",
                    error.Time.ToLongDateString()));
                bodyRow.Cells.Add(FormatCell(new TableCell(), error.Time.ToShortTimeString(), "time-col",
                    error.Time.ToLongTimeString()));

                //
                // Finally, add the row to the table.
                //

                table.Rows.Add(bodyRow);
            }

            table.RenderControl(writer);
        }

        private TableCell FormatCell(TableCell cell, string contents, string cssClassName)
        {
            return FormatCell(cell, contents, cssClassName, string.Empty);
        }

        private TableCell FormatCell(TableCell cell, string contents, string cssClassName, string toolTip)
        {
            cell.Wrap = false;
            cell.CssClass = cssClassName;

            if (string.IsNullOrEmpty(contents))
            {
                cell.Text = "&nbsp;";
            }
            else
            {
                string encodedContents = HttpContext.Current.Server.HtmlEncode(contents);

                if (string.IsNullOrEmpty(toolTip))
                {
                    cell.Text = encodedContents;
                }
                else
                {
                    Label label = new Label();
                    label.ToolTip = toolTip;
                    label.Text = encodedContents;
                    cell.Controls.Add(label);
                }
            }

            return cell;
        }

        private void RenderLinkToPage(HtmlTextWriter writer, string type, string text, int pageIndex)
        {
            RenderLinkToPage(writer, type, text, pageIndex, errors.pageSize);
        }

        private void RenderLinkToPage(HtmlTextWriter writer, string type, string text, int pageIndex, int pageSize)
        {
            if (!(pageIndex >= 0)) throw new Exception("pageIndex >= 0");
            if (!(pageSize >= 0)) throw new Exception("pageSize >= 0");

            string href = string.Format("{0}?page={1}&size={2}",
                BasePageName,
                (pageIndex + 1).ToString(CultureInfo.InvariantCulture),
                pageSize.ToString(CultureInfo.InvariantCulture));

            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);

            if (type != null && type.Length > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Rel, type);

            writer.RenderBeginTag(HtmlTextWriterTag.A);
            HttpContext.Current.Server.HtmlEncode(text, writer);
            writer.RenderEndTag();
        }

        public bool FirstPage()
        {
            return errors.pageIndex == 0;
        }
    }
}
