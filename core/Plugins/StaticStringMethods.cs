﻿using System;
using System.Linq.Expressions;
using core.Enums;
using core.Logic;

namespace core.Plugins
{
    /// <summary>
    /// Extension that allows the conversion of static `String` class methods.
    /// </summary>
    public class StaticStringMethods : JavascriptConversionExtension
    {
        public override void ConvertToJavascript(JavascriptConversionContext context)
        {
            if (context.Node is MethodCallExpression methodCall)
                if (methodCall.Method.DeclaringType == typeof(string))
                {
                    switch (methodCall.Method.Name)
                    {
                        // TODO: to support VB string concat operator `&` we must do this by default... so this code is not necessary
                        // TODO: decide whether this will be the default or not
                        //case "Concat":
                        //    {
                        //        context.PreventDefault();
                        //        var writer = context.GetWriter();
                        //        using (writer.Operation(JavascriptOperationTypes.Concat))
                        //        {
                        //            if (methodCall.Arguments.Count == 0)
                        //                writer.Write("''");
                        //            else
                        //            {
                        //                if (GetTypeOfExpression(methodCall.Arguments[0]) != typeof(string))
                        //                    writer.Write("''+");
                        //                context.WriteMany('+', methodCall.Arguments);
                        //            }
                        //        }

                        //        return;
                        //    }

                        case "Join":
                            {
                                context.PreventDefault();
                                var writer = context.GetWriter();
                                using (writer.Operation(JavascriptOperationTypes.Call))
                                {
                                    using (writer.Operation(JavascriptOperationTypes.IndexerProperty))
                                    {
                                        var pars = methodCall.Method.GetParameters();
                                        if (pars.Length == 4 && pars[1].ParameterType.IsArray && pars[2].ParameterType == typeof(int) && pars[3].ParameterType == typeof(int))
                                            throw new NotSupportedException("The `String.Join` method with start and count paramaters is not supported.");

                                        if (pars.Length != 2 || !TypeHelpers.IsEnumerableType(pars[1].ParameterType))
                                            throw new NotSupportedException("This `String.Join` method is not supported.");

                                        // if second parameter is an enumerable, render it directly
                                        context.Visitor.Visit(methodCall.Arguments[1]);
                                        writer.Write(".join");
                                    }

                                    writer.Write('(');

                                    // separator
                                    using (writer.Operation(0))
                                        context.Visitor.Visit(methodCall.Arguments[0]);

                                    writer.Write(')');
                                }

                                return;
                            }
                    }
                }
        }
    }
}
