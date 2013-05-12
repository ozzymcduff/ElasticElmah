using System;
using System.Collections.Specialized;
using Nest;
using log4net.Core;
using System.Linq;
using System.Collections.Generic;

namespace ElasticElmah.Appender
{
    public class ElasticConnectionBuilder
    {
        public static Tuple<ConnectionSettings, IDictionary<string, string>> BuildElsticSearchConnection(string connectionString)
        {
            var lookup = connectionString
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split(new[] { '=' }))
                .ToDictionary(v => v[0], v => v[1], StringComparer.InvariantCultureIgnoreCase);
            var server = lookup["Server"];
            var port = Convert.ToInt32(lookup["Port"]);
            var settings = new ConnectionSettings(new Uri("http://" + server + ":" + port));
            settings.SetDefaultIndex(lookup["Index"]);
            return new Tuple<ConnectionSettings,IDictionary<string,string>>(settings,lookup);
        }
    }
}
