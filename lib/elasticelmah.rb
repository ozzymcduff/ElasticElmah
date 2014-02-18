require 'elasticsearch'
require 'log4r'
require 'time'
module ElasticElmah

    class Outputter < Log4r::Outputter
        attr_reader :log
        def initialize(_name, hash={})
            super(_name, hash)
            @index = "logs"

            @index = hash[:index] if hash.has_key?(:index) 
            @closed = false
            @initialized = false
            @client = Elasticsearch::Client.new #log: true
        end

        def time_now
            Time.now.utc
        end
        def time_stamp
            time_now.strftime('%Y-%m-%dT%H:%M:%S.%7N%:z') #"0001-01-01T00:00:00.0 00 000 0+01:00"
        end

        def closed?
            @closed
        end

        def close
            @closed = true
            @level = OFF
            OutputterFactory.create_methods(self)
            Logger.log_internal {"Outputter '#{@name}' closed and set to OFF"}
        end

        def serialize_logevent_for_elastic(l)
                    #    attr_reader :level, :tracer, :data, :name, :fullname
      # [level]    The integer level of the log event. Use LNAMES[level]
      #            to get the actual level name.
      # [tracer]   The execution stack returned by <tt>caller</tt> at the
      #            log event. It is nil if the invoked Logger's trace is false.
      # [data]     The object that was passed into the logging method.
      # [name]     The name of the logger that was invoked.
      # [fullname] The fully qualified name of the logger that was invoked.
            return {
                loggerName:l.name,
                level: Log4r::LNAMES[l.level],
                message: l.data != nil && l.data.respond_to?(:message) ? l.data.message : l.data,
                threadName:"",
                timeStamp:time_stamp,
                locationInfo:{
                    className:"?",
                    fileName:"?",
                    lineNumber:"?",
                    methodName:"?"
                },
                userName:"",
                properties:{},
                exceptionString: l.tracer!=nil ? l.tracer.join("\n") : "",
                domain:"",
                identity:""
            }
        end

        #######
        private
        #######
        def create_index
            @client.indices.create(index: @index, body: {
                        settings: {
                          index: { number_of_shards: 1, number_of_replicas: 0 }
                        },
                        mappings:{
                            "LoggingEvent" => logging_event_mappings
                        }
                    })
            #@client.indices.put_mapping(index: @index, type: 'LoggingEvent', body: logging_event_mappings)
        end
        def logging_event_mappings
            {
      _source: {
        enabled: true,
        compress: false
      },
      _ttl: {
        enabled: true,
        default: "24d"
      },
      _timestamp: {
        enabled: true,
        path: "timeStamp",
        store: true
      },
      properties: {
        timeStamp: {
          type: "date"
        },
        message:{type: "string"},
        exceptionString:{type: "string"},
        domain:{type: "string"},
        identity:{type: "string"},
        userName: {type: "string"},
        locationInfo:{
            type: "object",
            properties: {
                className:{type: "string"},
                fileName:{type: "string"},
                lineNumber:{type: "string"},
                methodName:{type: "string"}
            }
        },
        threadName:{type: "string"},
        loggerName:{type: "string"},
        level:{type:"string"},
        properties:{ type: "object", store: "yes" }
      }
    }
        end

        # perform the write
        def canonical_log(logevent)
            if !@initialized
                @initialized= true
                if !@client.indices.exists(index: @index)
                    create_index
                end
            end
            @client.index  index: @index, type: 'LoggingEvent', body: serialize_logevent_for_elastic(logevent)
        end
    end
end
