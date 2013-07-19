using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Razor.Parser.SyntaxTree;
using Telligent.Evolution.ScriptedContentFragments.Model;
using Telligent.Evolution.ScriptedContentFragments.Services;

namespace TelliRazor
{
	public class RazorWidgetService : IRazorWidgetService
	{
        //Required to set context to allow support of widget extensions via the Extensions dynamic
        private readonly IScriptedContentFragmentContextService ContextService = Telligent.Common.Services.Get<IScriptedContentFragmentContextService>();
		//private readonly RazorWidgetCSharpCompiler _compiler;
		private readonly IRazorWidgetFileService _widgetFileService;

		public RazorWidgetService(IRazorWidgetFileService widgetFileService)
		{
			_widgetFileService = widgetFileService;
		}

        public IDictionary<string, Lazy<RazorWidgetConfig>> WidgetConfigurations()
        {
            return _widgetFileService.WidgetConfiguraitons();
        }

		public void RenderWidget(RazorWidgetConfig config, TextWriter writer, string fileName = RazorSpecialFileNames.Widget)
		{
            var instance = CreateWidgetInstance(config, writer, fileName);
            try
            {
                instance.Execute();
            }
            catch (CancelRenderingException)
            {
                return;
            }
		}

        public RazorWidgetBase CreateWidgetInstance(RazorWidgetConfig config, TextWriter writer, string fileName = RazorSpecialFileNames.Widget)
		{
            var virtualPath = String.Format("~/_razor/{0}/{1}", config.InstanceId, fileName);
            try
            {
                var widget = (RazorWidgetBase)BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(RazorWidgetBase));
                widget.Writer = writer;
                return widget;
            }
            catch (HttpCompileException ex)
            {
                throw new RazorCompileException(ex.Results.Errors);
            }
            catch (HttpParseException ex)
            {
                var errors = ex.ParserErrors
                    .OfType<ParserError>()
                    .Select(x => new RazorError(x.ErrorText, -1, x.Line - 1, 0));

                throw new RazorParseException(errors);
            }
            catch (Exception ex)
            {
                //TODO: handle
                throw;
            }
		}


	}
}
