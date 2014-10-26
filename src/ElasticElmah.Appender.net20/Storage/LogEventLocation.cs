namespace ElasticElmah.Appender.Storage
{
    public class LogEventLocation
    {
        public string className { get; set; }
        public string fileName { get; set; }
        public string lineNumber { get; set; }
        public string methodName { get; set; }
    }
}