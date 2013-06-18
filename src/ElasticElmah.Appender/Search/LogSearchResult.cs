using System.Collections.Generic;

namespace ElasticElmah.Appender.Search
{
    public class LogSearchResult
    {
        public readonly IEnumerable<LogWithId> Hits;
        public readonly int Total;

        public LogSearchResult(IEnumerable<LogWithId> hits, int total)
        {
            Hits = hits;
            Total = total;
        }
    }
}
