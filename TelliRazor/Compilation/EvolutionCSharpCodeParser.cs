using System;
using System.Web.Razor.Parser;

namespace TelliRazor
{
    public class EvolutionCSharpCodeParser : CSharpCodeParser
    {
        private const string RegisterEndOfPageKeyword = "registerEndOfPageHtml";
        public EvolutionCSharpCodeParser()
        {
            //MapDirectives(RegisterEndOfPageHtmlDirective, RegisterEndOfPageKeyword);
        }

/*
        protected virtual void RegisterEndOfPageHtmlDirective()
        {
            AcceptAndMoveNext();
            Accept(CurrentSymbol);

        }*/

        protected override void InheritsDirective()
        {
            Context.OnError(CurrentLocation, "Telligent Evolution does not support custom base types for Razor widgets");
            base.InheritsDirective();
        }
        protected override void LayoutDirective()
        {
            Context.OnError(CurrentLocation, "Telligent Evolution does not support the layout directives");
            base.LayoutDirective();
        }
        protected override void SessionStateDirective()
        {
            Context.OnError(CurrentLocation, "Telligent Evolution does not support SessionState");
            base.SessionStateDirective();
        }
/*
        protected override void FunctionsDirective()
        {
            Context.OnError(CurrentLocation, "Telligent Evolution does not support the functions directive");
            base.FunctionsDirective();
        }*/
        protected override void HelperDirective()
        {
            Context.OnError(CurrentLocation, "Telligent Evolution does not support helper directives");
            base.HelperDirective();
        }
        protected override bool ValidSessionStateValue()
        {
            return false;
        }

        protected override void ReservedDirective(bool topLevel)
        {
            Context.OnError(CurrentLocation, "Telligent Evolution does not support custom classes or namespaces in widgets.  Create a custom widget extension.");
            base.ReservedDirective(topLevel);
        }

        protected override void SectionDirective()
        {
            Context.OnError(CurrentLocation, "Telligent Evolution does not support section directives");
            base.SectionDirective();
        }

        public override void ParseSection(Tuple<string, string> nestingSequences, bool caseSensitive)
        {
            base.ParseSection(nestingSequences, caseSensitive);
        }

    }
}
