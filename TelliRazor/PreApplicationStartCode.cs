using System;
using System.Web;
using System.Web.Razor.Generator;
using System.Web.WebPages.Razor;

[assembly: PreApplicationStartMethod(typeof(TelliRazor.PreApplicationStartCode), "Start")]
namespace TelliRazor
{
    public static class PreApplicationStartCode
    {
        public static void Start()
        {
            RazorBuildProvider.CompilingPath += RazorBuildProvider_CompilingPath;
            RazorBuildProvider.CodeGenerationStarted += RazorBuildProvider_CodeGenerationStarted;
            RazorBuildProvider.CodeGenerationCompleted += RazorBuildProvider_CodeGenerationCompleted;
        }

        static void RazorBuildProvider_CodeGenerationCompleted(object sender, CodeGenerationCompleteEventArgs e)
        {
            //TODO: stop tracepoint
        }

        static void RazorBuildProvider_CodeGenerationStarted(object sender, EventArgs e)
        {
            //TODO: start tracepoint
        }


        private static void RazorBuildProvider_CompilingPath(object sender, CompilingPathEventArgs e)
        {
            if (e.VirtualPath.StartsWith("/_razor/", StringComparison.OrdinalIgnoreCase))
            {
                e.Host = new EvolutionRazorHost(e.VirtualPath);
            }
        }
    }
}
