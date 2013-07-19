using System;
using System.IO;
using Xunit;

namespace WidgetConversionTests
{
    public class VelocityConversionTests
    {
        [Fact]public void EmptyVelocityUnchanged() { Compare("empty"); }
        [Fact]public void TextOnlyVelocityUnchanged() { Compare("HtmlOnly"); }
        [Fact]public void SingleVariable() { Compare("SingleVariable"); }
        [Fact]public void SetVariable() { Compare("Set"); }
        [Fact]public void SetDoubleVariable() { Compare("DoubleSet"); }
        [Fact]public void SetInterpolatedString() { Compare("SetInterpolatedString"); }
        [Fact]public void SetDictionaryString() { Compare("SetDictionaryString"); }
        [Fact]public void SetVariableFromProperty() { Compare("SetFromProperty"); }
        [Fact]public void SetVariableFromMethod() { Compare("SetFromMethod"); }
        [Fact]public void SingleExtension() { Compare("SingleExtension"); }
        [Fact]public void ExtensionMethodNoParameters() { Compare("ExtensionMethod0Parameters"); }
        [Fact]public void ExtensionMethodOneParameter() { Compare("ExtensionMethod1Parameter"); }
        [Fact]public void ExtensionMethodTwoParameters() { Compare("ExtensionMethod2Parameters"); }
        [Fact]public void SingleLineIf() { Compare("SingleLineIf"); }
        [Fact]public void SingleLineIfElseIf() { Compare("SingleLineIfElseIf"); }
        [Fact]public void MultiLineIf() { Compare("MultiLineIf"); }
        [Fact]public void MultiLineIfElseIf() { Compare("MultiLineIfElseIf"); }
        [Fact]public void NestedIf() { Compare("NestedIf"); }
        [Fact]public void IfHide() { Compare("IfHide"); }
        [Fact]public void IfSet() { Compare("IfSet"); }
        [Fact]public void ConditionalStyle() { Compare("ConditionalStyle"); }

        private void Compare(string fileName)
        {
            var directory = "..\\..\\ConversionTests\\";
            var velocityPath = Path.Combine(directory, fileName + ".vm");
            var razorPath = Path.Combine(directory, fileName + ".cshtml");

            var velocity = String.Join(Environment.NewLine, File.ReadAllLines(velocityPath));
            var expectedRazor = String.Join(Environment.NewLine, File.ReadAllLines(razorPath));

            var converter = new WidgetConverter.WidgetConverter(".");

            var actualRazor = converter.VelocityToRazor(velocity);

            //Trim newlines from end - conversion sometimes generates extras causing hard to troubleshoot errors.
            Assert.Equal(AdjustRazorWhitespace(expectedRazor), AdjustRazorWhitespace(actualRazor));
        }

        private string AdjustRazorWhitespace(string razor)
        {
            return razor
                .Trim() //Leading / Trailing whitespace is unimportant
                .Replace("\t", "    ") // Replace tabs with four spaces
                ;
        }




    }
}
