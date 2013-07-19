using NVelocity.Context;
using NVelocity.Runtime;
using NVelocity.Runtime.Parser.Node;
using NVelocity.Runtime.Visitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WidgetConverter
{
    /*
     * Known Issues
     *  + Doesn't handle foreach loops
     *  + Doesn't handle registerEndOfPageHtml directives
     *  + Issues with velocity's handling of if statemetns - velocity excepts any object, not just a boolean
     *  + String Interpolation not supported ("$var$var$var")
     */
    public class VelocityRazorConverterVisitor : BaseVisitor //NodeViewMode
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly IRuntimeServices _runtimeServices;
        private readonly IInternalContextAdapter _context;
        private readonly IDictionary<string, Action> _directives = new Dictionary<string, Action>
		{
			{"foreach", () => {}},
			{"registerEndOfPageHtml", () => {}}
		};

        private bool _inCodeBlock = false;
        protected bool IsInMarkup { get; set; }
        protected bool IsInCodeBlock
        {
            get { return _inCodeBlock; }
            set
            {
                if (value & !IsInCodeBlock)
                    IsInMarkup = false;
                else if (value == false && IsInCodeBlock)
                    IsInMarkup = true;

                _inCodeBlock = value;
            }
        }



        public VelocityRazorConverterVisitor(IRuntimeServices runtimeServices, IInternalContextAdapter context)
        {
            _runtimeServices = runtimeServices;
            _context = context;
            IsInCodeBlock = false;
            IsInMarkup = true;
        }

        private bool IsExtension(string label)
        {
            return label.StartsWith("core_", StringComparison.OrdinalIgnoreCase);
        }

        private void AddSpaceIfOnSameLineAsParentNode(INode node)
        {
            if (node.Parent.Line == node.Line)
                _builder.Append(' ');
        }

        protected virtual void Assert(bool expression, string message = "")
        {
            if (expression)
                return;

            if (String.IsNullOrEmpty(message))
                throw new Exception("Assertion Failed");
            else
                throw new Exception("Assertion Failed: " + message);
        }
        public string RazorCode()
        {
            return _builder.ToString();
        }


        public override object Visit(SimpleNode node, object data)
        {
            var range = node as ASTIntegerRange;
            if (range != null)
                return Visit(range, data);
            else
                return base.Visit(node, data);
        }


        #region Binary Operators
        // Boolean logic
        public override object Visit(ASTAndNode node, object data) { return VisitBinaryOperator(node, data, "&&"); }
        public override object Visit(ASTOrNode node, object data) { return VisitBinaryOperator(node, data, "||"); }
        // Numeric Operations
        public override object Visit(ASTAddNode node, object data) { return VisitBinaryOperator(node, data, "+"); }
        public override object Visit(ASTSubtractNode node, object data) { return VisitBinaryOperator(node, data, "-"); }
        public override object Visit(ASTMulNode node, object data) { return VisitBinaryOperator(node, data, "*"); }
        public override object Visit(ASTDivNode node, object data) { return VisitBinaryOperator(node, data, "/"); }
        public override object Visit(ASTModNode node, object data) { return VisitBinaryOperator(node, data, "%"); }
        //Comparison operators
        public override object Visit(ASTEQNode node, object data) { return VisitBinaryOperator(node, data, "=="); }
        public override object Visit(ASTNENode node, object data) { return VisitBinaryOperator(node, data, "!="); }
        public override object Visit(ASTGTNode node, object data) { return VisitBinaryOperator(node, data, ">"); }
        public override object Visit(ASTGENode node, object data) { return VisitBinaryOperator(node, data, ">="); }
        public override object Visit(ASTLTNode node, object data) { return VisitBinaryOperator(node, data, "<"); }
        public override object Visit(ASTLENode node, object data) { return VisitBinaryOperator(node, data, "<="); }


        public override object Visit(ASTAssignment node, object data)
        {
            Assert(node.ChildrenCount == 2);

            var reference = node.GetChild(0) as ASTReference;
            var expression = node.GetChild(1) as ASTExpression;

            Assert(reference != null);
            Assert(expression != null);

            bool startedInCodeBlock = IsInCodeBlock;

            if (!startedInCodeBlock)
            {
                _builder.Append("@{");
                IsInCodeBlock = true;
            }
            else if (IsInMarkup)
                _builder.Append('@');

            _builder.Append("ViewBag.");
            reference.Accept(this, data);
            _builder.Append(" = ");
            expression.Accept(this, data);
            _builder.Append(';');
            if (!startedInCodeBlock)
                _builder.Append(" }");

            IsInCodeBlock = startedInCodeBlock;
            return null;
        }

        public override object Visit(ASTBlock node, object data)
        {
            bool startedInCodeBlock = IsInCodeBlock;
            _builder.Append('{');
            IsInCodeBlock = true;
            if (node.Line != node.Parent.Line)
                _builder.AppendLine();

            var children = Enumerable.Range(0, node.ChildrenCount)
                .Select(x => node.GetChild(x));

            foreach (var child in children)
            {
                child.Accept(this, data);
                if (!IsInMarkup && child is ASTReference)
                    _builder.Append(';');
            }

            IsInCodeBlock = startedInCodeBlock;
            _builder.Append("}");

            if (node.Line != node.Parent.Line)
                _builder.AppendLine();

            return null;
        }

        public override object Visit(ASTDirective node, object data)
        {
            var key = _directives.Keys.FirstOrDefault(x => x.Equals(node.DirectiveName, StringComparison.OrdinalIgnoreCase));
            if (key != null)
            {
                var handler = _directives[key];
                handler();
            }
            else
            {
                _builder.Append(node.Literal);
            }
            return null;

            return base.Visit(node, data);
        }

        protected virtual object VisitBinaryOperator(INode node, object data, string operand)
        {
            Assert(node.ChildrenCount == 2);
            var left = node.GetChild(0);
            var right = node.GetChild(1);

            if (left is ASTExpression)
                _builder.Append("(");
            left.Accept(this, data);
            if (left is ASTExpression)
                _builder.Append(")");

            _builder.Append(" ");
            _builder.Append(operand);
            _builder.Append(" ");


            if (right is ASTExpression)
                _builder.Append("(");
            right.Accept(this, data);
            if (right is ASTExpression)
                _builder.Append(")");

            return null;
        }
        #endregion

        #region If Else
        public override object Visit(ASTIfStatement node, object data)
        {
            Assert(node.ChildrenCount >= 2);
            var expr = node.GetChild(0) as ASTExpression;
            Assert(expr != null);
            bool startedInCode = IsInCodeBlock;
            if (!startedInCode)
                _builder.Append('@');

            _builder.Append("if(");
            IsInCodeBlock = true;
            Visit(expr, data);
            IsInCodeBlock = startedInCode;
            _builder.Append(") ");
            for (int i = 1; i < node.ChildrenCount; i++)
            {
                node.GetChild(i).Accept(this, data);
            }
            return null;
        }

        public override object Visit(ASTElseIfStatement node, object data)
        {
            Assert(node.ChildrenCount >= 2);
            var expr = node.GetChild(0) as ASTExpression;
            Assert(expr != null);

            AddSpaceIfOnSameLineAsParentNode(node);
            _builder.Append("else if(");
            Visit(expr, data);
            _builder.Append(") ");
            for (int i = 1; i < node.ChildrenCount; i++)
            {
                node.GetChild(i).Accept(this, data);
            }
            return null;
        }

        public override object Visit(ASTElseStatement node, object data)
        {
            AddSpaceIfOnSameLineAsParentNode(node);

            _builder.Append("else ");
            //TODO: fix inentation of "} else {" for a multi line if
            return base.Visit(node, data);
        }
        #endregion


        #region Literals

        public override object Visit(ASTFalse node, object data)
        {
            _builder.Append("false");
            return base.Visit(node, data);
        }

        public override object Visit(ASTTrue node, object data)
        {
            _builder.Append("true");
            return base.Visit(node, data);
        }

        public override object Visit(ASTStringLiteral node, object data)
        {
            //TODO: support string dictionary
            //TODO: support string interpolation
            //TODO: use 'image' field via reflection
            var image = node.FirstToken.Image.Substring(1, (node.FirstToken.Image.Length - 1) - 1);
            if (StringLiteralWrapper.IsDictionaryString(image))
            {
                var results = StringLiteralWrapper.InterpolateDictionaryString(image);
                WriteDictionary(results);
            }
            else if (!(node.FirstToken.Image.StartsWith("\"") && ((node.FirstToken.Image.IndexOf('$') != -1) || (node.FirstToken.Image.IndexOf('#') != -1))))
            {
                _builder.Append('"');
                _builder.Append(image);
                _builder.Append('"');
            }
            else
            {
                _builder.Append('"');
                _builder.Append(image);
                _builder.Append('"');
            }
            return base.Visit(node, data);
        }

        private void WriteDictionary(IDictionary<string, object> dict)
        {
            _builder.Append("Dictionary(new {");
            bool isFirstEntry = true;
            foreach (var entry in dict)
            {
                if (isFirstEntry)
                    isFirstEntry = false;
                else
                    _builder.Append(", ");

                _builder.Append(entry.Key);
                _builder.Append(" = ");
                if (entry.Value is IDictionary<string, object>)
                {
                    WriteDictionary(entry.Value as IDictionary<string, object>);
                }
                else if (entry.Value is string)
                {
                    var value = (string)entry.Value;
                    using (var reader = new StringReader(entry.Value.ToString()))
                    {
                        var node = _runtimeServices.Parse(reader, "StringDictionary");
                        node.Init(_context, _runtimeServices);
                        node.Accept(this, null);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Values must be of type string or IDictionary<string, object>");
                }
            }
            _builder.Append("})");
        }

        public override object Visit(ASTNumberLiteral node, object data)
        {
            _builder.Append(node.Value(context));
            return base.Visit(node, data);
        }

        public override object Visit(ASTObjectArray node, object data)
        {
            _builder.Append("List(");
            if (node.ChildrenCount > 0)
            {
                bool isFirst = true;
                for (int i = 0; i < node.ChildrenCount; i++)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        _builder.Append(", ");
                    node.GetChild(i).Accept(this, data);
                }
            }
            _builder.Append(")");

            return null;
        }
        #endregion

        #region TODO

        public virtual object Visit(ASTIntegerRange node, object data)
        {
            throw new NotImplementedException();
        }

        public override object Visit(ASTText node, object data)
        {
            bool startedInCodeBlock = IsInCodeBlock;

            if (!IsInMarkup && !String.IsNullOrWhiteSpace(node.Literal) && node.Literal.Trim().StartsWith("<"))
                IsInMarkup = true;

            _builder.Append(node.Literal);
            var result = base.Visit(node, data);
            return result;
        }

        public override object Visit(ASTprocess node, object data)
        {
            return base.Visit(node, data);
            throw new NotImplementedException();
            //return result;
        }

        public override object Visit(ASTWord node, object data)
        {
            var result = base.Visit(node, data);
            throw new NotImplementedException();
            return result;
        }

        public override object Visit(ASTMap node, object data)
        {
            throw new NotSupportedException();
        }

        #endregion


        #region Code

        public override object Visit(ASTIdentifier node, object data)
        {
            if (node.Parent is ASTReference || node.Parent is ASTMethod)
            {
                _builder.Append('.');
                _builder.Append(node.FirstToken.Image);
                return base.Visit(node, data);
            }
            else
                throw new NotImplementedException();
        }

        public override object Visit(ASTReference node, object data)
        {
            var name = node.RootString;
            if (node.Parent is ASTAssignment)
            {
                _builder.Append(name);
                return base.Visit(node, data);
            }
            else
            {
                if (IsInMarkup)
                    _builder.Append('@');

                if (IsExtension(name))
                    _builder.Append("Extensions.");
                else
                    _builder.Append("ViewBag.");

                _builder.Append(name);

                var result = base.Visit(node, data);

                return result;
            }
        }

        public override object Visit(ASTMethod node, object data)
        {
            bool startedInCode = IsInCodeBlock;
            _builder.Append('.');
            _builder.Append(node.FirstToken.Image);
            _builder.Append("(");
            if (node.ChildrenCount > 1)
            {
                IsInCodeBlock = true;
                bool firstParam = true;
                for (int i = 1; i < node.ChildrenCount; i++)
                {
                    if (!firstParam)
                        _builder.Append(", ");
                    else
                        firstParam = false;

                    node.GetChild(i).Accept(this, data);
                }

            }
            IsInCodeBlock = startedInCode;
            _builder.Append(")");
            return null;
        }

        #endregion

        public override object Visit(ASTExpression node, object data)
        {
            Assert(node.ChildrenCount == 1);
            return node.GetChild(0).Accept(this, data);
        }


        public override object Visit(ASTNotNode node, object data)
        {
            _builder.Append("!_VELOCITY_IF(");
            var result = base.Visit(node, data);
            _builder.Append(")");
            return result;
        }

        public override object Visit(ASTSetDirective node, object data)
        {
            Assert(node.ChildrenCount == 1);
            var expression = node.GetChild(0) as ASTExpression;
            Assert(expression != null);
            var result = Visit(expression, data);
            _builder.Append(Environment.NewLine);
            return result;
        }

        public override object Visit(ASTComment node, object data)
        {
            _builder.Append("@*");
            if (node.Literal.StartsWith("##"))
            {
                _builder.Append(node.Literal.Substring(2).TrimEnd('\r', '\n'));
                _builder.AppendLine("*@");
            }
            else
            {
                throw new NotSupportedException();
            }
            return null;
        }




    }
}
