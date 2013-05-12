using Elmah;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElasticElmahMVC.Code
{
    public class Helper
    {
        private static readonly object _contextKey = new object();

        public static ErrorLog GetDefault(HttpContextBase context)
        {
            return GetDefaultImpl(context);
        }

        internal static ErrorLog GetDefaultImpl(HttpContextBase context)
        {
            ErrorLog log;

            if (context != null)
            {
                log = (ErrorLog)context.Items[_contextKey];

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


        private HttpContextBase _context;
        public Helper(HttpContextBase context)
        {
            _context = context;
        }
        

    }
}