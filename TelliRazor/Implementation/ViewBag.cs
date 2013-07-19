using System.Collections.Generic;
using System.Dynamic;

namespace TelliRazor
{
    public class ViewBag : DynamicObject
    {
        private readonly IDictionary<string, object> _items = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            //To be better compatible with velocity, return null if object can't be found
            if (!_items.TryGetValue(binder.Name.ToUpperInvariant(), out result))
                result = VelocityCompatibilityWrapper.NullInstance;

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value == null)
                TryDeleteMember(binder.Name);
            else
                _items[binder.Name.ToUpperInvariant()] = value;

            return true;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return TryDeleteMember(binder.Name);
        }

        private bool TryDeleteMember(string name)
        {
            var key = name.ToUpperInvariant();
            if (!_items.ContainsKey(key))
                return false;

            _items.Remove(key);
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _items.Keys;
        }
    }
}
