namespace ElasticElmah.Append
open log4net;
open log4net.Core;
open log4net.Util;
open System.Reflection;

   module Mappings=
        //type PropertyDictionary=log4net.Util.PropertyDictionary;
        let _log =LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)
        let tap<'typeparam>(o:'typeparam,a) : 'typeparam=
            a(o)
            o

        let mapPropTo (p : log4net.Util.PropertiesDictionary) =
            Map.ofSeq(p.GetKeys() |> Array.map (fun key ->(key, p.Item(key))) )
        
        let mapMapTo (m : Map<string,System.Object>) =
            //Map.ofSeq(p.GetKeys() |> Array.map (fun key ->(key, p.Item(key))) )
            let mutable p = new log4net.Util.PropertiesDictionary()
            for kv in m do
                p.[kv.Key] <- kv.Value
            p

        let inline isNull< ^a when ^a : not struct> (x:^a) =
            obj.ReferenceEquals (x, Unchecked.defaultof<_>)

        let mapTo (l) = 
            let mutable d = new LoggingEventData()
            d.LoggerName <- l.loggerName
            d.Level <- _log.Logger.Repository.LevelMap.[l.level]
            d.Message <- l.message
            d.ThreadName <- l.threadName
            d.TimeStamp <- l.timeStamp
            d.UserName <- l.userName
            d.ExceptionString <- l.exceptionString
            d.Domain <- l.domain
            d.Identity <- l.identity
            if not (isNull(l.properties)) then
                d.Properties <- mapMapTo(l.properties)

            if not (isNull(l.locationInfo)) then
                let i = l.locationInfo
                d.LocationInfo <- new LocationInfo(i.className, i.methodName, i.fileName, i.lineNumber)
            d

        //type LoggingEvent = ElasticElmah.Append.LoggingEvent
        let empty = System.String.Empty

        let mapToLog (l : log4net.Core.LoggingEvent): ElasticElmah.Append.LoggingEvent =
            let i = l.LocationInformation
            {
                loggerName = l.LoggerName;
                level = if not(isNull( l.Level)) then l.Level.Name else empty;
                message = l.RenderedMessage;
                threadName = l.ThreadName;
                timeStamp = l.TimeStamp;
                userName = l.UserName;
                exceptionString = l.GetExceptionString();
                domain = l.Domain;
                identity = l.Identity;
                locationInfo = 
                    if not(isNull(i)) 
                    then { className=i.ClassName; fileName=i.FileName; lineNumber=i.LineNumber; methodName=i.MethodName } 
                    else { className=empty; fileName=empty; lineNumber=empty; methodName=empty};
                properties = mapPropTo(l.Properties)
            }
