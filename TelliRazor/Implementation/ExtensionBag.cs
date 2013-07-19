using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Telligent.Evolution.Extensibility.UI.Version1;
using Telligent.Evolution.Extensibility.Version1;

namespace TelliRazor
{
    public class ExtensionBag : DynamicObject
    {
        private readonly IDictionary<string, object> _items = new Dictionary<string, dynamic>();
        private readonly IScriptedContentFragmentExtension[] extensions = PluginManager.Get<IScriptedContentFragmentExtension>().ToArray();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var key = binder.Name.ToUpperInvariant();
            if (!_items.TryGetValue(key, out result))
            {
                var extensionPlugin = extensions.FirstOrDefault(x => x.ExtensionName.Equals(binder.Name, StringComparison.OrdinalIgnoreCase));
                result = extensionPlugin == null
                    ? VelocityCompatibilityWrapper.NullInstance
                    : result = extensionPlugin.Extension;
                _items[key] = result;
            }
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return extensions.Select(x => x.Name);
        }
    }
}
