using System;
using System.Linq;
using ElasticElmah.Core.Infrastructure;
using log4net.Core;
using log4net.Util;
using System.Collections.Generic;
using System.Collections;

namespace ElasticElmah.Core.ErrorLog
{
    /// <summary>
    /// Represents a logical application error (as opposed to the actual 
    /// exception it may be representing).
    /// </summary>
    [Serializable]
    public sealed class Error
    {
        private readonly LoggingEventData _data;
        public LoggingEventData Data { get { return _data; } }
        public Error(LoggingEventData l, string id)
        {
            _data = l;
            Id = id;
        }

        public string Id { get; private set; }

        public Dictionary<string, string> Properties
        {
            get { return Map(_data.Properties); }
        }

        private Dictionary<string, string> Map(PropertiesDictionary propertiesDictionary)
        {
            var dic = new Dictionary<string, string>();
            foreach (var key in propertiesDictionary.GetKeys())
            {
                dic.Add(key, MapToString(propertiesDictionary[key]));
            }
            return dic;
        }

        private string MapToString(object p)
        {
            if (p == null) 
            {
                return string.Empty;
            }
            if (p is string) 
            {
                return (string)p;
            }
            var t = typeof(KeyValuePair<,>);
            if (p.GetType().GetGenericTypeDefinition()==t) 
            {
                return p.ToString();
            }
            if (p is IEnumerable) 
            {
                return String.Join(", ", ((IEnumerable)p).Cast<Object>().Select(o => MapToString(o)));
            }
            throw new NotImplementedException(p.GetType().Name);
        }


        /// <summary>
        /// Gets or sets name of host machine where this error occurred.
        /// </summary>
        public string HostName
        {
            get { return _data.Domain; }
        }

        /// <summary>
        /// Gets or sets the type, class or category of the error.
        /// </summary>
        public string Type
        {
            get { return _data.LocationInfo != null ? _data.LocationInfo.ClassName : string.Empty; }
        }

        /// <summary>
        /// Gets or sets the source that is the cause of the error.
        /// </summary>
        public string Source
        {
            get { return _data.LoggerName ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets a brief text describing the error.
        /// </summary>
        public string Message
        {
            get { return Mask.NullString(_data.Message)
                .Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries)
                .First(); }
        }

        /// <summary>
        /// Gets or sets a detailed text describing the error, such as a
        /// stack trace.
        /// </summary>
        public string Detail
        {
            get 
            {
                return string.IsNullOrEmpty(_data.ExceptionString)
                    ?_data.Message
                    :_data.ExceptionString; 
            }
        }

        /// <summary>
        /// Gets or sets the user logged into the application at the time 
        /// of the error.
        /// </summary>
        public string User
        {
            get { return Mask.NullString(_data.UserName); }
        }

        /// <summary>
        /// Gets or sets the date and time (in local time) at which the 
        /// error occurred.
        /// </summary>
        public DateTime Time
        {
            get { return _data.TimeStamp; }
        }


        /// <summary>
        /// Returns the value of the <see cref="Message"/> property.
        /// </summary>
        public override string ToString()
        {
            return Message;
        }
    }
}