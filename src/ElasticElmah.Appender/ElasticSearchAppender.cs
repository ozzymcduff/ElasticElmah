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
#if ASYNC
                ObserveExceptions(AppendAsync(loggingEvent));
#else
				throw new System.NotImplementedException("Compiled without ASYNC");
#endif  
			}
            else
            {
                AppendSync(loggingEvent);
            }
        }

        public void AppendSync(LoggingEvent loggingEvent)
        {
            var repo = Repo; 
            if (init)
            {
                repo.CreateIndexOrRefreshMappings();
            }
            repo.Add(loggingEvent);
        }
#if ASYNC
        public Task AppendAsync(LoggingEvent loggingEvent)
        {
            if (!init)
            {
                return ObserveExceptions(Repo.AddAsync(loggingEvent));
            }
            else
            {
                return ObserveExceptions(Repo.CreateIndexOrRefreshMappingsAsync()
                    .ContinueWith(t =>
                    {
                        return Repo.AddAsync(loggingEvent);
                    }));
            }
        }

        private Task ObserveExceptions(Task t) 
        {
            return t.ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    ErrorHandler.Error("Unhandled", task.Exception);
                }
            });
        }
#endif
    }
}
