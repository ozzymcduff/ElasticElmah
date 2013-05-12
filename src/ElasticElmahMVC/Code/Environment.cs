
namespace Elmah
{
    #region Imports

    using System.Security;
    using System.Web;

    #endregion

    public class Environment
    {
        private HttpContextBase context;

        public Environment(HttpContextBase context)
        {
            this.context = context;
        }

        private string _hostname;
        public string HostName
        {
            get
            {
                if (_hostname == null)
                {
                    _hostname=TryGetMachineName(context);
                }
                return _hostname;
            }
        }

        public static string TryGetMachineName(HttpContextBase context)
        {
            return TryGetMachineName(context, null);
        }

        /// <remarks>
        /// If <paramref name="unknownName"/> is a null reference then this
        /// method will still return an empty string.
        /// </remarks>

        public static string TryGetMachineName(HttpContextBase context, string unknownName)
        {
            //
            // System.Web.HttpServerUtility.MachineName and 
            // System.Environment.MachineName require different permissions.
            // Try the former then the latter...chances are higher to have
            // permissions for the former.
            //

            if (context != null)
            {
                try
                {
                    return context.Server.MachineName;
                }
                catch (HttpException)
                {
                    // Yes, according to docs, HttpServerUtility.MachineName
                    // throws HttpException on failing to obtain computer name.
                }
                catch (SecurityException)
                {
                    // A SecurityException may occur in certain, possibly 
                    // user-modified, Medium trust environments.
                }
            }

            try
            {
                return System.Environment.MachineName;
            }
            catch (SecurityException)
            {
                // A SecurityException may occur in certain, possibly 
                // user-modified, Medium trust environments.
            }

            return Mask.NullString(unknownName);
        }

        private Environment() { }

        public string BasePageName { get { return ToBase(context.Request.Url); } }

        private static string ToBase(System.Uri uri)
        {
            return uri.Scheme + "://" + uri.Host + (uri.Port != 80 || uri.Port != 443 ? ":" + uri.Port : string.Empty);
        }

        public System.Uri BasePageUrl { get { return new System.Uri(ToBase(context.Request.Url)); } }
        private string _appName;

        public string ApplicationName
        {
            get
            {
                if (string.IsNullOrEmpty(_appName))
                {
                    _appName = InferApplicationName(context);
                }
                return Mask.NullString(_appName);
            }
        }

        private string InferApplicationName(HttpContextBase context)
        {
            //
            // Setup the application name (ASP.NET 2.0 or later).
            //

            string appName = null;

            if (context.Request != null)
            {
                //
                // ASP.NET 2.0 returns a different and more cryptic value
                // for HttpRuntime.AppDomainAppId comared to previous 
                // versions. Also HttpRuntime.AppDomainAppId is not available
                // in partial trust environments. However, the APPL_MD_PATH
                // server variable yields the same value as 
                // HttpRuntime.AppDomainAppId did previously so we try to
                // get to it over here for compatibility reasons (otherwise
                // folks upgrading to this version of ELMAH could find their
                // error log empty due to change in application name.
                //

                appName = context.Request.ServerVariables["APPL_MD_PATH"];
            }

            if (string.IsNullOrEmpty(appName))
            {
                //
                // Still no luck? Try HttpRuntime.AppDomainAppVirtualPath,
                // which is available even under partial trust.
                //

                appName = HttpRuntime.AppDomainAppVirtualPath;
            }

            return Mask.EmptyString(appName, "/");

        }

    }
}
