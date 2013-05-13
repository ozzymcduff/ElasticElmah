using System.Collections.Generic;

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

        /// <summary>
        /// Retrieves a single application error from log given its 
        /// identifier, or null if it does not exist.
        /// </summary>
        public abstract Error GetError(string id);

        public abstract Errors GetErrors(int pageIndex, int pageSize);

        #region Nested type: Errors

        /// <summary>
        /// Retrieves a page of application errors from the log in 
        /// descending order of logged time.
        /// </summary>
        public class Errors
        {
            public IList<Error> Entries;
            public int Total;
            public int pageIndex;
            public int pageSize;
        }

        #endregion
    }
}