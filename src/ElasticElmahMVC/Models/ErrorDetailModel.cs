using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using ElasticElmah.Core.ErrorLog;
using ElasticElmah.Core.Infrastructure;
using log4net.Util;
using Environment = ElasticElmahMVC.Code.Environment;
using log4net.Core;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Core;
using ElasticElmah.Appender.Presentation;
using ElasticElmah.Appender.Search;
namespace ElasticElmahMVC.Models
{
    /// <summary>
    /// Renders an HTML page displaying details about an error from the 
    /// error log.
    /// </summary>
    public class ErrorDetailModel
    {

        private readonly LogWithId _errorEntry;

        private readonly Environment environment;
        private string PageTitle;
 //       public string Properties { get { return _errorEntry.Properties; } }
        public ErrorDetailModel(LogWithId errorLogEntry, Environment environment)
        {
            this.environment = environment;
            _errorEntry = errorLogEntry;
            PageTitle = string.Format("Error: {0} [{1}]", Type, _errorEntry.Id);
        }


        //
        // Do we have details, like the stack trace? If so, then write 
        // them out in a pre-formatted (pre) element. 
        // NOTE: There is an assumption here that detail will always
        // contain a stack trace. If it doesn't then pre-formatting 
        // might not be the right thing to do here.
        //
        public string Detail
        {
            get
            {
                return string.IsNullOrEmpty(_errorEntry.Data.ExceptionString)
                    ? _errorEntry.Data.Message
                    : new ColorizeStackTrace(_errorEntry.Data.ExceptionString).Html();
            }
        }

        public string Properties
        {
            get { return FormatDictionary.ToTable(_errorEntry.Data.Properties); }
        }

        /// <summary>
        /// Gets or sets name of host machine where this error occurred.
        /// </summary>
        public string HostName
        {
            get { return _errorEntry.Data.Domain; }
        }

        /// <summary>
        /// Gets or sets the type, class or category of the error.
        /// </summary>
        public string Type
        {
            get { return _errorEntry.Data.LocationInfo != null ? _errorEntry.Data.LocationInfo.ClassName : string.Empty; }
        }

        /// <summary>
        /// Gets or sets the source that is the cause of the error.
        /// </summary>
        public string Source
        {
            get { return _errorEntry.Data.LoggerName ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets a brief text describing the error.
        /// </summary>
        public string Message
        {
            get
            {
                return Mask.NullString(_errorEntry.Data.Message)
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .First();
            }
        }

        /// <summary>
        /// Gets or sets the user logged into the application at the time 
        /// of the error.
        /// </summary>
        public string User
        {
            get { return Mask.NullString(_errorEntry.Data.UserName); }
        }

        /// <summary>
        /// Gets or sets the date and time (in local time) at which the 
        /// error occurred.
        /// </summary>
        public DateTime Time
        {
            get { return _errorEntry.Data.TimeStamp; }
        }

        public string BasePageName
        {
            get { return environment.BasePageName; }
        }

        public string Title
        {
            get { return _errorEntry.Data.Message; }
        }
    }
}