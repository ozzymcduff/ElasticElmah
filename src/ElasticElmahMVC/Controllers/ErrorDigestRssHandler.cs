
namespace Elmah
{
    #region Imports

    using System;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using ContentSyndication;

    using ArrayList = System.Collections.ArrayList;
    using System.Collections.Generic;
    using System.Web.Mvc;

    #endregion

    /// <summary>
    /// Renders an RSS feed that is a daily digest of the most recently 
    /// recorded errors in the error log. The feed spans at most 15
    /// days on which errors occurred.
    /// </summary>

    internal sealed class ErrorDigestRssHandler : ActionResult
    {
        public const int pageSize = 30;
        public const int maxPageLimit = 30;

        Environment environment;
        private IList<ErrorLogEntry> entries;
        public ErrorDigestRssHandler(Environment environment, IList<ErrorLogEntry> entries)
        {
            this.entries = entries;
            this.environment = environment;
        }

        private void Render(ControllerContext context)
        {
            var Response = context.HttpContext.Response;
            var Request = context.HttpContext.Request;
            Response.ContentType = "application/xml";

            //
            // We'll be emitting RSS vesion 0.91.
            //

            RichSiteSummary rss = new RichSiteSummary();
            rss.version = "0.91";

            //
            // Set up the RSS channel.
            //

            Channel channel = new Channel();
            channel.title = "Daily digest of errors in "
                          + environment.ApplicationName
                          + (environment.HostName.Length > 0 ? " on " + environment.HostName : null);
            channel.description = "Daily digest of application errors";
            channel.language = "en";

            channel.link = environment.BasePageUrl.ToString();

            rss.channel = channel;

            //
            // Build the channel items.
            //

            ArrayList itemList = new ArrayList(pageSize);

            //
            // Start with the first page of errors.
            //


            //
            // Initialize the running state.
            //

            DateTime runningDay = DateTime.MaxValue;
            int runningErrorCount = 0;
            Item item = null;
            StringBuilder sb = new StringBuilder();
            HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb));

            foreach (ErrorLogEntry entry in entries)
            {
                Error error = entry.Error;
                DateTime time = error.Time.ToUniversalTime();
                DateTime day = new DateTime(time.Year, time.Month, time.Day);

                //
                // If we're dealing with a new day then break out to a 
                // new channel item, finishing off the previous one.
                //

                if (day < runningDay)
                {
                    if (runningErrorCount > 0)
                    {
                        RenderEnd(writer);
                        item.description = sb.ToString();
                        itemList.Add(item);
                    }

                    runningDay = day;
                    runningErrorCount = 0;

                    if (itemList.Count == pageSize)
                        break;

                    item = new Item();
                    item.pubDate = time.ToString("r");
                    item.title = string.Format("Digest for {0} ({1})",
                        runningDay.ToString("yyyy-MM-dd"), runningDay.ToLongDateString());

                    sb.Length = 0;
                    RenderStart(writer);
                }

                RenderError(context.HttpContext.Server, writer, entry, environment.BasePageUrl);
                runningErrorCount++;
            }

            if (runningErrorCount > 0)
            {
                RenderEnd(writer);
                item.description = sb.ToString();
                itemList.Add(item);
            }

            channel.item = (Item[])itemList.ToArray(typeof(Item));

            //
            // Stream out the RSS XML.
            //

            Response.Write(XmlText.StripIllegalXmlCharacters(XmlSerializer.Serialize(rss)));
        }

        private static void RenderStart(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);
        }

        private void RenderError(HttpServerUtilityBase Server, HtmlTextWriter writer, ErrorLogEntry entry, Uri baseUrl)
        {
            Error error = entry.Error;
            writer.RenderBeginTag(HtmlTextWriterTag.Li);

            string errorType = ErrorDisplay.HumaneExceptionErrorType(error);

            if (errorType.Length > 0)
            {
                bool abbreviated = errorType.Length < error.Type.Length;

                if (abbreviated)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, error.Type);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                }

                Server.HtmlEncode(errorType, writer);

                if (abbreviated)
                    writer.RenderEndTag(/* span */);

                writer.Write(": ");
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Href, baseUrl + "/detail?id=" + HttpUtility.UrlEncode(entry.Id));
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            Server.HtmlEncode(error.Message, writer);
            writer.RenderEndTag(/* a */);

            writer.RenderEndTag( /* li */);
        }

        private static void RenderEnd(HtmlTextWriter writer)
        {
            writer.RenderEndTag(/* li */);
            writer.Flush();
        }

        public override void ExecuteResult(ControllerContext context)
        {
            Render(context);
        }
    }
}