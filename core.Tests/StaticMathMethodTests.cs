using System;
using System.Linq.Expressions;
using core.Plugins;
using Xunit;

namespace core.Tests
{
    public class StaticMathMethodTests
    {
        [Fact]
        public void Test__MathPow()
        {
            Expression<Func<MyClass, double>> expr = o => Math.Pow(o.Age, 2.0);

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            Assert.Equal("Math.pow(Age,2)", js);
        }

        [Fact]
        public void Test__MathLog()
        {
            Expression<Func<MyClass, double>> expr = o => Math.Log(o.Age) + 1;

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            Assert.Equal("Math.log(Age)+1", js);
        }

        [Fact]
        public void Test__MathLog2Args()
        {
            Expression<Func<MyClass, double>> expr = o => Math.Log(o.Age, 2.0) + 1;

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            Assert.Equal("Math.log(Age)/Math.log(2)+1", js);
        }

        [Fact]
        public void Test__MathRound()
        {
            Expression<Func<MyClass, double>> expr = o => Math.Round(o.Age / 0.7);

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods(true)));

            Assert.Equal("Math.round(Age/0.7)", js);
        }

        [Fact]
        public void Test__MathRound2Args()
        {
            Expression<Func<MyClass, double>> expr = o => Math.Round(o.Age / 0.7, 2);

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods(true)));

            Assert.Equal("(function(a,b){return Math.round(a*b)/b;})(Age/0.7,Math.pow(10,2))", js);
        }

        [Fact]
        public void Test__MathE()
        {
            Expression<Func<MyClass, double>> expr = o => Math.E;

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            Assert.Equal("Math.E", js);
        }

        [Fact]
        public void Test__MathPi()
        {
            Expression<Func<MyClass, double>> expr = o => Math.PI;

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new StaticMathMethods()));

            Assert.Equal("Math.PI", js);
        }
    }
}