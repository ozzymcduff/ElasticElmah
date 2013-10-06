using System.Web;
using ElasticElmah.Core.ErrorLog;

namespace ElasticElmahMVC.Code
{
    public class Helper
    {
        private static readonly object _contextKey = new object();


        private HttpContextBase _context;

        public Helper(HttpContextBase context)
        {
            _context = context;
        }

        public static IErrorLog GetDefault(HttpContextBase context)
        {
            return GetDefaultImpl(context);
        }

        internal static IErrorLog GetDefaultImpl(HttpContextBase context)
        {
            IErrorLog log;

            if (context != null)
            {
                log = (IErrorLog) context.Items[_contextKey];

                if (log != null)
                    return log;
            }

            //
            // Determine the default store type from the configuration and 
            // create an instance of it.
            //

            log = null;

            //
            // If no object got created (probably because the right 
            // configuration settings are missing) then default to 
            // the in-memory log implementation.
            //

            if (log == null)
                log = new ElasticErrorLog("Server=localhost;Index=log;Port=9200");
            //log = new MemoryErrorLog();

            if (context != null)
            {
                //
                // Infer the application name from the context if it has not
                // been initialized so far.
                //

                //
                // Save into the context if context is there so retrieval is
                // quick next time.
                //

                context.Items[_contextKey] = log;
            }

            return log;
        }
    }
}