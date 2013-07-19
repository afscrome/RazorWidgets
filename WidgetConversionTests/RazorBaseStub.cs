using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WidgetConversionTests
{
    //Stub to trick intellisense for this project
    public abstract class RazorBaseStub
    {
        protected dynamic ViewBag;
        protected dynamic Extensions;
        protected void Dictionary(object values) { throw new NotImplementedException(); }
        [Obsolete("Workaround for velocity allowing if(null) == false and if (!null) = true", false)]
        protected bool _VELOCITY_IF(object value) { throw new NotImplementedException(); }
    }
}
