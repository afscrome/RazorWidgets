using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using Telligent.DynamicConfiguration.Components;
using Telligent.Evolution.Extensibility.UI.Version1;
using Telligent.Evolution.ScriptedContentFragments;
using Telligent.Evolution.ScriptedContentFragments.Model;

namespace TelliRazor
{
    public class RazorWidgetInstance : ConfigurableContentFragmentBase, IInstanceBasedContentFragment
    {
        private readonly IRazorWidgetService _razorWidgetService;
        private Lazy<RazorWidgetConfig> _config;
        public RazorWidgetConfig Config { get { return _config.Value; } }

        public string InstanceIdentifier { get; private set; }

        public RazorWidgetInstance()
        {
            _razorWidgetService = Singletons.WidgetService;
        }

        public IEnumerable<IInstanceBasedContentFragment> GetAllInstances()
        {
            return _razorWidgetService.WidgetConfigurations()
                .Select(x => new RazorWidgetInstance(x.Value, _razorWidgetService));
        }

        internal RazorWidgetInstance(Lazy<RazorWidgetConfig> config, IRazorWidgetService razorWidgetService)
        {
            _razorWidgetService = razorWidgetService;
            InstanceIdentifier = config.Value.InstanceId;
            _config = config;
        }

        public bool LoadInstance(string instanceId, string themeName)
        {
            InstanceIdentifier = instanceId;
            ThemeName = themeName;
            _config = _razorWidgetService.WidgetConfigurations().First(x => x.Key == InstanceIdentifier).Value;
            return _config != null;
        }


        public override string FragmentName { get { return Config.Name; } }
        public override string FragmentDescription { get { return Config.Description; } }
        public override bool ShowHeaderByDefault { get { return Config.ShowHeaderByDefault; } }
        public string InstanceCssClass { get { return Config.CssClass; } }


        public override void AddPreviewContentControls(Control control)
        {
            AddControls(control, true);
        }

        public override void AddContentControls(Control control)
        {
            AddControls(control, false);
        }

        private void AddControls(Control control, bool isPreview)
        {
            control.Controls.Add(new RazorWidgetControl(this, _razorWidgetService, isPreview));
        }

        public override string GetAdditionalCssClasses(Control control)
        {
            return Config.CssClass;
            //TODO: Css scripts
        }

        public override string GetFragmentHeader(Control control)
        {
            try
            {
                using (var writer = new StringWriter())
                {
                    _razorWidgetService.RenderWidget(Config, writer, RazorSpecialFileNames.Header);
                    //TODO: Flush?
                    return writer.ToString();
                }
            }
            catch
            {
                //TODO:
                return "ERROR: " + FragmentName;
            }
        }


        //No Caching support FOR NOW
        protected override bool IsCacheable { get { return false; } }
        protected override string CacheKey { get { return "TODO"; } }
        // No Context support for now
        public override bool HasRequiredContext(Control control) { return true; }
        // No Theme support for now
        public bool SupportsTheme(string instanceIdentifier, string themeName) { return false; }

        public override PropertyGroup[] GetPropertyGroups()
        {
            return new PropertyGroup[0];
            //return Config.Configuration.Value;
        }
    }
}
