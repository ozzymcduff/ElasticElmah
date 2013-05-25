using System;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Appender;
using log4net.Core;
using System.Threading.Tasks;
using ElasticElmah.Appender.Search;

namespace ElasticElmah.Core.ErrorLog
{
    public class ElasticErrorLog : IErrorLog
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

        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }


        /// <summary>
        /// Returns the specified error from the database, or null 
        /// if it does not exist.
        /// </summary>
        public Task<LogWithId> GetErrorAsync(string id)
        {
            var r = appender.GetAsync(id);
            return r.ContinueWith<LogWithId>(t =>
            {
                return t.Result;
            });
        }

        public Task<LogSearchResult> GetErrorsAsync(int pageIndex, int pageSize)
        {
            var r = appender.GetPagedAsync(pageIndex,pageSize);
            return r.ContinueWith(t=>
            {
                return t.Result;
            });
        }
    }
}