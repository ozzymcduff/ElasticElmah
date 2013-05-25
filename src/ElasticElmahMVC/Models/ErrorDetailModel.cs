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
namespace ElasticElmahMVC.Models
{
    #region Imports



    #endregion

    /// <summary>
    /// Renders an HTML page displaying details about an error from the 
    /// error log.
    /// </summary>
    public class ErrorDetailModel
    {

        private readonly Error _errorEntry;

        private readonly Environment environment;
        private string PageTitle;
        public Dictionary<string, string> Properties { get { return _errorEntry.Properties; } }
        public ErrorDetailModel(Error errorLogEntry, Environment environment)
        {
            this.environment = environment;
            _errorEntry = errorLogEntry;
            PageTitle = string.Format("Error: {0} [{1}]", _errorEntry.Type, _errorEntry.Id);
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

        // Write out the error log time. This will be in the local
        // time zone of the server. Would be a good idea to indicate
        // it here for the user.
        //
        public DateTime Time
        {
            get { return _errorEntry.Time; }
        }

        public string BasePageName
        {
            get { return environment.BasePageName; }
        }

        public string Title
        {
            get { return _errorEntry.Message; }
        }

        public string Type
        {
            get { return _errorEntry.Type; }
        }

        public string Message
        {
            get { return _errorEntry.Message; }
        }

    }
}