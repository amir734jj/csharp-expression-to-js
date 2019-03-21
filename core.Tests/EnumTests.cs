using System;
using System.Linq.Expressions;
using core.Plugins;
using Xunit;

namespace core.Tests
{
    public class EnumTests
    {
        [Fact]
        public void EnumAsInteger0()
        {
            Expression<Func<SomeFlagsEnum>> expr = () => 0;
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            Assert.Equal(@"0", js);
        }

        [Fact]
        public void EnumAsInteger1()
        {
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A;
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            Assert.Equal(@"1", js);
        }

        [Fact]
        public void EnumCompareAsInteger0()
        {
            Expression<Func<MyClassWithEnum, bool>> expr = o => o.SomeFlagsEnum == 0;
            var js = expr.CompileToJavascript();
            Assert.Equal(@"SomeFlagsEnum===0", js);
        }

        [Fact]
        public void EnumCompareAsInteger1()
        {
            Expression<Func<MyClassWithEnum, bool>> expr = o => o.SomeFlagsEnum == SomeFlagsEnum.A;
            var js = expr.CompileToJavascript();
            Assert.Equal(@"SomeFlagsEnum===1", js);
        }

        [Fact]
        public void EnumCompareAsInteger2()
        {
            Expression<Func<MyClassWithEnum, bool>> expr = o => o.SomeFlagsEnum == (SomeFlagsEnum.A | SomeFlagsEnum.B);
            var js = expr.CompileToJavascript();
            Assert.Equal(@"SomeFlagsEnum===3", js);
        }

        [Fact]
        public void EnumCompareWithEnclosed()
        {
            var enclosed = SomeFlagsEnum.B;
            Expression<Func<MyClassWithEnum, bool>> expr = o => o.SomeFlagsEnum == (SomeFlagsEnum.A ^ ~enclosed);
            var js = expr.CompileToJavascript();
            Assert.Equal(@"SomeFlagsEnum===(1^~2)", js);
        }

        [Fact]
        public void EnumCallWithEnumParam()
        {
            Expression<Action<MyClassWithEnum>> expr = o => o.SetGender(SomeFlagsEnum.B);
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new MyCustomClassEnumMethods()));
            Assert.Equal(@"o.SetGender(2)", js);
        }

        [Fact]
        public void EnumCallWithEnumParam2()
        {
            Expression<Action<MyClassWithEnum>> expr = o => o.SetGender(SomeFlagsEnum.A | SomeFlagsEnum.B);
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new MyCustomClassEnumMethods()));
            Assert.Equal(@"o.SetGender(3)", js);
        }

        [Fact]
        public void EnumAsString0()
        {
            Expression<Func<SomeFlagsEnum>> expr = () => 0;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.UseStrings)));
            Assert.Equal(@"""""", js);
        }

        [Fact]
        public void EnumAsString1()
        {
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.UseStrings)));
            Assert.Equal(@"""A""", js);
        }

        [Fact]
        public void EnumAsString2()
        {
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A | SomeFlagsEnum.B;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsStringWithSeparator | EnumOptions.UseStrings)));
            Assert.Equal(@"""B|A""", js);
        }

        [Fact]
        public void EnumAsString3()
        {
            Expression<Func<SomeStrangeFlagsEnum>> expr = () => SomeStrangeFlagsEnum.A | SomeStrangeFlagsEnum.B;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsStringWithSeparator | EnumOptions.UseStrings)));
            Assert.Equal(@"""C|1""", js);
        }

        [Fact]
        public void EnumAsArrayOfStrings0()
        {
            Expression<Func<SomeFlagsEnum>> expr = () => 0;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsArray | EnumOptions.UseStrings)));
            Assert.Equal(@"[]", js);
        }

        [Fact]
        public void EnumAsArrayOfStrings1()
        {
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsArray | EnumOptions.UseStrings)));
            Assert.Equal(@"[""A""]", js);
        }

        [Fact]
        public void EnumAsArrayOfStrings2()
        {
            Expression<Func<SomeFlagsEnum>> expr = () => SomeFlagsEnum.A | SomeFlagsEnum.B;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.FlagsAsArray | EnumOptions.UseStrings)));
            Assert.Equal(@"[""B"",""A""]", js);
        }

        [Fact]
        public void EnumAsStringEquals()
        {
            Expression<Func<MyClassWithEnum, bool>> expr = doc => doc.SomeFlagsEnum == SomeFlagsEnum.B;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.UseStrings)));
            Assert.Equal(@"doc.SomeFlagsEnum===""B""", js);
        }

        [Fact]
        public void EnumAsStringNotEquals()
        {
            Expression<Func<MyClassWithEnum, bool>> expr = doc => doc.SomeFlagsEnum != SomeFlagsEnum.B;
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly,
                    new EnumConversionExtension(EnumOptions.UseStrings)));
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

    class MyClassWithEnum : MyClass
    {
        public SomeFlagsEnum SomeFlagsEnum { get; }
        public SomeLongEnum SomeLongEnum { get; }
        public SomeUnorderedFlagsEnum SomeUnorderedFlagsEnum { get; }
        public void SetGender(SomeFlagsEnum someFlagsEnum) { }
    }

    [Flags]
    enum SomeFlagsEnum
    {
        A = 1,
        B = 2
    }

    [Flags]
    enum SomeStrangeFlagsEnum
    {
        A = 0x011,
        B = 0x101,
        C = 0x110
    }

    enum SomeLongEnum : long
    {
        A = 1,
        B = 2
    }

    [Flags]
    enum SomeUnorderedFlagsEnum
    {
        AB = 3,
        A = 1,
        B = 2
    }
}