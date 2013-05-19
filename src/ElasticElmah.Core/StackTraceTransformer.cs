using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElasticElmah.Core
{
    public class StackTraceTransformer
    {
        private static readonly Regex _reStackTrace =
            new Regex(
                @"
                ^
                \s*
                \w+ \s+ 
                (?<type> .+ ) \.
                (?<method> .+? ) 
                (?<params> \( (?<params> .*? ) \) )
                ( \s+ 
                \w+ \s+ 
                  (?<file> [a-z] \: .+? ) 
                  \: \w+ \s+ 
                  (?<line> [0-9]+ ) \p{P}? )?
                \s*
                $",
                RegexOptions.IgnoreCase
                | RegexOptions.Multiline
                | RegexOptions.ExplicitCapture
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnorePatternWhitespace
                | RegexOptions.Compiled);
        public class MyClass
        {
            private Match item;
            string text;
            int anchor;
            public MyClass(Match item, string text, int anchor)
            {
                this.item = item;
                this.text = text;
                this.anchor = anchor;
            }


            public string First
            {
                get
                {
                    return text.Substring(anchor, item.Index - anchor);
                }
            }

            public int Index { get { return item.Index; } }

            public GroupCollection Groups { get { return item.Groups; } }

            public int Length { get { return item.Length; } }
        }
        public IEnumerable<MyClass> Match(string text)
        {
            int anchor = 0;
            foreach (Match item in _reStackTrace.Matches(text))
            {
                yield return new MyClass(item, text, anchor);
                anchor = item.Index + item.Length;

            }
        }
    }

}
