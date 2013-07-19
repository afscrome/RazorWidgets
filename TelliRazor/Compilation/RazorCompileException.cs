using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace TelliRazor
{
	public class RazorCompileException : Exception
	{
		public ICollection<CompilerError> Errors { get; private set; }
		public RazorCompileException(CompilerErrorCollection errors)
		{
			var filteredErrors = errors
				.Cast<CompilerError>()
				.Where(x => !x.IsWarning);

			//Some compiler errors may be from the intermediate .cs file rather than the .cshtml file
			//most of the .cs errors are caused by errors in the .cshtml file, nor can we display the 
			//source for the cs file.  So hide 
            /*
			if (filteredErrors.Any(x => x.FileName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)))
				filteredErrors = filteredErrors.Where(x => x.FileName.EndsWith(".cshtml"));
             * */

			Errors = filteredErrors.ToList();
		}
	}
}
