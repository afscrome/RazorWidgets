using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Razor.Parser.SyntaxTree;

namespace TelliRazor
{
	public class RazorParseException : Exception
	{
		public IList<RazorError> Errors { get; private set; }
		public RazorParseException(IEnumerable<RazorError> errors)
		{
			Errors = errors.ToList();
		}
	}
}
