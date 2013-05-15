namespace ElasticElmah.Appender.Storage
open System
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
        timeStamp :DateTime;
        locationInfo :LogEventLocation;
        userName :string;
        properties :Map<string,Object>;
        exceptionString :string;
        domain :string;
        identity :string;
    }
