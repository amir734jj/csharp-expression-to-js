using System;
using System.Linq.Expressions;
using core.Plugins;
using Xunit;

namespace core.Tests
{
    public class StaticStringMethodsTests
    {
        [Fact]
        public void StringConcat0()
        {
            Expression<Func<MyClass, string>> expr = o => string.Concat("A", "B");

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            Assert.Equal("\"A\"+\"B\"", js);
        }

        [Fact]
        public void StringConcat1()
        {
            Expression<Func<MyClass, string>> expr = o => string.Concat(1, 2);

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            Assert.Equal("''+1+2", js);
        }

        [Fact]
        public void StringEmpty()
        {
            Expression<Func<MyClass, string>> expr = o => string.Empty;
            var js = expr.CompileToJavascript();
            Assert.Equal("\"\"", js);
        }

        [Fact]
        public void StringJoin()
        {
            Expression<Func<MyClass, string>> expr = o => string.Join<Phone>(",", o.Phones);

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            Assert.Equal("Phones.join(\",\")", js);
        }

        [Fact]
        public void StringJoin2()
        {
            Expression<Func<MyClass, string>> expr =
                o => string.Join(" ", "Miguel", "Angelo", "Santos", "Bicudo");

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            Assert.Equal("[\"Miguel\",\"Angelo\",\"Santos\",\"Bicudo\"].join(\" \")", js);
        }

        [Fact]
        public void StringConcat()
        {
            Expression<Func<MyClass, string>> expr = o => string.Concat(o.Name, ":", o.Age + 10);

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            Assert.Equal("Name+\":\"+(Age+10)", js);
        }

        [Fact]
        public void StringConcatContains()
        {
            Expression<Func<MyClass, bool>> expr = o => string.Concat(o.Name, ":", o.Age + 10).Contains("30");

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            Assert.Equal("(Name+\":\"+(Age+10)).indexOf(\"30\")>=0", js);

            js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter,
                    ScriptVersion.Es60, new StaticStringMethods()));

            Assert.Equal("(Name+\":\"+(Age+10)).includes(\"30\")", js);
        }

        [Fact]
        public void StringConcat2()
        {
            Expression<Func<MyClass, string>> expr = o => string.Concat(10, ":", 20);

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            Assert.Equal("''+10+\":\"+20", js);
        }
    }
}