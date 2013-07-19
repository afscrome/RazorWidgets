using System;
using System.Web.WebPages.Razor;

namespace TelliRazor
{
    public class EvolutionRazorHostFactory : WebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            if (virtualPath.StartsWith("/_razor/", StringComparison.OrdinalIgnoreCase))
                return new EvolutionRazorHost(virtualPath);

            return base.CreateHost(virtualPath, physicalPath);
        }
    }
}
