using log4net.Appender;
using log4net.Core;

namespace ElasticElmah.Appender
{
    public class ElasticSearchAppender : AppenderSkeleton
    {
        private ElasticSearchRepository _repo;
        private bool _init = false;
        public string ConnectionString { get; set; }
        private static readonly object LockObj = new object();
        protected virtual ElasticSearchRepository Repo
        {
            get
            {
                lock (LockObj)
                {
                    if (_repo != null)
                    {
                        _init = true;
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
            CreateIndexIfNeededAndAppend(loggingEvent);
        }

        public void CreateIndexIfNeededAndAppend(LoggingEvent loggingEvent)
        {
            var repo = Repo; 
            if (_init)
            {
                repo.CreateIndexOrRefreshMappings();
            }
            repo.Add(loggingEvent);
        }
    }
}
