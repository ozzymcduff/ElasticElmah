
namespace Elmah
{
    #region Imports

    using System;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Assembly = System.Reflection.Assembly;
    using HttpUtility = System.Web.HttpUtility;
    using Cache = System.Web.Caching.Cache;
    using CacheItemPriority = System.Web.Caching.CacheItemPriority;
    using HttpRuntime = System.Web.HttpRuntime;
    using System.Web.Mvc;
    using System.Text;

    #endregion

    /// <summary>
    /// Displays a "Powered-by ELMAH" message that also contains the assembly
    /// file version informatin and copyright notice.
    /// </summary>

    public class PoweredBy 
    {
        private AboutSet _about;

        /// <summary>
        /// Renders the contents of the control into the specified writer.
        /// </summary>

        public String RenderContents()
        {
            //
            // Write out the assembly title, version number, copyright and
            // license.
            //

            AboutSet about = this.About;
            var v = new StringBuilder();
            v.Append("Powered by <a href=\"https://github.com/wallymathieu/ElasticElmah\">");
            v.Append(HttpUtility.HtmlEncode(Mask.EmptyString(about.Product, "(product)")));
            v.Append("</a>");
            v.Append(", version ");

            string version = about.GetFileVersionString();
            
            if (version.Length == 0)
                version = about.GetVersionString();

            v.Append(HttpUtility.HtmlEncode(Mask.EmptyString(version, "?.?.?.?")));

#if DEBUG
            v.Append(" (" + Build.Configuration + ")");
#endif

            v.Append(". ");
            
            string copyright = about.Copyright;
            
            if (copyright.Length > 0)
            {
                v.Append(HttpUtility.HtmlEncode(copyright));
                v.Append(' ');
            }

            v.Append("Licensed under ");
            v.Append("<a href=\"http://www.apache.org/licenses/LICENSE-2.0\">");
            v.Append("Apache License, Version 2.0");
            v.Append("</a>");
            v.Append(". ");
            return v.ToString();
        }

        private AboutSet About
        {
            get
            {
                string cacheKey = GetType().FullName;

                //
                // If cache is available then check if the version 
                // information is already residing in there.
                //

                if (this.Cache != null)
                    _about = (AboutSet) this.Cache[cacheKey];

                //
                // Not found in the cache? Go out and get the version 
                // information of the assembly housing this component.
                //
                
                if (_about == null)
                {
                    //
                    // NOTE: The assembly information is picked up from the 
                    // applied attributes rather that the more convenient
                    // FileVersionInfo because the latter required elevated
                    // permissions and may throw a security exception if
                    // called from a partially trusted environment, such as
                    // the medium trust level in ASP.NET.
                    //
                    
                    AboutSet about = new AboutSet();                    
                    Assembly assembly = this.GetType().Assembly;
                    about.Version = assembly.GetName().Version;
                    
                    AssemblyFileVersionAttribute version = (AssemblyFileVersionAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute));
                    
                    if (version != null)
                        about.FileVersion = new Version(version.Version);

                    AssemblyProductAttribute product = (AssemblyProductAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
                    
                    if (product != null)
                        about.Product = product.Product;

                    AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));
                    
                    if (copyright != null)
                        about.Copyright = copyright.Copyright;
                    
                    //
                    // Cache for next time if the cache is available.
                    //

                    if (this.Cache != null)
                    {
                        this.Cache.Add(cacheKey, about,
                            /* absoluteExpiration */ null, Cache.NoAbsoluteExpiration,
                            TimeSpan.FromMinutes(2), CacheItemPriority.Normal, null);
                    }
                    
                    _about = about;
                }

                return _about;
            }
        }

        private Cache Cache
        {
            get
            {
                //
                // Get the cache from the container page, or failing that, 
                // from the runtime. The Page property can be null
                // if the control has not been added to a page's controls
                // hierarchy.
                //

                return HttpRuntime.Cache;
            }
        }

        [ Serializable ]
        private sealed class AboutSet
        {
            private string _product;
            private Version _version;
            private Version _fileVersion;
            private string _copyright;

            public string Product
            {
                get { return Mask.NullString(_product); }
                set { _product = value; }
            }

            public Version Version
            {
                get { return _version; }
                set { _version = value; }
            }

            public string GetVersionString()
            {
                return _version != null ? _version.ToString() : string.Empty;
            }

            public Version FileVersion
            {
                get { return _fileVersion; }
                set { _fileVersion = value; }
            }

            public string GetFileVersionString()
            {
                return _fileVersion != null ? _fileVersion.ToString() : string.Empty;
            }

            public string Copyright
            {
                get { return Mask.NullString(_copyright); }
                set { _copyright = value; }
            }
        }
    }
}
