
namespace Elmah
{
    #region Imports

    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    #endregion

    /// <summary>
    /// Renders an HTML page that presents information about the version,
    /// build configuration, source files as well as a method to check
    /// for updates.
    /// </summary>

    public class AboutModel 
    {
        public string PageTitle { get; set; }
        private Environment environment;
        public AboutModel(Environment environment)
        {
            this.environment = environment;
            PageTitle = "About ElasticELMAH";
        }

        public string BasePageName
        {
            get { return environment.BasePageName; }
        }

        public Version GetVersion() 
        {
            return GetType().Assembly.GetName().Version;
        }

        public Version GetFileVersion()
        {
            AssemblyFileVersionAttribute version = (AssemblyFileVersionAttribute) Attribute.GetCustomAttribute(GetType().Assembly, typeof(AssemblyFileVersionAttribute));
            return version != null ? new Version(version.Version) : new Version();
        }
    }
}
