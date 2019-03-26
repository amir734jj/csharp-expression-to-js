using System;
using System.Linq.Expressions;
using core.Enums;
using core.Plugins;
using Xunit;

namespace core.Tests
{
    public class StaticStringMethodsTests
    {
        [Fact]
        public void Test__StringConcat0()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => string.Concat("A", "B");

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            // Assert
            Assert.Equal("\"A\"+\"B\"", js);
        }

        [Fact]
        public void Test__StringConcat1()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => string.Concat(1, 2);

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            // Assert
            Assert.Equal("''+1+2", js);
        }

        [Fact]
        public void Test__StringEmpty()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => string.Empty;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("\"\"", js);
        }

        [Fact]
        public void Test__StringJoin()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => string.Join<Phone>(",", o.Phones);

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            // Assert
            Assert.Equal("Phones.join(\",\")", js);
        }

        [Fact]
        public void Test__StringJoin2()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr =
                o => string.Join(" ", "Miguel", "Angelo", "Santos", "Bicudo");

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            // Assert
            Assert.Equal("[\"Miguel\",\"Angelo\",\"Santos\",\"Bicudo\"].join(\" \")", js);
        }

        [Fact]
        public void Test__StringConcat()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => string.Concat(o.Name, ":", o.Age + 10);

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            // Assert
            Assert.Equal("Name+\":\"+(Age+10)", js);
        }

        [Fact]
        public void Test__StringConcatContains1()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => string.Concat(o.Name, ":", o.Age + 10).Contains("30");

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            // Assert
            Assert.Equal("(Name+\":\"+(Age+10)).indexOf(\"30\")>=0", js);
        }
        
        [Fact]
        public void Test__StringConcatContains2()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => string.Concat(o.Name, ":", o.Age + 10).Contains("30");

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter,
                    ScriptVersion.Es60, new StaticStringMethods()));

            // Assert
            Assert.Equal("(Name+\":\"+(Age+10)).includes(\"30\")", js);
        }

        [Fact]
        public void Test__StringConcat2()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => string.Concat(10, ":", 20);

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticStringMethods()));

            // Assert
            Assert.Equal("''+10+\":\"+20", js);
        }
    }
}