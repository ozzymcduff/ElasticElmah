using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NET20
namespace ElasticElmah.Appender.net20.Tests
#else
namespace ElasticElmah.Appender.Tests
#endif
{
    class Global
    {
        public static int Port = 9200;
        public static bool UseFiddler = false;
    }
}
