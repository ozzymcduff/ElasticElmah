using System;
using ElasticElmah.Appender;
using System.Threading.Tasks;
using ElasticElmah.Appender.Search;
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
        public LogWithId GetError(string id)
        {
            var r = appender.Get(id);
            return r;
        }

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LogSearchResult GetErrors(int pageIndex, int pageSize)
        {
			try {
				return appender.GetPaged(pageIndex, pageSize);
			} catch (Exception ex) {
				_log.Warn(ex);
				return new LogSearchResult(new LogWithId[0], 0);
			}
        }
    }
}