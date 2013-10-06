using System;
using System.IO;
using System.Web;
using System.Web.UI;
using ElasticElmah.Core.Infrastructure;

namespace ElasticElmahMVC.Code
{
    #region Imports

    

    #endregion

    public class SpeedBar
    {
        public static readonly ItemTemplate Home = new ItemTemplate("Errors", "List of logged errors", "{0}");

        public static readonly ItemTemplate RssFeed = new ItemTemplate("RSS Feed", "RSS feed of recent errors",
                                                                       "{0}/rss");

        public static readonly ItemTemplate RssDigestFeed = new ItemTemplate("RSS Digest",
                                                                             "RSS feed of errors within recent days",
                                                                             "{0}/rss/digest");

        public static readonly FormattedItem Help = new FormattedItem("Help",
                                                                      "Documentation, discussions, issues and more",
                                                                      "https://github.com/wallymathieu/ElasticElmah");

        public static readonly ItemTemplate About = new ItemTemplate("About", "Information about this version and build",
                                                                     "{0}/about");

        private SpeedBar()
        {
        }

        public static HtmlString Render(params FormattedItem[] items)
        {
            if (items == null || items.Length == 0)
                return new HtmlString("");
            using (var stream = new MemoryStream())
            {
                var writer = new HtmlTextWriter(new StreamWriter(stream));
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "SpeedList");
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                foreach (FormattedItem item in items)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    item.Render(writer);
                    writer.RenderEndTag( /* li */);
                }

                writer.RenderEndTag( /* ul */);
                writer.Flush();
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return new HtmlString(reader.ReadToEnd());
            }
        }

        #region Nested type: FormattedItem

        [Serializable]
        public sealed class FormattedItem : Item
        {
            public FormattedItem(string text, string title, string href) :
                base(text, title, href)
            {
            }

            public void Render(HtmlTextWriter writer)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, Href);
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Title);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                HttpUtility.HtmlEncode(Text, writer);
                writer.RenderEndTag( /* a */);
            }
        }

        #endregion

        #region Nested type: Item

        [Serializable]
        public abstract class Item
        {
            private readonly string _href;
            private readonly string _text;
            private readonly string _title;

            public Item(string text, string title, string href)
            {
                _text = Mask.NullString(text);
                _title = Mask.NullString(title);
                _href = Mask.NullString(href);
            }

            public string Text
            {
                get { return _text; }
            }

            public string Title
            {
                get { return _title; }
            }

            public string Href
            {
                get { return _href; }
            }

            public override string ToString()
            {
                return Text;
            }
        }

        #endregion

        #region Nested type: ItemTemplate

        [Serializable]
        public sealed class ItemTemplate : Item
        {
            public ItemTemplate(string text, string title, string href) :
                base(text, title, href)
            {
            }

            public FormattedItem Format(string url)
            {
                return new FormattedItem(Text, Title, string.Format(Href, url));
            }
        }

        #endregion
    }
}