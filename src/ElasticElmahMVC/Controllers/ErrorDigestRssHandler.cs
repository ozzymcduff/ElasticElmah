using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using ElasticElmah.Core;
using ElasticElmah.Core.ErrorLog;
using ElasticElmah.Core.Infrastructure;
using ElasticElmahMVC.Code;
using Environment = ElasticElmahMVC.Code.Environment;
using ElasticElmah.Appender.Search;

namespace ElasticElmahMVC.Controllers
{
    #region Imports

    

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

        private readonly IList<LogWithId> entries;
        private readonly Environment environment;

        public ErrorDigestRssHandler(Environment environment, IList<LogWithId> entries)
        {
            this.entries = entries;
            this.environment = environment;
        }

        private void Render(ControllerContext context)
        {
            HttpResponseBase Response = context.HttpContext.Response;
            HttpRequestBase Request = context.HttpContext.Request;
            Response.ContentType = "application/xml";

            //
            // We'll be emitting RSS vesion 0.91.
            //

            var rss = new RichSiteSummary();
            rss.version = "0.91";

            //
            // Set up the RSS channel.
            //

            var channel = new Channel();
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

            var itemList = new ArrayList(pageSize);

            //
            // Start with the first page of errors.
            //


            //
            // Initialize the running state.
            //

            DateTime runningDay = DateTime.MaxValue;
            int runningErrorCount = 0;
            Item item = null;
            var sb = new StringBuilder();
            var writer = new HtmlTextWriter(new StringWriter(sb));

            foreach (LogWithId entry in entries)
            {
                var error = entry;
                DateTime time = error.Data.TimeStamp.ToUniversalTime();
                var day = new DateTime(time.Year, time.Month, time.Day);

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

            channel.item = (Item[]) itemList.ToArray(typeof (Item));

            //
            // Stream out the RSS XML.
            //

            Response.Write(XmlText.StripIllegalXmlCharacters(XmlSerializer.Serialize(rss)));
        }

        private static void RenderStart(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);
        }

        private void RenderError(HttpServerUtilityBase Server, HtmlTextWriter writer, LogWithId entry, Uri baseUrl)
        {
            var error = entry;
            writer.RenderBeginTag(HtmlTextWriterTag.Li);

            string errorType = ErrorDisplay.HumaneExceptionErrorType(error);

            if (errorType.Length > 0)
            {
                var type = (error.Data.LocationInfo != null ? error.Data.LocationInfo.ClassName : string.Empty);
                bool abbreviated = errorType.Length < type.Length;

                if (abbreviated)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, type);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                }

                Server.HtmlEncode(errorType, writer);

                if (abbreviated)
                    writer.RenderEndTag( /* span */);

                writer.Write(": ");
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Href, baseUrl + "/detail?id=" + HttpUtility.UrlEncode(entry.Id));
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            Server.HtmlEncode(error.Data.Message, writer);
            writer.RenderEndTag( /* a */);

            writer.RenderEndTag( /* li */);
        }

        private static void RenderEnd(HtmlTextWriter writer)
        {
            writer.RenderEndTag( /* li */);
            writer.Flush();
        }

        public override void ExecuteResult(ControllerContext context)
        {
            Render(context);
        }
    }
}