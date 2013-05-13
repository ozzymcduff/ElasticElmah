using System;
using System.Reflection;
using Environment = ElasticElmahMVC.Code.Environment;

namespace ElasticElmahMVC.Models
{
    #region Imports

    

    #endregion

    /// <summary>
    /// Renders an HTML page that presents information about the version,
    /// build configuration, source files as well as a method to check
    /// for updates.
    /// </summary>
    public class AboutModel
    {
        private readonly Environment environment;

        public AboutModel(Environment environment)
        {
            this.environment = environment;
            PageTitle = "About ElasticELMAH";
        }

        public string PageTitle { get; set; }

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
            var version =
                (AssemblyFileVersionAttribute)
                Attribute.GetCustomAttribute(GetType().Assembly, typeof (AssemblyFileVersionAttribute));
            return version != null ? new Version(version.Version) : new Version();
        }
    }
}