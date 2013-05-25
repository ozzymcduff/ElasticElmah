using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using ElasticElmah.Core;
using ElasticElmah.Core.ErrorLog;
using ElasticElmah.Core.Infrastructure;
using ElasticElmahMVC.Code;
using ElasticElmah.Appender.Search;

namespace ElasticElmahMVC.Controllers
{
    /// <summary>
    /// Renders a XML using the RSS 0.91 vocabulary that displays, at most,
    /// the 15 most recent errors recorded in the error log.
    /// </summary>
    internal sealed class ErrorRssHandler : ActionResult
    {
        public const int pageSize = 15;

        private readonly Environment environment;
        private readonly IList<LogWithId> errorEntryList;

        public ErrorRssHandler(Environment environment, IList<LogWithId> errorEntryList)
        {
            this.errorEntryList = errorEntryList;
            this.environment = environment;
        }

        public void ProcessRequest(HttpContextBase context)
        {
            context.Response.ContentType = "application/xml";

            //
            // Get the last set of errors for this application.
            //


            //
            // We'll be emitting RSS vesion 0.91.
            //

            var rss = new RichSiteSummary();
            rss.version = "0.91";

            //
            // Set up the RSS channel.
            //

            var channel = new Channel();
            channel.title = "Error log of " + environment.ApplicationName
                            + (environment.HostName.Length > 0 ? " on " + environment.HostName : null);
            channel.description = "Log of recent errors";
            channel.language = "en";
            channel.link = environment.BasePageUrl.ToString();

            rss.channel = channel;

            //
            // For each error, build a simple channel item. Only the title, 
            // description, link and pubDate fields are populated.
            //

            channel.item = new Item[errorEntryList.Count];

            for (int index = 0; index < errorEntryList.Count; index++)
            {
                var errorEntry = errorEntryList[index];
                var error = errorEntry;

                var item = new Item();

                item.title = error.Data.Message;
                item.description = "An error of type " + error.Data.LocationInfo != null ? error.Data.LocationInfo.ClassName : string.Empty + " occurred. " + error.Data.Message;
                item.link = channel.link + "/detail?id=" + HttpUtility.UrlEncode(errorEntry.Id);
                item.pubDate = error.Data.TimeStamp.ToUniversalTime().ToString("r");

                channel.item[index] = item;
            }

            //
            // Stream out the RSS XML.
            //

            context.Response.Write(XmlText.StripIllegalXmlCharacters(XmlSerializer.Serialize(rss)));
        }


        public override void ExecuteResult(ControllerContext context)
        {
            ProcessRequest(context.HttpContext);
        }
    }
}