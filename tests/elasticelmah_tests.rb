$:.unshift File.join(File.dirname(__FILE__),'..','lib')

require 'elasticelmah'
require 'test/unit'
require 'securerandom'
class OutputterWithFakedElastic < ElasticElmah::Outputter
    attr_reader :written
    def initialize(_name, hash={})
        super(_name, hash)
        @written = []
    end

    private
    def canonical_log(logevent)
        @written.push(logevent)
    end
end

class OutputterWithFakedElasticAndTimestamp < OutputterWithFakedElastic
    def initialize(_name, hash={})
        super(_name, hash)
        @written = []
    end
    def time_now
        Time.new(2008,6,21, 12,30,0, "+01:00")
    end
end

class TestElasticElmahOutputterForReal < Test::Unit::TestCase
    def setup
        Log4r::Logger.root.level = Log4r::INFO
        @logger = Log4r::Logger.new 'testout_for_real'
        @index = SecureRandom.uuid
        @outp = ElasticElmah::Outputter.new 'outp_for_real',{index: @index}
        @logger.add('outp_for_real')
        @client = Elasticsearch::Client.new# log: true
    end
    def teardown
        @client.indices.delete(index: @index)
    end

    def test_generated_from_logging_event_data
        @logger.error('some_error')
        @client.indices.flush(index: @index )
        result = @client.search index: @index
        assert_equal 1, result["hits"]["hits"].length
        hits = result["hits"]["hits"]
        assert_equal 'some_error', hits[0]["_source"]["message"]
    end

    def test_generated_from_logging_event_data
        begin
            raise 'some_error'
        rescue Exception => e
            @logger.error(e)
        end
        @client.indices.flush(index: @index )
        result = @client.search index: @index
        assert_equal 1, result["hits"]["hits"].length
        hits = result["hits"]["hits"]
        assert_equal 'some_error', hits[0]["_source"]["message"]
    end
end


class TestElasticElmahOutputter < Test::Unit::TestCase
    def setup
        Log4r::Logger.root.level = Log4r::INFO
        @logger = Log4r::Logger.new 'testoutp'

        @outp = OutputterWithFakedElasticAndTimestamp.new 'outp1'
        @logger.add('outp1')
    end

    def test_generated_from_logging_event_data
        @logger.error('some_error')
        assert_equal([{:loggerName=>"testoutp",
:level=>"ERROR",
:message=>"some_error",
:threadName=>"",
:timeStamp=>"2008-06-21T12:30:00.0000000+01:00",
:locationInfo=>
 {:className=>"?", :fileName=>"?", :lineNumber=>"?", :methodName=>"?"},
:userName=>"",
:properties=>{},
:exceptionString=>"",
:domain=>"",
:identity=>""}],
            @outp.written.map do |written| @outp.serialize_logevent_for_elastic(written) end)
    end
end

class FakeLogger
    attr_reader :name, :fullname
    def initialize(name,fullname)
        @name = name
        @fullname = fullname
    end
end

class TestElasticElmahSerializer < Test::Unit::TestCase
    def setup
        @outp = OutputterWithFakedElasticAndTimestamp.new 'outp2'
    end


    def test_will_serialize_exception_string
        logger = FakeLogger.new('logger_1', 'fullname_of_logger_1')
        tracer = caller
        data = Exception.new("Message")
        l = Log4r::LogEvent.new(Log4r::INFO, logger, tracer, data)
        s =  @outp.serialize_logevent_for_elastic(l)
        assert_equal("Message",s[:message])
        assert_equal(caller.join("\n"),s[:exceptionString])
    end
    
    def test_will_serialize_exception_without_exception_string
        expected = {
            loggerName:"logger_1",
            level:"INFO",
            message:"Message",
            threadName:"",
            timeStamp:"2008-06-21T12:30:00.0000000+01:00",
            locationInfo:{
                className:"?",
                fileName:"?",
                lineNumber:"?",
                methodName:"?"
            },
            userName:"",
            properties:{},
            exceptionString:"",
            domain:"",
            identity:""
        }
        logger = FakeLogger.new('logger_1', 'fullname_of_logger_1')
        tracer = nil
        data = Exception.new("Message")
        l = Log4r::LogEvent.new(Log4r::INFO, logger, tracer, data)
        assert_equal(expected, @outp.serialize_logevent_for_elastic(l))
    end
end
