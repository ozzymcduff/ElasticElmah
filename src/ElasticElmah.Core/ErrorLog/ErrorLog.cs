
using System.Collections;
using System.Collections.Generic;


namespace Elmah
{

    public abstract class ErrorLog
    {

        /// <summary>
        /// Retrieves a single application error from log given its 
        /// identifier, or null if it does not exist.
        /// </summary>

        public abstract Error GetError(string id);

        /// <summary>
        /// Retrieves a page of application errors from the log in 
        /// descending order of logged time.
        /// </summary>

        
        public class Errors
        {
            public int pageIndex; public int pageSize;
            public int Total;
            public IList<Error> Entries;
        }

        public abstract Errors GetErrors(int pageIndex, int pageSize);
        

        /// <summary>
        /// Get the name of this log.
        /// </summary>

        public virtual string Name
        {
            get { return this.GetType().Name; }
        }



    }
}
