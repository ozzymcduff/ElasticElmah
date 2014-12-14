ElasticElmah [![Build Status](https://travis-ci.org/wallymathieu/ElasticElmah.png?branch=master)](https://travis-ci.org/wallymathieu/ElasticElmah) [![Build status](https://ci.appveyor.com/api/projects/status/v7fcwree00bpgrt8?svg=true)](https://ci.appveyor.com/project/wallymathieu/elasticelmah)
============

Originally a fork of ELMAH
--------------------------
https://code.google.com/p/elmah/

Log4net elasticsearch appender
------------------------------
Now it's a log4net appender to elastic that knows knows how to log web context and can format some stack traces.

Usage
-----
Register it as an appender in log4net.

Parameters:
connectionstring: Right now a connection string looking like a database connection string.

    <appender name="ElasticSearchAppender" type="ElasticElmah.Appender.ElasticSearchAppender, ElasticElmah.Appender">
      <connectionString value="Server=localhost;Index=log;Port=9200"/>
    </appender>

In order to read logs, you can use the class ElasticSearchRepository. There is a test site based on Elmah (ElasticElmahMVC).
The reason for an asp.net MVC site is that it enables you to integrate logging information with your admin site

What to copy from this lib?
---------------------------

Elmah has a dump of the web context. Implemented against log4net here [./src/ElasticElmah.Appender/ElasticSearchWebAppender.cs](./src/ElasticElmah.Appender/ElasticSearchWebAppender.cs)

Alternatives
------------
- [serilog](https://github.com/serilog/serilog)
- [log4net.ElasticSearch](https://github.com/jptoto/log4net.ElasticSearch)
- [Nest to output to kibana format](https://github.com/elasticsearch/elasticsearch-net)

License
-------
Same as ELMAH

http://www.apache.org/licenses/LICENSE-2.0
