using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;

namespace TelliRazor
{

    public class RazorWidgetFileService : IRazorWidgetFileService
    {
        private static readonly string _baseDirectory = HostingEnvironment.MapPath("~/_Razor/");

        public IDictionary<string, Lazy<RazorWidgetConfig>> WidgetConfiguraitons()
        {
            var directories = Directory.GetDirectories(_baseDirectory)
                .OrderBy(x => x)
                .ToArray();

            var dict = new SortedDictionary<string, Lazy<RazorWidgetConfig>>();

            foreach (var directory in directories)
	        {
                var config = new Lazy<RazorWidgetConfig>(() => { 
                    var  doc = XDocument.Load(Path.Combine(directory, RazorSpecialFileNames.Config));
                    return new RazorWidgetConfig(doc.Root);
                });
                    
                dict.Add(Path.GetFileName(directory), config);		 
	        }

            return dict;
        }
        
        public string GetFullFilePath(RazorWidgetConfig config, string fileName)
        {
            return Path.Combine(_baseDirectory, config.InstanceId, fileName);
        }

        public Stream OpenReadStream(RazorWidgetConfig config, string fileName)
        {
            var fullPath = GetFullFilePath(config, fileName);
            return File.OpenRead(fullPath);
        }

        public IEnumerable<string> GetFileNames(RazorWidgetConfig config)
        {
            var widgetDir = Path.Combine(_baseDirectory, config.InstanceId);

            return Directory.GetFiles(_baseDirectory, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(x => !x.StartsWith("_"));
        }
    }
}
