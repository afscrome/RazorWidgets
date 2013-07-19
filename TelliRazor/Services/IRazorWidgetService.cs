using System;
using System.Collections.Generic;
using System.IO;

namespace TelliRazor
{
	public interface IRazorWidgetService
	{
		void RenderWidget(RazorWidgetConfig widget, TextWriter writer, string fileName = RazorSpecialFileNames.Widget);
        IDictionary<string, Lazy<RazorWidgetConfig>> WidgetConfigurations();
	}

}
