using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Search
{
    public class LogSearchHistogramResult
    {

        public HistogramEntry[] Histogram { get; set; }
    }
    public class HistogramEntry
    {
        public DateTime Time { get; set; }
        public int Count { get; set; }
    }
}
