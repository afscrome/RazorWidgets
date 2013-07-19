using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TelliRazor
{
	//[SecuritySafeCritical]
	// Based on System.web.webpages.webpageexecuting base
	public abstract class RazorWidgetBase
	{
		private TextWriter _writer;

        internal TextWriter Writer
        {
            set
            {
                if (_writer != null)
                    throw new InvalidOperationException("Writer has already been set");
                _writer = value;
            }
            private get
            {
                if (_writer == null)
                    throw new InvalidOperationException("Writer has not yet been set");
                return _writer;
            }
        }

        protected dynamic ViewBag = new ViewBag();
        protected dynamic Extensions = new ExtensionBag();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void Execute();

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void Write(object obj) { WriteTo(Writer, obj); }
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void WriteLiteral(string str) { WriteLiteralTo(Writer, str); }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected static void WriteLiteralTo(TextWriter writer, string literal)
		{
			if (literal != null)
				writer.Write(literal);
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected static void WriteTo(TextWriter writer, object obj)
		{
			if (obj != null)
				writer.Write(obj);
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void WriteAttribute(string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
		{
			WriteAttributeTo(Writer, name, prefix, suffix, values);
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected static void WriteAttributeTo(TextWriter writer, string name, PositionTagged<string> prefix, PositionTagged<string> suffix, params AttributeValue[] values)
		{
			bool first = true;
			bool wroteSomething = false;
			if (values.Length == 0)
			{
				// Explicitly empty attribute, so write the prefix and suffix
				WriteLiteralTo(writer, prefix);
				WriteLiteralTo(writer, suffix);
			}
			else
			{
				for (int i = 0; i < values.Length; i++)
				{
					AttributeValue attrVal = values[i];
					PositionTagged<object> val = attrVal.Value;
					PositionTagged<string> next = i == values.Length - 1 ?
						suffix : // End of the list, grab the suffix
						values[i + 1].Prefix; // Still in the list, grab the next prefix

					bool? boolVal = null;
					if (val.Value is bool)
					{
						boolVal = (bool)val.Value;
					}

					if (val.Value != null && (boolVal == null || boolVal.Value))
					{
						string valStr = val.Value as string;
						if (valStr == null)
						{
							valStr = val.Value.ToString();
						}
						if (boolVal != null)
						{
							Debug.Assert(boolVal.Value);
							valStr = name;
						}

						if (first)
						{
							WriteLiteralTo(writer, prefix);
							first = false;
						}
						else
						{
							WriteLiteralTo(writer, attrVal.Prefix);
						}

						// Calculate length of the source span by the position of the next value (or suffix)
						int sourceLength = next.Position - attrVal.Value.Position;

						if (attrVal.Literal)
							WriteLiteralTo(writer, valStr);
						else
							WriteTo(writer, valStr); // Write value

						wroteSomething = true;
					}
				}
				if (wroteSomething)
				{
					WriteLiteralTo(writer, suffix);
				}
			}
		}

        [Obsolete("Workaround for velocity allowing if(null) == false and if (!null) = true", false)]
        protected bool _VELOCITY_IF(object value)
        {
            if (value is VelocityCompatibilityWrapper)
                return false;
            if (value is bool)
                return (bool)value;
            if (value is string)
                return !String.IsNullOrEmpty((string)value);
            else
                return value != null;
        }

        protected IDictionary Dictionary(object values)
        {
            var dict = new HybridDictionary(true);
            var type = values.GetType();
            if (!Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false))
                throw new ArgumentOutOfRangeException("values", "Must be an anonymous type");

            foreach (var property in type.GetProperties())
            {
                dict.Add(property.Name, property.GetValue(values, null));                
            }

            return dict;
        }

        protected ICollection<dynamic> List(params object[]values) {
            return new List<dynamic>(values);
        }


        //TODO: support instrumentation via tracepoints
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected static void BeginContext(string virtualPath, int startPosition, int length, bool isLiteral)
        {
        }
        //TODO: support instrumentation
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected static void EndContext(string virtualPath, int startPosition, int length, bool isLiteral)
        {
        }

    }
}