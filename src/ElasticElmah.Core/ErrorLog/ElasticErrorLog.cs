using System;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Appender;
using log4net.Core;
using System.Threading.Tasks;

namespace ElasticElmah.Core.ErrorLog
{
    public class ElasticErrorLog : ErrorLog
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


        /// <summary>
        /// Returns the specified error from the database, or null 
        /// if it does not exist.
        /// </summary>
        public override Task<Error> GetErrorAsync(string id)
        {
            var r = appender.GetAsync(id);
            return r.ContinueWith<Error>(t => {
                return new Error(t.Result.Data, t.Result.Id);
            });
        }

        public override Task<ErrorLog.Errors> GetErrorsAsync(int pageIndex, int pageSize)
        {
            var r = appender.GetPagedAsync(pageIndex,pageSize);
            return r.ContinueWith(t=>
            {
                var res = t.Result;
                return new Errors() {
                    Total = res.Total,
                    Entries = res.Hits.Select(e => new ElasticElmah.Core.ErrorLog.Error(e.Data, e.Id)).ToList(),
                    pageIndex = pageIndex,
                    pageSize = pageSize
                };
            });
        }
    }
}