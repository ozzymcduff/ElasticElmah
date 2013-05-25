using System.IO;
using System.Xml;

namespace ElasticElmah.Core.Infrastructure
{
    #region Imports

    

    #endregion

    /// <summary>
    /// Serializes object to and from XML documents.
    /// </summary>
    public sealed class XmlSerializer
    {
        private XmlSerializer()
        {
        }

        public static string Serialize(object obj)
        {
            var sw = new StringWriter();
            Serialize(obj, sw);
            return sw.GetStringBuilder().ToString();
        }

        public static void Serialize(object obj, TextWriter output)
        {
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.CheckCharacters = false;
            settings.OmitXmlDeclaration = true;
            XmlWriter writer = XmlWriter.Create(output, settings);

            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
                serializer.Serialize(writer, obj);
                writer.Flush();
            }
            finally
            {
                writer.Close();
            }
        }
    }
}