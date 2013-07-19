using System;
using System.IO;
using System.Web.UI;

namespace TelliRazor
{
	public class RazorWidgetControl : Control
	{
        private readonly RazorWidgetInstance _widget;
		private readonly IRazorWidgetService _widgetService;
		private readonly bool _isPreview;

		public RazorWidgetControl(RazorWidgetInstance widget, IRazorWidgetService widgetService, bool isPreview)
		{
			_widget = widget;
			_widgetService = widgetService;
			_isPreview = isPreview;
		}

		protected override void Render(HtmlTextWriter writer)
		{
			//TODO: Work out how to support sub views / partials
			var fileName = RazorSpecialFileNames.Widget;
			var errorWriter = new RazorWidgetErrorWriter(writer, _widget.Config, null);
			var tempWriter = new StringWriter();

            bool detailedErrors = Context.IsCustomErrorEnabled;
			//TODO: how does this cope if a partial fails to render?
			//Look at UnhandledErrorFormatter for examples on producing detailed errors
            Telligent.Common.Services.Get<Telligent.Evolution.ScriptedContentFragments.Services.IScriptedContentFragmentContextService>().SetContext(_widget, this.Parent); 
			try
			{
                _widgetService.RenderWidget(_widget.Config, tempWriter);
			}
			catch (RazorParseException ex)
			{
				errorWriter.WriteError("Error Parsing Widget", () =>
				{
					foreach (var error in ex.Errors)
					{
						//Line index is 0 based, but we display line numbers as 1 based
						int errorLine = error.Location.LineIndex + 1;
						var msg = String.Format("{0} (Line: {1} Character: {2})", error.Message, errorLine, error.Location.CharacterIndex);
                        errorWriter.WriteParseCompileError(Singletons.FileService.OpenReadStream(_widget.Config, fileName), msg, errorLine);
					}
				});
				return;
			}
			catch (RazorCompileException ex)
			{
				errorWriter.WriteError("Error Compiling Widget", () =>{
					foreach (var error in ex.Errors)
					{
						var msg = String.Format("{0} (Line: {1})", error.ErrorText, error.Line);
                        errorWriter.WriteParseCompileError(Singletons.FileService.OpenReadStream(_widget.Config, fileName), msg, error.Line);
					}
				});
				return;
			}
			catch (Exception ex)
			{
				errorWriter.WriteError("Error Executing Widget", () => errorWriter.WriteExceptionDetail(ex));
				return;
			}
			writer.Write(tempWriter);
		}
	}
}
