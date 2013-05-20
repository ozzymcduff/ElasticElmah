using log4net.Appender;
using log4net.Core;
using System.Threading.Tasks;

namespace ElasticElmah.Appender
{
    public class ElasticSearchAppender : AppenderSkeleton
    {
        private ElasticSearchRepository _repo;
        private bool init = false;
        public bool Async { get; set; }
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
                        init = true;
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
            if (Async)
            {
                AppendAsync(loggingEvent);
            }
            else
            {
                AppendSync(loggingEvent);
            }
        }

        public void AppendSync(LoggingEvent loggingEvent)
        {
            if (init)
            {
                Repo.CreateIndexIfNotExists();
            }
            Repo.Add(loggingEvent);
        }

        public Task AppendAsync(LoggingEvent loggingEvent)
        {
            if (init)
            {
                return Repo.AddAsync(loggingEvent);
            }
            else
            {
                return Repo.CreateIndexIfNotExistsAsync()
                    .ContinueWith(t =>
                    {
                        return Repo.AddAsync(loggingEvent);
                    });
            }
        }
    }
}
