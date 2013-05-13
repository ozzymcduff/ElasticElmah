using log4net.Appender;
using log4net.Core;

namespace ElasticElmah.Appender
{
    public class ElasticSearchAppender : AppenderSkeleton
    {
        private ElasticSearchRepository _repo;
        public string ConnectionString { get; set; }
        private static readonly object _lockObj = new object();
        protected virtual ElasticSearchRepository Repo
        {
            get
            {
                lock (_lockObj)
                {
                    if (_repo != null)
                    {
                        return _repo;
                    }
                    _repo = new ElasticSearchRepository(ConnectionString);
                    return _repo;
                }
            }
        }
        /// <summary>
        /// Add a log event to the ElasticSearch Repo
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            _repo.Add(loggingEvent);
        }
    }
}
