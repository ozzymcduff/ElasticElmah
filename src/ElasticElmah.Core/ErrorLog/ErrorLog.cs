using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasticElmah.Core.ErrorLog
{
    public abstract class ErrorLog
    {
        /// <summary>
        /// Get the name of this log.
        /// </summary>
        public virtual string Name
        {
            get { return GetType().Name; }
        }

        public class Errors
        {
            public IList<Error> Entries;
            public int Total;
            public int pageIndex;
            public int pageSize;
        }
        /// <summary>
        /// Retrieves a single application error from log given its 
        /// identifier, or null if it does not exist.
        /// </summary>
        public abstract Task<Error> GetErrorAsync(string id);
        /// <summary>
        /// Retrieves a page of application errors from the log in 
        /// descending order of logged time.
        /// </summary>
        public abstract Task<Errors> GetErrorsAsync(int pageIndex, int pageSize);
    }
}