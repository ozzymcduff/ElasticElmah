[![Build Status](https://travis-ci.org/wallymathieu/ElasticElmah.png?branch=master)](https://travis-ci.org/wallymathieu/ElasticElmah) 
ElasticElmah
============

Fork of ELMAH
-------------
https://code.google.com/p/elmah/


Usage
-----
Register it as an appender in log4net.

Parameters:
async: The default is that it uses synchronous requests when logging. There might be issues when using asynchronous requests.
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
