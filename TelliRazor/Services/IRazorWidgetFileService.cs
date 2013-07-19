using System;
using System.Collections.Generic;
using System.IO;

namespace TelliRazor
{
	public interface IRazorWidgetFileService
	{
        IDictionary<string, Lazy<RazorWidgetConfig>> WidgetConfiguraitons();
		Stream OpenReadStream(RazorWidgetConfig config, string fileName);
		IEnumerable<string> GetFileNames(RazorWidgetConfig config);
		string GetFullFilePath(RazorWidgetConfig config, string fileName);
	}
}
