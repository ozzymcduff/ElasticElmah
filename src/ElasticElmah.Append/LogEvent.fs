namespace ElasticElmah.Append
open System
open System.Collections.Generic
type LogEventLocation=
    { className :string;
      fileName :string;
      lineNumber :string;
      methodName :string;
    }
    
type LoggingEvent=
    {
        loggerName :string;
        level :string;
        message :string;
        threadName :string;
        timeStamp :string;
        locationInfo :LogEventLocation;
        userName :string;
        properties :Dictionary<string,Object>;
        exceptionString :string;
        domain :string;
        identity :string;
    }
