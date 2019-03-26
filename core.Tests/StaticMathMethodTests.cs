using System;
using System.Linq.Expressions;
using core.Enums;
using core.Plugins;
using Xunit;

namespace core.Tests
{
    public class StaticMathMethodTests
    {
        [Fact]
        public void Test__MathPow()
        {
            // Arrange
            Expression<Func<MyClass, double>> expr = o => Math.Pow(o.Age, 2.0);

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            // Assert
            Assert.Equal("Math.pow(Age,2)", js);
        }

        [Fact]
        public void Test__MathLog()
        {
            // Arrange
            Expression<Func<MyClass, double>> expr = o => Math.Log(o.Age) + 1;

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            // Assert
            Assert.Equal("Math.log(Age)+1", js);
        }

        [Fact]
        public void Test__MathLog2Args()
        {
            // Arrange
            Expression<Func<MyClass, double>> expr = o => Math.Log(o.Age, 2.0) + 1;

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            // Assert
            Assert.Equal("Math.log(Age)/Math.log(2)+1", js);
        }

        [Fact]
        public void Test__MathRound()
        {
            // Arrange
            Expression<Func<MyClass, double>> expr = o => Math.Round(o.Age / 0.7);

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods(true)));

            // Assert
            Assert.Equal("Math.round(Age/0.7)", js);
        }

        [Fact]
        public void Test__MathRound2Args()
        {
            // Arrange
            Expression<Func<MyClass, double>> expr = o => Math.Round(o.Age / 0.7, 2);

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods(true)));

            // Assert
            Assert.Equal("(function(a,b){return Math.round(a*b)/b;})(Age/0.7,Math.pow(10,2))", js);
        }

        [Fact]
        public void Test__MathE()
        {
            // Arrange
            Expression<Func<MyClass, double>> expr = o => Math.E;

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            // Assert
            Assert.Equal("Math.E", js);
        }

        [Fact]
        public void Test__MathPi()
        {
            // Arrange
            Expression<Func<MyClass, double>> expr = o => Math.PI;

            // Act
            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            // Assert
            Assert.Equal("Math.PI", js);
        }
    }
}