using log4net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElasticElmah.Appender.Presentation
{
    public class FormatDictionary
    {
        public static string ToTable(PropertiesDictionary propDic)
        {
            return String.Format( @"<table>
        <thead>
            <tr>
                <th class=""name-col"">Name</th>
                <th class=""value-col"">Value</th>
            </tr>
        </thead>
        <tbody>
            {0}
        </tbody>
    </table>",String.Join(Environment.NewLine, Map(propDic).Select((kv,i)=>string.Format(@"<tr class=""{0}"">
                    <td class=""key-col"">{1}</td>
                    <td class=""value-col"">{2}</td>
                </tr>",
                      (i % 2 == 0 ? "large-even-row" : "large-odd-row"),
                      kv.Key,
                      kv.Value))));
        }

        private static Dictionary<string, string> Map(PropertiesDictionary propertiesDictionary)
        {
            var dic = new Dictionary<string, string>();
            foreach (var key in propertiesDictionary.GetKeys())
            {
                dic.Add(key, MapToString(propertiesDictionary[key]));
            }
            return dic;
        }

        private static string MapToString(object that)
        {
            if (ReferenceEquals(that, null))
            {
                return "Null";
            }
            if (that is String)
            {
                return "<pre>" + Encode((string)that) + "</pre>";
            }

            if (that.GetType().IsPrimitive)
            {
                return Encode(that.ToString());
            }

            if (that is IDictionary) 
            {
                var d = that as IDictionary;
                var keys = d.Keys.Cast<string>();
                if (keys.Any())
                {
                    return String.Format(@"<table>
        <thead>
            <tr>
                <th class=""name-col"">Name</th>
                <th class=""value-col"">Value</th>
            </tr>
        </thead>
        <tbody>
            {0}
        </tbody>
    </table>", String.Join(Environment.NewLine, keys.Select((k, i) =>
                        string.Format(@"<tr class=""{0}"">
                    <td class=""key-col"">{1}</td>
                    <td class=""value-col"">{2}</td>
                </tr>", (i % 2 == 0 ? "even-row" : "odd-row"), Encode(k), MapToString(d[k])))));
                }
                else 
                {
                    return string.Empty;
                }
            }
            
            if (that is IEnumerable)
            {
                return "<ul>" + string.Join(Environment.NewLine,
                    ((IEnumerable)that).Cast<Object>().Select(kv => "<li>" + MapToString(kv) + "</li>").ToArray()) + "</ul>";
            }

            if (that.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var type = that.GetType();
                var properties = type.GetProperties();
                var key = properties.Single(p => p.Name == "Key").GetValue(that, null);
                var val = properties.Single(p => p.Name == "Value").GetValue(that, null);
                return string.Format("<b>{0}</b>: {1}", Encode((string)key), MapToString(val));
            }
            throw new NotImplementedException(that.GetType().Name);
        }

        private static string Encode(string val)
        {
            return HttpUtility.HtmlEncode(val);
        }

    }
}
