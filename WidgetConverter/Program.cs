using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WidgetConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
                throw new ArgumentOutOfRangeException("args", "must have exactly one argument");

            var file = args[0];

            if (!File.Exists(file))
                throw new ArgumentOutOfRangeException("args", String.Format("File '{0}' does not exist", file));



            var doc = XDocument.Load(file);
            var widgets = doc.Descendants("scriptedContentFragment");

            var converter = new WidgetConverter("C:\\Telligent\\SVN\\RazorWidgets\\Web\\_Razor\\");
            widgets.AsParallel().ForAll(converter.ConvertWidget);
        }




    }
}
