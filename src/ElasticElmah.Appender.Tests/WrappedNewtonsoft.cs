using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using log4net.Util;

namespace ElasticElmah.Appender
{
    public class WrappedNewtonsoft : IJsonSerializer
    {
        public T Deserialize<T>(string val)
        {
            return JsonConvert.DeserializeObject<T>(val, new JsonSerializerSettings().Tap(t => t.Converters.Add(new DictionaryConverter())));
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public class DictionaryConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var dic = new Dictionary<string, object>();
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            var name = reader.Value;
                            dic.Add(name.ToString(), ExpectDictionaryOrPrimitive(reader));
                            break;
                        case JsonToken.EndObject:
                            return dic;
                        default:
                            throw new Exception(reader.TokenType.ToString());
                    }
                }
                return dic;
            }

            private object ExpectDictionaryOrPrimitive(JsonReader reader)
            {
                var dic = new Dictionary<string, object>();
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.String:
                            return reader.Value.ToString();

                        case JsonToken.StartObject:
                            return ExpectDictionaryOrPrimitive(reader);
                        case JsonToken.PropertyName:
                            dic.Add(reader.Value.ToString(), ExpectDictionaryOrPrimitive(reader));
                            break;
                        case JsonToken.EndObject:
                            return dic;
                        default:
                            throw new Exception("Unrecognized token: " + reader.TokenType.ToString());
                    }
                }
                return dic;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Dictionary<string, object>);
            }
        }
    }

}
