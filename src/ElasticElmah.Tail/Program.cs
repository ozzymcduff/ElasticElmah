using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;
using log4net.Core;
using log4net.Layout;
using ElasticElmah.Appender;

namespace ElasticElmah.Tail
{
    class Program
    {
        private static string helpTest = @"Usage:
-i|index={an index}
    The elastic index to read

-l|lines={tail x lines}	
    Display the last x lines. Defaults to 10 lines. 

-y|layout={pattern layout syntax as defined in log4net.Layout.PatternLayout}

-f|format={a named layout format}
    The available formats are:
        -f=minusminus Information delimited by newline and ----------------------

-h|?|help
    Display help

For instance to :
LogTail.exe -i=log
LogTail.exe -index=log

If you are in powershell (or cygwin) you can do the following:
cat yourlogfile.xml | LogTail.exe
";

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
                { "y|layout=",v=> { layout=new PatternLayout(v);}},
                { "f|format=", v=> { layout = GetFormatLayout(v); }}
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
                Console.WriteLine(helpTest);
                return;
            }

            if (index.Any())
            {
                Tail(lines ?? 10, index, (entry) => showentry(Console.Out, entry), s => Console.Out.WriteLine("Missing index: '{0}'", s));
                return;
            }
            // No indexes, display help before exit
            Console.WriteLine(helpTest);
        }

        private static LayoutSkeleton GetFormatLayout(string format)
        {
            switch (format)
            {
                case "minusminus":
                    return
                        new PatternLayout(
                            "%newline----------------------BEGIN LOG----------------------%newline##date: %newline%date %newline##level: %newline%-5level %newline##logger: %newline%logger %newline########message:########%newline%message %newline########exception:########%newline %exception%newline----------------------END LOG----------------------%newline");
                default:
                    Console.Error.WriteLine("Unknown format '{0}'", format);
                    return null;
            }
        }

        private static void Tail(int lines, List<string> indexes, Action<LoggingEventData> showentry, Action<string> showmissingindex)
        {
            foreach (var index in indexes)
            {
                var repo = new ElasticSearchRepository("Server=localhost;Index=" + index + ";Port=9200");

				repo.GetPaged(0, lines).Tap(t =>
				                                          {
					foreach (var item in t.Hits.Select(e => e.Data))
					{
						showentry(item);
					}
				});
            }
        }
    }
}
