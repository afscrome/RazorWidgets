using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace TelliRazor
{
	internal class RazorWidgetErrorWriter
	{
		private readonly HtmlTextWriter _writer;
		private readonly RazorWidgetConfig _config;
		private readonly string _viewName;
		public RazorWidgetErrorWriter(HtmlTextWriter writer, RazorWidgetConfig config, string viewName = null)
		{
			_writer = writer;
			_config = config;
			_viewName = viewName;
		}

		public void WriteError(string title, Action detail)
		{
			_writer.AddAttribute("class", "message error");
			WriteInTag(HtmlTextWriterTag.Div, () =>
			{
				_writer.AddStyleAttribute(HtmlTextWriterStyle.Margin, "0");
				WriteInTag(HtmlTextWriterTag.H3, () => _writer.WriteEncodedText(title));

				if (HttpContext.Current.IsCustomErrorEnabled || HttpContext.Current.IsDebuggingEnabled)
				{
					detail();

					WriteInTag(HtmlTextWriterTag.Div, () =>
					{
						WriteInTag(HtmlTextWriterTag.Strong, () => _writer.Write("Widget: "));
						_writer.WriteEncodedText(_config.Name);
						WriteInTag(HtmlTextWriterTag.Em, () =>
						{
							_writer.Write(" (");
							_writer.Write(_config.InstanceId);
							_writer.Write(")");
						});
						if (!String.IsNullOrEmpty(_viewName))
						{
							WriteInTag(HtmlTextWriterTag.Strong, () => _writer.Write(" View: "));
							_writer.Write(_viewName);
						}
					});

				}
			});
		}

		public void WriteExceptionDetail(Exception ex)
		{
			WriteInTag(HtmlTextWriterTag.P, () => _writer.WriteEncodedText(ex.Message));

			var frame = ExceptionSourceMapper.GetLatestSourceFrame(ex);
			if (frame != null)
			{
				var fileName = frame.GetFileName();
				if (!String.IsNullOrEmpty(fileName))
				{
					using (var file = File.OpenRead(frame.GetFileName()))
					{
						WriteSource(file, frame.GetFileLineNumber());
					}
				}
			}

			WriteStackTrace(ex);
		}

		public void WriteParseCompileError(Stream file, string message, int lineNumber)
		{
			WriteInTag(HtmlTextWriterTag.P, () => _writer.WriteEncodedText(message));

			WriteSource(file, lineNumber);
		}



		protected void WriteStackTrace(Exception ex)
		{
			WriteInTag(HtmlTextWriterTag.Strong, () => _writer.Write("Stack Trace:"));
			WriteCodeBlock(() => _writer.WriteEncodedText(ex.ToString()));
		}

		protected void WriteInTag(HtmlTextWriterTag tag, Action write)
		{
			_writer.RenderBeginTag(tag);
			write();
			_writer.RenderEndTag();
		}

		protected void WriteCodeBlock(Action code)
		{
			_writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "#ffffcc");
			_writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "black");
			_writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0.5em");
			WriteInTag(HtmlTextWriterTag.Pre, () =>
			{
				var originalIndent = _writer.Indent;
				_writer.Indent = 0;
				code();
				_writer.Indent = originalIndent;
			});
		}


		private void WriteSource(Stream sourceFile, int errorLine, int surroundingLines = 2)
		{
			//TODO: Try catch
			var errorAndSurroundingLines = ExceptionSourceMapper.GetCodeSurroundingErrorFromSource(sourceFile, errorLine, surroundingLines);

			if (!errorAndSurroundingLines.Any())
				return;

			WriteCodeBlock(() =>
			{
				var maxLineNumberLength = errorAndSurroundingLines.Max(x => x.LineNumber).ToString().Length;

				for (int i = 0; i < errorAndSurroundingLines.Count; i++)
				{
					var line = errorAndSurroundingLines[i];
					if (line.IsError)
						_writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "red");

					WriteInTag(HtmlTextWriterTag.Span, () =>
						{
							//Make sure all outputed linenumbers have the same length
							var paddedLineNumber = line.LineNumber.ToString().PadLeft(maxLineNumberLength);
							_writer.Write("Line ");
							_writer.Write(paddedLineNumber);
							_writer.Write(": ");
							_writer.WriteEncodedText(line.Contents);
						});

					if (i != errorAndSurroundingLines.Count - 1)
						_writer.Write(Environment.NewLine);
				}
			});
		}
	}

	public static class ExceptionSourceMapper
	{
		public static IList<SourceError> GetCodeSurroundingErrorFromSource(StackFrame frame, int surroundingLines = 2)
		{
			using (var stream = File.OpenRead(frame.GetFileName()))
			{
				return GetCodeSurroundingErrorFromSource(stream, frame.GetFileLineNumber(), surroundingLines);
			}
		}

		public static IList<SourceError> GetCodeSurroundingErrorFromSource(Stream sourceFile, int errorLine, int surroundingLines = 2)
		{
			int lowerBound = errorLine - surroundingLines;
			int upperBound = errorLine + surroundingLines;
			int currentLine = 1;

			var source = new List<SourceError>();

			using (var reader = new StreamReader(sourceFile))
			{
				while (currentLine <= upperBound && !reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (currentLine >= lowerBound)
					{
						source.Add(new SourceError(line, currentLine, currentLine == errorLine));
					}
					currentLine++;
				}
			}

			return source;
		}

		public static StackFrame GetLatestSourceFrame(Exception ex)
		{
			var stack = new StackTrace(ex, true);
			for (int i = 0; i < stack.FrameCount; i++)
			{
				var frame = stack.GetFrame(i);
				var sourceFile = frame.GetFileName();
				if (!String.IsNullOrEmpty(sourceFile) && File.Exists(sourceFile))
					return frame;
			}
			return null;
		}

	}

	public class SourceError
	{
		public SourceError(string line, int lineNumber, bool isError)
		{
			Contents = line;
			LineNumber = lineNumber;
			IsError = isError;
		}

		public string Contents { get; private set; }
		public int LineNumber { get; private set; }
		public bool IsError { get; private set; }
	}
}
