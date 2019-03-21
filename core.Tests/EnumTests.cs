using System;
using System.Linq.Expressions;
using core.Plugins;
using Xunit;

namespace core.Tests
{
    public class EnumTests
    {
        [Fact]
        public void Test__EnumAsInteger0()
        {
            // Arrange
            Expression<Func<SomeFlagsEnum>> expr = () => 0;
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            
            // Assert
            Assert.Equal(@"0", js);
        }

        [Fact]
        public void Test__EnumAsInteger1()
        {
            // Arrange
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A;
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            
            // Assert
            Assert.Equal(@"1", js);
        }

        [Fact]
        public void Test__EnumCompareAsInteger0()
        {
            // Arrange
            Expression<Func<MyClassWithEnum, bool>> expr = o => o.SomeFlagsEnum == 0;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"SomeFlagsEnum===0", js);
        }

        [Fact]
        public void Test__EnumCompareAsInteger1()
        {
            // Arrange
            Expression<Func<MyClassWithEnum, bool>> expr = o => o.SomeFlagsEnum == SomeFlagsEnum.A;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"SomeFlagsEnum===1", js);
        }

        [Fact]
        public void Test__EnumCompareAsInteger2()
        {
            // Arrange
            Expression<Func<MyClassWithEnum, bool>> expr = o => o.SomeFlagsEnum == (SomeFlagsEnum.A | SomeFlagsEnum.B);
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"SomeFlagsEnum===3", js);
        }

        [Fact]
        public void Test__EnumCompareWithEnclosed()
        {
            // Arrange
            const SomeFlagsEnum enclosed = SomeFlagsEnum.B;
            Expression<Func<MyClassWithEnum, bool>> expr = o => o.SomeFlagsEnum == (SomeFlagsEnum.A ^ ~enclosed);
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"SomeFlagsEnum===(1^~2)", js);
        }

        [Fact]
        public void Test__EnumCallWithEnumParam()
        {
            // Arrange
            Expression<Action<MyClassWithEnum>> expr = o => o.SetGender(SomeFlagsEnum.B);
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new MyCustomClassEnumMethods()));
            
            // Assert
            Assert.Equal(@"o.SetGender(2)", js);
        }

        [Fact]
        public void Test__EnumCallWithEnumParam2()
        {
            // Arrange
            Expression<Action<MyClassWithEnum>> expr = o => o.SetGender(SomeFlagsEnum.A | SomeFlagsEnum.B);
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new MyCustomClassEnumMethods()));
            
            // Assert
            Assert.Equal(@"o.SetGender(3)", js);
        }

        [Fact]
        public void Test__EnumAsString0()
        {
            // Arrange
            Expression<Func<SomeFlagsEnum>> expr = () => 0;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"""""", js);
        }

        [Fact]
        public void Test__EnumAsString1()
        {
            // Arrange
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"""A""", js);
        }

        [Fact]
        public void Test__EnumAsString2()
        {
            // Arrange
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A | SomeFlagsEnum.B;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsStringWithSeparator | EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"""B|A""", js);
        }

        [Fact]
        public void Test__EnumAsString3()
        {
            // Arrange
            Expression<Func<SomeStrangeFlagsEnum>> expr = () => SomeStrangeFlagsEnum.A | SomeStrangeFlagsEnum.B;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsStringWithSeparator | EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"""C|1""", js);
        }

        [Fact]
        public void Test__EnumAsArrayOfStrings0()
        {
            // Arrange
            Expression<Func<SomeFlagsEnum>> expr = () => 0;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsArray | EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"[]", js);
        }

        [Fact]
        public void Test__EnumAsArrayOfStrings1()
        {
            // Arrange
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsArray | EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"[""A""]", js);
        }

        [Fact]
        public void Test__EnumAsArrayOfStrings2()
        {
            // Arrange
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A | SomeFlagsEnum.B;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsArray | EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"[""B"",""A""]", js);
        }

        [Fact]
        public void Test__EnumAsStringEquals()
        {
            // Arrange
            Expression<Func<MyClassWithEnum, bool>> expr = doc => doc.SomeFlagsEnum == SomeFlagsEnum.B;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"doc.SomeFlagsEnum===""B""", js);
        }

        [Fact]
        public void Test__EnumAsStringNotEquals()
        {
            // Arrange
            Expression<Func<MyClassWithEnum, bool>> expr = doc => doc.SomeFlagsEnum != SomeFlagsEnum.B;
            
            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.UseStrings)));
            
            // Assert
            Assert.Equal(@"doc.SomeFlagsEnum!==""B""", js);
        }
    }

    public class MyCustomClassEnumMethods : JavascriptConversionExtension
    {
        public override void ConvertToJavascript(JavascriptConversionContext context)
        {
            if (context.Node is MethodCallExpression methodCall)
                if (methodCall.Method.DeclaringType == typeof(MyClassWithEnum))
                {
                    switch (methodCall.Method.Name)
                    {
                        case "SetGender":
                            {
                                context.PreventDefault();
                                using (context.Operation(JavascriptOperationTypes.Call))
                                {
                                    using (context.Operation(JavascriptOperationTypes.IndexerProperty))
                                    {
                                        context.WriteNode(methodCall.Object);
                                        context.WriteAccessor("SetGender");
                                    }

                                    context.WriteManyIsolated('(', ')', ',', methodCall.Arguments);
                                }

                                return;
                            }
                    }
                }
        }
    }

    internal class MyClassWithEnum : MyClass
    {
        public SomeFlagsEnum SomeFlagsEnum { get; }
        public SomeLongEnum SomeLongEnum { get; }
        public SomeUnorderedFlagsEnum SomeUnorderedFlagsEnum { get; }
        public void SetGender(SomeFlagsEnum someFlagsEnum) { }
    }

    [Flags]
    internal enum SomeFlagsEnum
    {
        A = 1,
        B = 2
    }

    [Flags]
    internal enum SomeStrangeFlagsEnum
    {
        A = 0x011,
        B = 0x101,
        C = 0x110
    }

    internal enum SomeLongEnum : long
    {
        A = 1,
        B = 2
    }

    [Flags]
    internal enum SomeUnorderedFlagsEnum
    {
        AB = 3,
        A = 1,
        B = 2
    }
}