﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using core.Enums;
using core.Plugins;
using Xunit;

namespace core.Tests
{
    public class UsingCustomMethods
    {
        public void Test__LinqWhere1()
        {
            // Arrange
            Expression<Func<JsArray, object>> expr = array => array.RemoveAt(2);
            var extension = new CustomMethods();
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter,
                    new LinqMethods(), extension));

            // Assert
            Assert.Equal("array.splice(arg_0, 1)", js);
            Assert.Equal(2, ((ConstantExpression)extension.Parameters["arg_0"]).Value);
        }

        public class JavascriptMethodNameAttribute : Attribute
        {
            public string Name { get; }

            public object[] PositionalArguments { get; set; }

            public JavascriptMethodNameAttribute(string name)
            {
                Name = name;
            }
        }

        public class CustomMethods : JavascriptConversionExtension
        {
            public readonly Dictionary<string, object> Parameters = new Dictionary<string, object>();
            
            public override void ConvertToJavascript(JavascriptConversionContext context)
            {
                var methodCallExpression = context.Node as MethodCallExpression;

                var nameAttribute = methodCallExpression?
                    .Method
                    .GetCustomAttributes<JavascriptMethodNameAttribute>()
                    .FirstOrDefault();

                if (nameAttribute == null)
                    return;

                context.PreventDefault();

                context.Visitor.Visit(methodCallExpression.Object);
                var javascriptWriter = context.GetWriter();
                javascriptWriter.Write(".");
                javascriptWriter.Write(nameAttribute.Name);
                javascriptWriter.Write("(");

                for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
                {
                    var name = "arg_" + Parameters.Count;
                    if (i != 0)
                    {
                        javascriptWriter.Write(", ");
                    }

                    javascriptWriter.Write(name);
                    Parameters[name] = methodCallExpression.Arguments[i];
                }
                if (nameAttribute.PositionalArguments != null)
                {
                    for (var i = methodCallExpression.Arguments.Count;
                        i < nameAttribute.PositionalArguments.Length;
                        i++)
                    {
                        if (i != 0)
                        {
                            javascriptWriter.Write(", ");
                        }

                        context.Visitor.Visit(Expression.Constant(nameAttribute.PositionalArguments[i]));
                    }
                }

                javascriptWriter.Write(")");
            }
        }

        public class JsArray
        {
            [JavascriptMethodName("splice", PositionalArguments = new object[] { 0, 1 })]
            public JsArray RemoveAt(int index)
            {
                throw new NotSupportedException("Never called");
            }
        }
    }
}