using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Search
{
    public class LogSearchResult
    {
        public readonly IEnumerable<LogWithId> Hits;
        public readonly int Total;

        public LogSearchResult(IEnumerable<LogWithId> hits, int total)
        {
            this.Hits = hits;
            this.Total = total;
        }
    }
}
