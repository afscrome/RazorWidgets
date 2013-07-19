using NVelocity.Context;
using NVelocity.Runtime.Directive;
using NVelocity.Runtime.Parser.Node;
using System;
using System.IO;

namespace WidgetConverter
{
    public class RegisterEndOfPageHtmlDirective : Directive
    {
        private string _name = "registerEndOfPageHtml";
        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public override DirectiveType Type
        {
            get { return DirectiveType.BLOCK; }
        }

        public override bool Render(IInternalContextAdapter context, TextWriter writer, INode node)
        {
            throw new NotImplementedException();
        }
    }
}
