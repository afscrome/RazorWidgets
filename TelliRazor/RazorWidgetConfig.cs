
using System;
using System.Xml;
using System.Xml.Linq;
using Telligent.DynamicConfiguration.Components;
namespace TelliRazor
{
    public class RazorWidgetConfig
    {
        private RazorWidgetConfig() { }
        public RazorWidgetConfig(XElement config)
        {
            InstanceId = config.Attribute("instanceIdentifier").Value;
            Name = config.Attribute("name").Value + " (razor)";
            Description = config.Attribute("description").Value;
            CssClass = config.Attribute("cssClass").Value;
            ShowHeaderByDefault = (bool)config.Attribute("showHeaderByDefault");

            var dynamicConfig = config.Element("configuration");
            Configuration = new Lazy<PropertyGroup[]>(() =>
            {
                if (dynamicConfig == null || !dynamicConfig.HasElements)
                    return new PropertyGroup[0];

                using (var reader = dynamicConfig.CreateReader())
                {
                    var doc = new XmlDocument();
                    doc.Load(reader);
                    return PropertyGroup.ParseAll(doc.DocumentElement.ChildNodes);
                }
            });
        }

        public string InstanceId { get; private set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string CssClass { get; internal set; }
        public bool ShowHeaderByDefault { get; internal set; }
        public Lazy<PropertyGroup[]> Configuration { get; private set; }

        internal static readonly RazorWidgetConfig EmptyConfig = new RazorWidgetConfig()
        {
            InstanceId = "$$DUMMY$$",
            ShowHeaderByDefault = false
        };
    }
}
