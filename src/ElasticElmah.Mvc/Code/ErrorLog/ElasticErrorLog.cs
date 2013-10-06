using System;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Appender;
using log4net.Core;
using System.Threading.Tasks;
using ElasticElmah.Appender.Search;
using ElasticElmah.Appender.Web;
using System.Net;
using log4net;
using System.Reflection;

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

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Task<LogSearchResult> GetErrorsAsync(int pageIndex, int pageSize)
        {
            return appender.GetPagedAsync(pageIndex, pageSize).ContinueWith(res =>
                {
                    if (null!=res.Exception)
                    {
                        _log.Warn(res.Exception);
                        return new LogSearchResult(new LogWithId[0], 0);
                    }
                    else {
                        return res.Result;
                    }
                });
        }
    }
}