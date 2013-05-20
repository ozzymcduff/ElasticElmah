namespace ElasticElmah.Append.MsTests

open System
open ElasticElmah.Append
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type SerializationTests() = 

    [<TestMethod>]
    member x.Will_serialize_ok () = 
        let mutable data = new log4net.Core.LoggingEventData()
        data.Level <- log4net.Core.Level.Alert
        data.Message <- "Message"
        data.Properties <- new log4net.Util.PropertiesDictionary()
        data.Properties.["prop"] <- "msg"
        data.Domain <- ""
        data.Identity <- ""
        data.ThreadName <- ""
        data.UserName <- ""
        let loggingEvent = new log4net.Core.LoggingEvent(data)
        let serialized = Mappings.serialize(Mappings.mapToLog(loggingEvent))
        let expected = @"{
    ""loggerName"":null,
    ""level"":""ALERT"",
    ""message"":""Message"",
    ""threadName"":"""",
    ""timeStamp"":""0001-01-01T00:00:00.0000000+01:00"",
    ""locationInfo"":{
        ""className"":""?"",
        ""fileName"":""?"",
        ""lineNumber"":""?"",
        ""methodName"":""?""
    },
    ""userName"":"""",
    ""properties"":{""prop"":""msg""},
    ""exceptionString"":"""",
    ""domain"":"""",
    ""identity"":""""
}"
        Assert.AreEqual( expected.Replace(" ","").Replace(Environment.NewLine,""), serialized.Replace(" ","").Replace(Environment.NewLine,""))
