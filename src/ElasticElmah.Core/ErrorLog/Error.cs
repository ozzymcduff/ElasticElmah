
using System;
using LogViewer;
using log4net.Util;
namespace Elmah
{

    /// <summary>
    /// Represents a logical application error (as opposed to the actual 
    /// exception it may be representing).
    /// </summary>

    [Serializable]
    public sealed class Error 
    {
        public PropertiesDictionary Properties { get { return _data.Data.Properties; } }
        private readonly LogEntry _data;
        
        public Error() { }

        public Error(log4net.Core.LoggingEventData l)
        {
            this._data = new LogEntry { Data = l };
        }


        /// <summary>
        /// Gets or sets name of host machine where this error occurred.
        /// </summary>

        public string HostName
        {
            get { return this._data.Data.Domain; }
        }

        /// <summary>
        /// Gets or sets the type, class or category of the error.
        /// </summary>

        public string Type
        {
            get { return this._data.Data.LocationInfo != null ? this._data.Data.LocationInfo.ClassName : string.Empty; }
        }

        /// <summary>
        /// Gets or sets the source that is the cause of the error.
        /// </summary>

        public string Source
        {
            get { return this._data.Data.LoggerName != null ? this._data.Data.LoggerName : string.Empty; }
        }

        /// <summary>
        /// Gets or sets a brief text describing the error.
        /// </summary>

        public string Message
        {
            get { return Mask.NullString(this._data.Data.Message); }
        }

        /// <summary>
        /// Gets or sets a detailed text describing the error, such as a
        /// stack trace.
        /// </summary>

        public string Detail
        {
            get { return Mask.NullString(this._data.Data.ExceptionString); }
        }

        /// <summary>
        /// Gets or sets the user logged into the application at the time 
        /// of the error.
        /// </summary>

        public string User
        {
            get { return Mask.NullString(this._data.Data.UserName); }
        }

        /// <summary>
        /// Gets or sets the date and time (in local time) at which the 
        /// error occurred.
        /// </summary>

        public DateTime Time
        {
            get { return this._data.Data.TimeStamp; }
        }


        /// <summary>
        /// Returns the value of the <see cref="Message"/> property.
        /// </summary>

        public override string ToString()
        {
            return this.Message;
        }

    }
}
