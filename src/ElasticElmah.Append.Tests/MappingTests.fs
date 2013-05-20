namespace ElasticElmah.Append.Tests
open ElasticElmah.Append
open FsUnit
open NUnit.Framework
module tests=
    open ElasticElmah.Append.Mappings
    let mutable data = new log4net.Core.LoggingEventData();
    data.Level <- log4net.Core.Level.Error
    data.Message <- "msg"
    let event = new log4net.Core.LoggingEvent(data);

    [<Test>]
    let ``should map to`` () =
        mapToLog(event) |> should equal { 
            loggerName=""; 
            level="ERROR"; 
            message="msg"; 
            threadName=""; 
            timeStamp=""; 
            locationInfo={ className=""; fileName=""; lineNumber=""; methodName=""; }; 
            userName=""; 
            properties= new System.Collections.Generic.Dictionary<string,System.Object>();
            exceptionString=""; 
            domain=""; 
            identity=""; 
        }
