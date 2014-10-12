ElasticElmah [![Build Status](https://travis-ci.org/wallymathieu/ElasticElmah.png?branch=master)](https://travis-ci.org/wallymathieu/ElasticElmah) 
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
The reason for an asp.net MVC site is that this is easier to read.

License
-------
Same as ELMAH

http://www.apache.org/licenses/LICENSE-2.0
