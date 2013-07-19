using NVelocity.Context;
using NVelocity.Runtime.Parser;
using NVelocity.Runtime.Parser.Node;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WidgetConverter
{
    /// <summary>
    /// Mostly based off code copy & pasted from the core NVelocity ASTStringLiteral class
    /// have to copy'n paste becuase it's mostly private code
    /// + contains a few modification to output original values rather than evaluating them
    /// </summary>
    public class StringLiteralWrapper
    {
        // begin and end dictionary string markers
        private static readonly String DictStart = "%{";
        private static readonly String DictEnd = "}";

        public static bool IsDictionaryString(string str)
        {
            return str.StartsWith(DictStart) && str.EndsWith(DictEnd);
        }

        /// <summary>
        /// Interpolates the dictionary string.
        /// dictionary string is any string in the format
        /// "%{ key='value' [,key2='value2' }"		
        /// "%{ key='value' [,key2='value2'] }"		
        /// </summary>
        /// <param name="str">If valid input a HybridDictionary with zero or more items,
        ///	otherwise the input string</param>
        /// <param name="context">NVelocity runtime context</param>
        public static IDictionary<string, object> InterpolateDictionaryString(string str)
        {
            char[] contents = str.ToCharArray();
            int lastIndex;

            return RecursiveBuildDictionary(contents, 2, out lastIndex);
        }

        private static void ProcessDictEntry(IDictionary<string, object> dict, StringBuilder keyBuilder, StringBuilder valueBuilder)
        {
            dict.Add(keyBuilder.ToString().Trim(), valueBuilder.ToString().Trim());
            keyBuilder.Clear();
            valueBuilder.Clear();
        }

        private static IDictionary<string, object> RecursiveBuildDictionary(char[] contents, int fromIndex,
                                                          out int lastIndex)
        {
            // key=val, key='val', key=$val, key=${val}, key='id$id'

            lastIndex = 0;

            //HybridDictionary hash = new HybridDictionary(true);
            var hash = new Dictionary<string, object>();

            bool inKey, valueStarted, expectSingleCommaAtEnd, inTransition;
            int inEvaluationContext = 0;
            inKey = false;
            inTransition = true;
            valueStarted = expectSingleCommaAtEnd = false;
            StringBuilder sbKeyBuilder = new StringBuilder();
            StringBuilder sbValBuilder = new StringBuilder();

            for (int i = fromIndex; i < contents.Length; i++)
            {
                char c = contents[i];

                if (inTransition)
                {
                    // Eat all insignificant chars
                    if (c == ',' || c == ' ')
                    {
                        continue;
                    }
                    else if (c == '}') // Time to stop
                    {
                        lastIndex = i;
                        break;
                    }
                    else
                    {
                        inTransition = false;
                        inKey = true;
                    }
                }

                if (c == '=' && inKey)
                {
                    inKey = false;
                    valueStarted = true;
                    continue;
                }

                if (inKey)
                {
                    sbKeyBuilder.Append(c);
                }
                else
                {
                    if (valueStarted && c == ' ') continue;

                    if (valueStarted)
                    {
                        valueStarted = false;

                        if (c == '\'')
                        {
                            expectSingleCommaAtEnd = true;
                            sbValBuilder.Append('"');
                            continue;
                        }
                        else if (c == '{')
                        {
                            object nestedHash = RecursiveBuildDictionary(contents, i + 1, out i);
                            hash.Add(sbKeyBuilder.ToString().Trim(), nestedHash);
                            sbKeyBuilder.Clear();
                            inKey = false;
                            valueStarted = false;
                            inTransition = true;
                            expectSingleCommaAtEnd = false;
                            continue;
                        }
                    }

                    if (c == '\\')
                    {
                        char ahead = contents[i + 1];

                        // Within escape

                        switch (ahead)
                        {
                            case 'r':
                                i++;
                                sbValBuilder.Append('\r');
                                continue;
                            case '\'':
                                i++;
                                sbValBuilder.Append('\'');
                                continue;
                            case '"':
                                i++;
                                sbValBuilder.Append('"');
                                continue;
                            case 'n':
                                i++;
                                sbValBuilder.Append('\n');
                                continue;
                        }
                    }

                    if ((c == '\'' && expectSingleCommaAtEnd) ||
                        (!expectSingleCommaAtEnd && c == ',') ||
                        (inEvaluationContext == 0 && c == '}'))
                    {
                        if (c == '\'')
                            sbValBuilder.Append('\"');
                        ProcessDictEntry(hash, sbKeyBuilder, sbValBuilder);

                        inKey = false;
                        valueStarted = false;
                        inTransition = true;
                        expectSingleCommaAtEnd = false;

                        if (inEvaluationContext == 0 && c == '}')
                        {
                            lastIndex = i;
                            break;
                        }
                    }
                    else
                    {
                        if (c == '{')
                        {
                            inEvaluationContext++;
                        }
                        else if (inEvaluationContext != 0 && c == '}')
                        {
                            inEvaluationContext--;
                        }

                        sbValBuilder.Append(c);
                    }
                }

                if (i == contents.Length - 1)
                {
                    if (sbKeyBuilder.ToString().Trim() == String.Empty)
                    {
                        break;
                    }

                    lastIndex = i;

                    ProcessDictEntry(hash, sbKeyBuilder, sbValBuilder);

                    inKey = false;
                    valueStarted = false;
                    inTransition = true;
                    expectSingleCommaAtEnd = false;
                }
            }

            return hash;
        }

    }
}
