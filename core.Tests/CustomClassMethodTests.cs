using System;
using System.Linq.Expressions;
using core.Enums;
using core.Plugins;
using Newtonsoft.Json;
using Xunit;

namespace core.Tests
{
    public class CustomClassMethodTests
    {
        public class MyCustomClass
        {
            public MyCustomClass(string name)
            {
                Name = name;
            }

            public MyCustomClass()
            {
            }

            public static int GetValue()
            {
                return 1;
            }

            public static int GetValue(int x)
            {
                return x + 1;
            }

            public string Name { get; set; }

            [JavascriptMember(MemberName = "otherName")]
            public string Custom { get; set; }

            [JsonProperty(PropertyName = "otherName2")]
            public string Custom2 { get; set; }
            
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Custom3 { get; set; }
        }

        public class MyCustomClassMethods : JavascriptConversionExtension
        {
            public override void ConvertToJavascript(JavascriptConversionContext context)
            {
                if (context.Node is MethodCallExpression methodCall)
                    if (methodCall.Method.DeclaringType == typeof(MyCustomClass))
                    {
                        switch (methodCall.Method.Name)
                        {
                            case "GetValue":
                                {
                                    context.PreventDefault();
                                    using (context.Operation(JavascriptOperationTypes.Call))
                                    {
                                        using (context.Operation(JavascriptOperationTypes.IndexerProperty))
                                            context.Write("Xpto").WriteAccessor("GetValue");

                                        context.WriteManyIsolated('(', ')', ',', methodCall.Arguments);
                                    }

                                    return;
                                }
                        }
                    }
            }
        }

        [Fact]
        public void Test__CombiningMultipleExtensions()
        {
            // Arrange
            Expression<Func<string>> expr = () => string.Concat(MyCustomClass.GetValue(1) * 2, "XYZ");

            // Acr
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new MyCustomClassMethods(),
                    new StaticStringMethods()));

            // Assert
            Assert.Equal("''+Xpto.GetValue(1)*2+\"XYZ\"", js);
        }

        [Fact]
        public void Test__CombiningMultipleExtensions2()
        {
            // Arrange
            Expression<Func<string>> expr = () => string.Concat(MyCustomClass.GetValue());

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new MyCustomClassMethods(),
                    new StaticStringMethods()));

            // Assert
            Assert.Equal("''+Xpto.GetValue()", js);
        }

        [Fact]
        public void Test__NewCustomClassAsJson()
        {
            Expression<Func<MyCustomClass>> expr = () => new MyCustomClass { Name = "Miguel" };

            var js = expr.Body.CompileToJavascript(
                new JavascriptCompilationOptions(
                    new MemberInitAsJson(typeof(MyCustomClass))));

            Assert.Equal("{Name:\"Miguel\"}", js);
        }

        [Fact]
        public void Test__NewClassAsJson()
        {
            Expression<Func<MyCustomClass>> expr = () => new MyCustomClass { Name = "Miguel" };

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    (JsCompilationFlags)0,
                    MemberInitAsJson.ForAllTypes));

            Assert.Equal("function(){return {Name:\"Miguel\"};}", js);
        }

        [Fact]
        public void Test__NewCustomClassAsNewOfType()
        {
            // Arrange
            Expression<Func<MyCustomClass>> expr = () => new MyCustomClass("Miguel");

            // Act
            var js = expr.Body.CompileToJavascript();

            // Assert
            Assert.Equal("new core.Tests.CustomClassMethodTests.MyCustomClass(\"Miguel\")", js);
        }

        [Fact]
        public void Test__NewCustomClassAsNewOfTypeWithMemberInit()
        {
            // Arrange
            Expression<Func<MyCustomClass>> expr = () => new MyCustomClass { Name = "Miguel" };

            Exception exception = null;
            try
            {
                expr.Body.CompileToJavascript();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.True(exception.GetType().IsAssignableFrom(typeof(NotSupportedException)), "Exception not thrown.");
        }

        [Fact]
        public void Test__CustomMetadata1()
        {
            // Arrange
            Expression<Func<MyCustomClass, string>> expr = o => o.Custom;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"otherName", js);
        }

        [Fact]
        public void Test__CustomMetadata2()
        {
            // Arrange
            Expression<Func<MyCustomClass, string>> expr = o => o.Custom2;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"otherName2", js);
        }

        [Fact]
        public void Test__CustomMetadata3()
        {
            // Arrange
            Expression<Func<MyCustomClass, string>> expr = o => o.Custom3;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"Custom3", js);
        }

        public class MyClassBase
        {
            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
        }

        public class MyClass : MyClassBase
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public delegate object MyFuncDef(string name);

        [Fact]
        public void Test__CustomMetadataInMemberInit()
        {
            Expression<MyFuncDef> expr = name => new MyClass
            {
                Type = "xpto",
                Name = name
            };

            var js = expr.CompileToJavascript(
                    new JavascriptCompilationOptions((JsCompilationFlags)0, MemberInitAsJson.ForAllTypes));

            Assert.Equal("function(name){return {type:\"xpto\",name:name};}", js);
        }
    }
}