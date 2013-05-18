using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(string val);
        string Serialize<T>(T obj);
    }
}
