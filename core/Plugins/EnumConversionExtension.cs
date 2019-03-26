using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using core.Enums;
using core.Logic;

namespace core.Plugins
{
    public class EnumConversionExtension : JavascriptConversionExtension
    {
        private readonly EnumOptions opts;

        public EnumConversionExtension(EnumOptions options)
        {
            opts = options;
        }

        public override void ConvertToJavascript(JavascriptConversionContext context)
        {
            if (context.Node is ConstantExpression cte && cte.Type.IsEnum)
            {
                context.PreventDefault();
                var writer = context.GetWriter();
                var remaining = Convert.ToInt64(cte.Value);
                var flagsAsString = (opts & EnumOptions.FlagsAsStringWithSeparator) != 0;
                var flagsAsOrs = (opts & EnumOptions.FlagsAsNumericOrs) != 0;
                var flagsAsArray = (opts & EnumOptions.FlagsAsArray) != 0;
                var isFlags = cte.Type.IsDefined(typeof(FlagsAttribute), false);

                // when value is zero
                if (remaining == 0 && (!isFlags || !(flagsAsString || flagsAsOrs || flagsAsArray)))
                {
                    if (WriteSingleEnumItem(context, writer, 0, cte.Value, false))
                        return;
                }

                // reading enum composition
                var values = Enum.GetValues(cte.Type);
                var selected = new List<int>();
                for (var itV = 0; itV < values.Length; itV++)
                {
                    var val = Convert.ToInt64(values.GetValue(values.Length - itV - 1));
                    if ((val & remaining) == val)
                    {
                        remaining &= ~val;
                        selected.Add(values.Length - itV - 1);

                        if (!isFlags) break;
                    }
                }

                // selecting enum case
                if (isFlags)
                {
                    var cnt = selected.Count + (remaining != 0 ? 1 : 0);

                    PrecedenceController xpto = null;
                    var start = "";
                    var separator = "";
                    var end = "";

                    if (flagsAsString)
                    {
                        xpto = writer.Operation(JavascriptOperationTypes.Literal);
                        start = "\"";
                        separator = "|";
                        end = "\"";
                    }
                    else if (flagsAsArray)
                    {
                        xpto = writer.Operation(0);
                        start = "[";
                        separator = ",";
                        end = "]";
                    }
                    else if (flagsAsOrs && cnt > 1)
                    {
                        xpto = writer.Operation(JavascriptOperationTypes.Or);
                        start = "";
                        separator = "|";
                        end = "";
                    }
                    else if (cnt > 1)
                    {
                        throw new NotSupportedException("When converting flags enums to JavaScript, a flags option must be specified.");
                    }

                    using (xpto)
                    {
                        writer.Write(start);
                        var pos0 = writer.Length;
                        for (var itIdx = 0; itIdx < selected.Count; itIdx++)
                        {
                            if (pos0 != writer.Length)
                                writer.Write(separator);

                            var index = selected[itIdx];
                            var enumVal = values.GetValue(index);
                            var val = Convert.ToInt64(enumVal);
                            WriteSingleEnumItem(context, writer, val, enumVal, flagsAsString);
                        }

                        if (remaining != 0)
                        {
                            if (pos0 != writer.Length)
                                writer.Write(separator);
                            WriteSingleEnumItem(context, writer, remaining, remaining, flagsAsString);
                        }
                        writer.Write(end);
                    }
                }
                else
                {
                    if (remaining != 0)
                    {
                        var enumVal = cte.Value;
                        var val = Convert.ToInt64(cte.Value);
                        WriteSingleEnumItem(context, writer, val, enumVal, false);
                    }
                    else
                        foreach (var index in selected)
                        {
                            var enumVal = values.GetValue(index);
                            var val = Convert.ToInt64(enumVal);
                            WriteSingleEnumItem(context, writer, val, enumVal, false);
                        }
                }
            }
        }

        private bool WriteSingleEnumItem(
            JavascriptConversionContext context,
            JavascriptWriter writer,
            long numValue,
            object enumValue,
            bool inStringAlready)
        {
            if ((opts & EnumOptions.UseStrings) != 0)
            {
                if (inStringAlready)
                {
                    if ((opts & EnumOptions.UseNumbers) != 0)
                        writer.WriteLiteralStringContent(numValue.ToString());
                    else
                    {
                        var str = enumValue.ToString();
                        if (numValue == 0 && str == "0")
                            writer.WriteLiteralStringContent("");
                        else
                            writer.WriteLiteralStringContent(str);
                    }
                    return true;
                }

                using (writer.Operation(JavascriptOperationTypes.Literal))
                {
                    if ((opts & EnumOptions.UseNumbers) != 0)
                        writer.WriteLiteral(numValue.ToString());
                    else
                    {
                        var str = enumValue.ToString();
                        if (numValue == 0 && str == "0")
                            writer.WriteLiteral("");
                        else
                            writer.WriteLiteral(str);
                    }
                    return true;
                }
            }

            if ((opts & EnumOptions.UseNumbers) != 0)
            {
                using (writer.Operation(JavascriptOperationTypes.Literal))
                    writer.WriteLiteral(numValue);
                return true;
            }

            if ((opts & EnumOptions.UseStaticFields) != 0)
            {
                using (writer.Operation(JavascriptOperationTypes.IndexerProperty))
                    context.Write(enumValue.GetType().Name).WriteAccessor(enumValue.ToString());
                return true;
            }

            return false;
        }
    }
}