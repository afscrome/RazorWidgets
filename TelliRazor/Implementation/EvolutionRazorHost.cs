using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.WebPages.Razor;

namespace TelliRazor
{
    public class EvolutionRazorHost : WebPageRazorHost 
	{
        private const string WidgetClassNamePrefix = "_Widget_";

        public EvolutionRazorHost(string virtualPath)
            :base(virtualPath)
		{
            //Clear namespaces added by by default.
            NamespaceImports.Clear();
            
            CodeLanguage = RazorCodeLanguage.GetLanguageByExtension(".cshmtl");
            DefaultBaseClass = DefaultPageBaseClass = typeof(RazorWidgetBase).FullName;
            
			DefaultNamespace = "TelliRazor.Widgets";
			DefaultClassName = "Widget";
            NamespaceImports.Add("Tuple = System.Tuple");
            
            this.StaticHelpers = true;
            this.EnableInstrumentation = false;
            /*
            GeneratedClassContext = new GeneratedClassContext(GeneratedClassContext.DefaultExecuteMethodName,
                GeneratedClassContext.DefaultWriteMethodName,
                GeneratedClassContext.DefaultWriteLiteralMethodName,
                "WriteTo",
                "WriteLiteralTo",
                typeof(HelperResult).FullName
                );
            */
		}

        protected override string GetClassName(string virtualPath)
        {
            // Remove "~/" and run through our santizer
            // For example, for ~/Foo/Bar/Baz.cshtml, the class name is _Page_Foo_Bar_Baz_cshtml
            return ParserHelpers.SanitizeClassName(WidgetClassNamePrefix + virtualPath.TrimStart('~', '/'));
        }

        public override RazorCodeGenerator DecorateCodeGenerator(RazorCodeGenerator incomingCodeGenerator)
        {
            return base.DecorateCodeGenerator(incomingCodeGenerator);
        }
        
        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            return new EvolutionCSharpCodeParser();
        }
        public override void PostProcessGeneratedCode(CodeGeneratorContext context)
        {
            //base.PostProcessGeneratedCode(context);
        }

        /*
        public override ParserBase DecorateMarkupParser(ParserBase incomingMarkupParser)
        {
            //TODO: add minification of whitespace ???
            var parser = base.DecorateMarkupParser(incomingMarkupParser);
            return parser;
        }
*/

	}
}
