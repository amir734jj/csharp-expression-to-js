using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using core.Plugins;
using Xunit;

namespace core.Tests
{
    public class LinqMethodsTests
    {
        [Fact]
        public void Test__LinqWhere1()
        {
            Expression<Func<string[], IEnumerable<string>>> expr = array => array.Where(x => x == "Miguel");

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new LinqMethods()));

            Assert.Equal("array.filter(function(x){return x===\"Miguel\";})", js);
        }

        [Fact]
        public void Test__LinqSelect1()
        {
            Expression<Func<string[], IEnumerable<char>>> expr = array => array.Select(x => x[0]);

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new LinqMethods()));

            Assert.Equal("array.map(function(x){return x[0];})", js);
        }

        [Fact]
        public void Test__LinqToArray1()
        {
            Expression<Func<string[], IEnumerable<string>>> expr = array => array.ToArray();

            var js = expr.CompileToJavascript(
                new JavascriptCompilationOptions(
                    JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter, new LinqMethods()));

            Assert.Equal("array.slice()", js);
        }

        [Fact]
        public void Test__LinqToArrayEs6()
        {
            Expression<Func<string[], IEnumerable<string>>> expr = array => array.ToArray();

            var js = expr.Body.CompileToJavascript(
                new JavascriptCompilationOptions(
                    ScriptVersion.Es60,
                    new LinqMethods()));

            Assert.Equal("[...array]", js);
        }
    }
}