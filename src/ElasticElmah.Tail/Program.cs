using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NDesk.Options;
using log4net.Core;
using log4net.Layout;
using ElasticElmah.Appender;
using System.Threading.Tasks;

namespace ElasticElmah.Tail
{
    class Program
    {
        static void Main(string[] args)
        {
            var index = new List<string>();
            int? lines = null;
            LayoutSkeleton layout = null;
            var help = false;
            var p = new OptionSet() {
                { "i|index=",   v => { index.Add(v); } },
                { "l|lines=", v => { lines=Int32.Parse(v);}},
                { "h|?|help", v => { help = true;}},
                { "y|layout=",v=> { layout=new PatternLayout(v);}}
            };
            var detectedFiles = args
                .Where(a => !(a.StartsWith("-") || a.StartsWith("/")))
                .Where(a => Uri.IsWellFormedUriString(a, UriKind.RelativeOrAbsolute))
                .Where(a => File.Exists(a));
            // does not seem to work. add tests
            index.AddRange(detectedFiles);

            p.Parse(args);
            if (layout == null)
            {
                layout = new SimpleLayout();
            }
            Action<TextWriter, LoggingEventData> showentry = (writer, l) => layout.Format(writer, new LoggingEvent(l));

            if (help)
            {
                Console.WriteLine(@"Usage:
-i|index={an index}
    The elastic index to read

-l|lines={tail x lines}	
    Display the last x lines. Defaults to 10 lines. 

-y|layout={pattern layout syntax as defined in log4net.Layout.PatternLayout}

-h|?|help
    Display help

For instance to :
LogTail.exe -i=log
LogTail.exe -index=log

If you are in powershell (or cygwin) you can do the following:
cat yourlogfile.xml | LogTail.exe
");
                return;
            }


            if (index.Any())
            {
                Tail(lines ?? 10, index, (entry) => showentry(Console.Out, entry));
                return;
            }
        }

        private static int ParseInterval(string v)
        {
            return Int32.Parse(v) * 1000;
        }

        private static async void Tail(int lines, List<string> indexes, Action<LoggingEventData> showentry)
        {
            foreach (var index in indexes)
            {
                var repo = new ElasticSearchRepository("Server=localhost;Index="+index+";Port=9200");
                await repo.GetPagedAsync(0, lines).ContinueWith(t => {
                    foreach (var item in t.Result.Hits.Select(e => e.Data))
                    {
                        showentry(item);
                    }
                });
            }
        }
    }
}
