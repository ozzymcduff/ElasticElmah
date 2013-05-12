
using ElasticElmah.Appender;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elmah
{


    public class ElasticErrorLog : ErrorLog
    {
        private readonly string _connectionString;

        public ElasticErrorLog(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("connectionString");

            _connectionString = connectionString;
            appender = new ElasticSearchAppender() { ConnectionString = _connectionString };
        }

        private static readonly object _lock = new object();
        private ElasticSearchAppender appender;

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
            var res = appender.GetPaged(pageIndex, pageSize);
            return new Errors
            {
                Total = res.Item2,
                Entries = res.Item1.Select(e => Map(e)).ToList(),
                pageIndex = pageIndex,
                pageSize = pageSize
            };
        }

        private ErrorLogEntry Map(Tuple<string, log4net.Core.LoggingEventData> e)
        {
            return new ErrorLogEntry(this, e.Item1, Map(e.Item2));
        }

        private Error Map(log4net.Core.LoggingEventData l)
        {
            return new Error(l);
        }

        /// <summary>
        /// Returns the specified error from the database, or null 
        /// if it does not exist.
        /// </summary>

        public override ErrorLogEntry GetError(string id)
        {
            return Map(appender.Get(id));
        }
    }
}