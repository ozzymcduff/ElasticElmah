using System;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Appender;
using Elmah;
using log4net.Core;

namespace ElasticElmah.Core.ErrorLog
{
    public class ElasticErrorLog : Elmah.ErrorLog
    {
        private readonly string _connectionString;

        private readonly ElasticSearchRepository appender;

        public ElasticErrorLog(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("connectionString");

            _connectionString = connectionString;
            appender = new ElasticSearchRepository(_connectionString);
        }

        /// <summary>
        /// Gets the name of this error log implementation.
        /// </summary>
        public override string Name
        {
            get { return "Elastic Error Log"; }
        }


        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }


        public override Errors GetErrors(int pageIndex, int pageSize)
        {
            Tuple<IEnumerable<Tuple<string, LoggingEventData>>, int> res = appender.GetPaged(pageIndex, pageSize);
            return new Errors
                       {
                           Total = res.Item2,
                           Entries = res.Item1.Select(e => Map(e)).ToList(),
                           pageIndex = pageIndex,
                           pageSize = pageSize
                       };
        }

        private Error Map(Tuple<string, LoggingEventData> e)
        {
            return new Error(e.Item2, e.Item1);
        }


        /// <summary>
        /// Returns the specified error from the database, or null 
        /// if it does not exist.
        /// </summary>
        public override Error GetError(string id)
        {
            return Map(appender.Get(id));
        }
    }
}