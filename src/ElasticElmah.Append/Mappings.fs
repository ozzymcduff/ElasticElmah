namespace ElasticElmah.Append
open System
open log4net
open log4net.Core
open log4net.Util
open System.Reflection
open System.Web.Script.Serialization
open System.Collections.Generic
   module Mappings=
        //type PropertyDictionary=log4net.Util.PropertyDictionary;
        let private _log =LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)
        let tap<'typeparam>(o:'typeparam,a) : 'typeparam=
            a(o)
            o

        let private mapPropTo (p : log4net.Util.PropertiesDictionary) =
            let dic = new Dictionary<string,Object>()
            p.GetKeys() |> Array.iter (fun key -> dic.Add(key, p.Item(key)))
            dic
        
        let private mapMapTo (m : Dictionary<string,System.Object>) =
            let mutable p = new log4net.Util.PropertiesDictionary()
            for kv in m do
                p.[kv.Key] <- kv.Value
            p

        let inline private isNull< ^a when ^a : not struct> (x:^a) =
            obj.ReferenceEquals (x, Unchecked.defaultof<_>)

        let mapTo (l) = 
            let mutable d = new LoggingEventData()
            d.LoggerName <- l.loggerName
            d.Level <- _log.Logger.Repository.LevelMap.[l.level]
            d.Message <- l.message
            d.ThreadName <- l.threadName
            d.TimeStamp <- DateTime.Parse(l.timeStamp)
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

        let private serializer = new JavaScriptSerializer()

        let serialize(o)=
            serializer.Serialize(o)

        let deSerialize<'T>(s)=
            serializer.Deserialize<'T>(s)

        let mapToLog (l : log4net.Core.LoggingEvent): ElasticElmah.Append.LoggingEvent =
            let empty = System.String.Empty
            let i = l.LocationInformation
            {
                loggerName = l.LoggerName;
                level = if not(isNull( l.Level)) then l.Level.Name else empty;
                message = l.RenderedMessage;
                threadName = l.ThreadName;
                timeStamp = l.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
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
