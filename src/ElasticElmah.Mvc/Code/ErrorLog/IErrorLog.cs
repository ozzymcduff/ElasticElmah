using ElasticElmah.Appender.Search;
using System.Threading.Tasks;

namespace ElasticElmah.Core.ErrorLog
{
    public interface IErrorLog
    {

        /// <summary>
        /// Retrieves a single application error from log given its 
        /// identifier, or null if it does not exist.
        /// </summary>
        LogWithId GetError(string id);
        /// <summary>
        /// Retrieves a page of application errors from the log in 
        /// descending order of logged time.
        /// </summary>
        LogSearchResult GetErrors(int pageIndex, int pageSize);
    }
}