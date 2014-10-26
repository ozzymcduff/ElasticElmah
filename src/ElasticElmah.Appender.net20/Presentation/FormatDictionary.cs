using log4net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace ElasticElmah.Appender.Presentation
{
    public class FormatDictionary
    {
		static string[] ToRow (Dictionary<string, string> dictionary)
		{
			var keys = dictionary.Keys;
			var array = new string[keys.Count];
			var i = 0;
			foreach (var key in keys) {
				string value = dictionary [key];
				array [i] = string.Format (@"<tr class=""{0}"">
                    <td class=""key-col"">{1}</td>
                    <td class=""value-col"">{2}</td>
                </tr>",
					(i % 2 == 0 ? "large-even-row" : "large-odd-row"),
					key,
					value);

				i++;
			}
			return array;
		}

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
    </table>",String.Join(Environment.NewLine,ToRow( Map(propDic))));
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

		static string[] ToRows (ICollection keys, IDictionary d)
		{
			var i = 0;
			var array = new string[keys.Count];
			foreach (string k in keys) {
				array [i] = string.Format (@"<tr class=""{0}"">
                    <td class=""key-col"">{1}</td>
                    <td class=""value-col"">{2}</td>
                </tr>", (i % 2 == 0 ? "even-row" : "odd-row"), Encode (k), MapToString (d [k]));

				i++;
			}
			return array;
		}

		static System.Reflection.PropertyInfo WithName (System.Reflection.PropertyInfo[] properties, string name)
		{
			foreach (var item in properties) {
				if (item.Name == name) {
					return item;
				}
			}
			throw new Exception ("Could not find property!");
		}

		static string[] ToLis (IEnumerable enumerable)
		{
			var list = new List<string> ();
			foreach (object item in enumerable) {
				list.Add ("<li>" + MapToString (item) + "</li>");
			}
			return list.ToArray ();
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

            if (that.GetType().IsPrimitive || that.GetType().IsValueType)
            {
                return Encode(that.ToString());
            }

            if (that is IDictionary) 
            {
                var d = that as IDictionary;
                var keys = d.Keys;
				if (keys.Count!=0)
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
    </table>", String.Join(Environment.NewLine, ToRows(keys, d)));
                }
                else 
                {
                    return string.Empty;
                }
            }
            
            if (that is IEnumerable)
            {
				var enumerable = (IEnumerable)that;
                return "<ul>" + string.Join(Environment.NewLine,
					ToLis(enumerable)) + "</ul>";
            }

            if (that.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var type = that.GetType();
                var properties = type.GetProperties();
                var key = WithName(properties,"Key").GetValue(that, null);
				var val = WithName(properties,"Value").GetValue(that, null);
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
