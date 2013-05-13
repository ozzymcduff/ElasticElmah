using System.Xml.Serialization;

namespace ElasticElmah.Core
{
    #region Imports

    

    #endregion

    //
    // See RSS 0.91 specification at http://backend.userland.com/rss091 for
    // explanation of the XML vocabulary represented by the classes in this
    // file.
    //

    [XmlRoot("rss", Namespace = "", IsNullable = false)]
    public class RichSiteSummary
    {
        public Channel channel;
        [XmlAttribute] public string version;
    }

    public class Channel
    {
        public string copyright;
        public string description;
        [XmlElement(DataType = "anyURI")] public string docs;
        public Image image;
        [XmlElement("item")] public Item[] item;
        [XmlElement(DataType = "language")] public string language;
        public string lastBuildDate;
        [XmlElement(DataType = "anyURI")] public string link;
        public string managingEditor;
        public string pubDate;
        public string rating;
        [XmlArrayItem("day", IsNullable = false)] public Day[] skipDays;
        [XmlArrayItem("hour", IsNullable = false)] public int[] skipHours;
        public TextInput textInput;
        public string title;
        public string webMaster;
    }

    public class Image
    {
        public string description;
        public int height;
        [XmlIgnore] public bool heightSpecified;
        [XmlElement(DataType = "anyURI")] public string link;
        public string title;
        [XmlElement(DataType = "anyURI")] public string url;
        public int width;
        [XmlIgnore] public bool widthSpecified;
    }

    public class Item
    {
        public string description;
        [XmlElement(DataType = "anyURI")] public string link;
        public string pubDate;
        public string title;
    }

    public class TextInput
    {
        public string description;
        [XmlElement(DataType = "anyURI")] public string link;
        public string name;
        public string title;
    }

    public enum Day
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,
    }
}