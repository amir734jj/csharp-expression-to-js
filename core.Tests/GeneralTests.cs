using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Xunit;

namespace core.Tests
{
    public class GeneralTests
    {
        [Fact]
        public void Test__FuncWithScopeArgsWithBody()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new { x.Age, x.Name, x.Phones };
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.ScopeParameter));
            
            // Assert
            Assert.Equal("function(Age,Name,Phones){return {Age:Age,Name:Name,Phones:Phones};}", js);
        }

        [Fact]
        public void Test__FuncWithScopeArgsWoBody()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new { x.Age, x.Name, x.Phones };
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.ScopeParameter | JsCompilationFlags.BodyOnly));
            
            // Assert
            Assert.Equal("{Age:Age,Name:Name,Phones:Phones}", js);
        }

        [Fact]
        public void Test__FuncWoScopeArgsWithBody()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new { x.Age, x.Name, x.Phones };
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(0));
            
            // Assert
            Assert.Equal("function(x){return {Age:x.Age,Name:x.Name,Phones:x.Phones};}", js);
        }

        [Fact]
        public void Test__Test__FuncWoScopeArgsWoBody()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new { x.Age, x.Name, x.Phones };
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            
            // Assert
            Assert.Equal("{Age:x.Age,Name:x.Name,Phones:x.Phones}", js);
        }

        [Fact]
        public void Test__Test__LinqWhere()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.Phones.Where(p => p.Ddd == 21);
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("System.Linq.Enumerable.Where(Phones,function(p){return p.DDD===21;})", js);
        }

        [Fact]
        public void Test__LinqCount()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.Phones.Count();
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("System.Linq.Enumerable.Count(Phones)", js);
        }

        [Fact]
        public void Test__LinqFirstOrDefault()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.Phones.FirstOrDefault(p => p.Ddd > 10);
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("System.Linq.Enumerable.FirstOrDefault(Phones,function(p){return p.DDD>10;})", js);
        }

        [Fact]
        public void Test__ArrayLength()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.Phones.Length;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("Phones.length", js);
        }

        [Fact]
        public void Test__ArrayIndex()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.Phones[10];
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("Phones[10]", js);
        }

        [Fact]
        public void Test__ListCount()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => ((IList<Phone>)x.Phones).Count;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("Phones.length", js);
        }

        [Fact]
        public void Test__DictionaryItem()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.PhonesByName["Miguel"];
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("PhonesByName[\"Miguel\"]", js);
        }

        [Fact]
        public void Test__DictionaryContainsKey()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.PhonesByName.ContainsKey("Miguel");
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("PhonesByName.hasOwnProperty(\"Miguel\")", js);
        }

        [Fact]
        public void Test__OrElseOperator()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.PhonesByName["Miguel"].Ddd == 32 || x.Phones.Length != 1;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("PhonesByName[\"Miguel\"].DDD===32||Phones.length!==1", js);
        }

        [Fact]
        public void Test__OrOperator()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => x.PhonesByName["Miguel"].Ddd == 32 | x.Phones.Length != 1;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("PhonesByName[\"Miguel\"].DDD===32|Phones.length!==1", js);
        }

        [Fact]
        public void Test__InlineNewDictionary1()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new Dictionary<string, string>
                {
                    { "name", "Miguel" },
                    { "age", "30" },
                    { "birth-date", "1984-05-04" }
                };
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("{name:\"Miguel\",age:\"30\",\"birth-date\":\"1984-05-04\"}", js);
        }

        [Fact]
        public void Test__InlineNewDictionary2()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new Hashtable
                {
                    { "name", "Miguel" },
                    { "age", 30 },
                    { "birth-date", "1984-05-04" }
                };
            
            // Act
            var js = expr.CompileToJavascript();
            Assert.Equal("{name:\"Miguel\",age:30,\"birth-date\":\"1984-05-04\"}", js);
        }

        [Fact]
        public void Test__InlineNewObject()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new
            {
                name = "Miguel",
                age = 30,
                birthDate = "1984-05-04"
            };
            
            // Act
            var js = expr.CompileToJavascript();
            Assert.Equal("{name:\"Miguel\",age:30,birthDate:\"1984-05-04\"}", js);
        }

        [Fact]
        public void Test__InlineNewArray1()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new[] { 1, 2, 3 };
            
            // Act
            var js = expr.CompileToJavascript();
            Assert.Equal("[1,2,3]", js);
        }

        [Fact]
        public void Test__InlineNewArray2()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new object[] { 1, 2, 3, "Miguel" };
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("[1,2,3,\"Miguel\"]", js);
        }

        [Fact]
        public void Test__InlineNewArray3()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new object[] { 1, 2, 3, "Miguel", (Func<int>)(() => 20) };
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("[1,2,3,\"Miguel\",function(){return 20;}]", js);
        }

        [Fact]
        public void Test__InlineNewArray4()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new[]
                {
                    new[] { 1, 2 },
                    new[] { 3, 4 }
                };
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("[[1,2],[3,4]]", js);
        }

        [Fact]
        public void Test__InlineNewList1()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new List<int> { 1, 2, 3 };
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("[1,2,3]", js);
        }

        [Fact]
        public void Test__InlineNewList2()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new ArrayList { 1, 2, 3 };
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("[1,2,3]", js);
        }

        [Fact]
        public void Test__InlineNewMultipleThings()
        {
            // Arrange
            Expression<Func<MyClass, object>> expr = x => new object[]
                {
                    new Dictionary<string, object>
                        {
                            { "name", "Miguel" },
                            { "age", 30 },
                            { "func", (Func<int, double>)(y => (y + 10) * 0.5) },
                            { "list", new List<string> { "a", "b", "c" } }
                        },
                    new
                        {
                            name = "André",
                            age = 30,
                            func = (Func<int, int>)(z => z + 5),
                            list = new List<int> { 10, 20, 30 }
                        }
                };
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"[{name:""Miguel"",age:30,func:function(y){return (y+10)*0.5;},list:[""a"",""b"",""c""]},{name:""André"",age:30,func:function(z){return z+5;},list:[10,20,30]}]", js);
        }

        [Fact]
        public void Test__ArrowFunctionOneArg()
        {
            // Arrange
            Expression<Func<int, int>> expr = x => 1024 + x;
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(0, ScriptVersion.Es60));
            
            // Assert
            Assert.Equal(@"x=>1024+x", js);
        }

        [Fact]
        public void Test__ArrowFunctionManyArgs()
        {
            // Arrange
            Expression<Func<int, int, int>> expr = (x, y) => y + x;
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(0, ScriptVersion.Es60));
            
            // Assert
            Assert.Equal(@"(x,y)=>y+x", js);
        }

        [Fact]
        public void Test__ArrowFunctionNoArgs()
        {
            // Arrange
            Expression<Func<int>> expr = () => 1024;
            
            // Act
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(0, ScriptVersion.Es60));
            
            // Assert
            Assert.Equal(@"()=>1024", js);
        }

        [Fact]
        public void Test__Regex1()
        {
            // Arrange
            Expression<Func<Regex>> expr = () => new Regex(@"^\d{4}-\d\d-\d\d$", RegexOptions.IgnoreCase);
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"/^\d{4}-\d\d-\d\d$/gi", js);
        }

        [Fact]
        public void Test__Regex1B()
        {
            // Arrange
            Expression<Func<Regex>> expr = () => new Regex(@"^\d{4}-\d\d-\d\d$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"/^\d{4}-\d\d-\d\d$/gim", js);
        }

        [Fact]
        public void Test__Regex2()
        {
            // Arrange
            Expression<Func<Func<string, Regex>>> expr = () => p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"function(p){return new RegExp(p,'gim');}", js);
        }

/*        [Fact()]
        [ExpectedException(typeof(NotSupportedException))]
        public void Regex3()
        {
            Expression<Func<Func<string, RegexOptions, Regex>>> expr = () => ((p, o) => new Regex(p, o | RegexOptions.Multiline));
            var js = expr.Body.CompileToJavascript();
            //Assert.AreEqual(@"function(p,o){return new RegExp(p,'g'+o+'m');}", js);
        }*/

        [Fact]
        public void Test__StringCompare1()
        {
            // Arrange
            Expression<Func<Func<string, string, int>>> expr = () => string.Compare;
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"function(s,b){return System.String.Compare(s,b);}", js);
        }

        [Fact]
        public void Test__StringContains()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.Contains("Miguel");
            
            // Act
            var js = expr.CompileToJavascript();
            Assert.Equal("Name.indexOf(\"Miguel\")>=0", js);
            js = expr.CompileToJavascript(ScriptVersion.Es60);
            Assert.Equal("Name.includes(\"Miguel\")", js);
        }

        [Fact]
        public void Test__StringContains2()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => "Miguel Angelo Santos Bicudo".Contains(o.Name);
            var js = expr.CompileToJavascript();
            Assert.Equal("(\"Miguel Angelo Santos Bicudo\").indexOf(Name)>=0", js);
            js = expr.CompileToJavascript(ScriptVersion.Es60);
            Assert.Equal("(\"Miguel Angelo Santos Bicudo\").includes(Name)", js);
        }

        [Fact]
        public void Test__StringStartsWith()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.StartsWith("Test");
            
            // Act
            var js = expr.CompileToJavascript(ScriptVersion.Es60);
            
            // Assert
            Assert.Equal("Name.startsWith(\"Test\")", js);
        }

        [Fact]
        public void Test__StringEndsWith()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.EndsWith("Test");
            
            // Act
            var js = expr.CompileToJavascript(ScriptVersion.Es60);
            
            // Assert
            Assert.Equal("Name.endsWith(\"Test\")", js);
        }

        [Fact]
        public void Test__StringToLower()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.ToLower() == "test";
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("Name.toLowerCase()===\"test\"", js);
        }

        [Fact]
        public void Test__StringToUpper()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.ToUpper() == "TEST";
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("Name.toUpperCase()===\"TEST\"", js);
        }

        [Fact]
        public void Test__StringTrim()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.Trim() == "test";
            
            // Act
            var js = expr.CompileToJavascript(ScriptVersion.Es51);
            
            // Assert
            Assert.Equal("Name.trim()===\"test\"", js);
        }

        [Fact]
        public void Test__StringTrimStart()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.TrimStart() == "test";
            
            // Act
            var js = expr.CompileToJavascript(ScriptVersion.Es50.NonStandard());
            
            // Assert
            Assert.Equal("Name.trimLeft()===\"test\"", js);
        }

        [Fact]
        public void Test__StringTrimEnd()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.TrimEnd() == "test";
            
            // Act
            var js = expr.CompileToJavascript(ScriptVersion.Es50.NonStandard());
            
            // Assert
            Assert.Equal("Name.trimRight()===\"test\"", js);
        }

        [Fact]
        public void Test__StringSubString()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.Substring(1) == "est";
            
            // Assert
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("Name.substring(1)===\"est\"", js);
        }

        [Fact]
        public void Test__StringPadLeft()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.PadLeft(1) == "est";
            
            // Act
            var js = expr.CompileToJavascript(ScriptVersion.Es80);
            
            // Assert
            Assert.Equal("Name.padStart(1)===\"est\"", js);
        }

        [Fact]
        public void Test__StringPadRight()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.PadRight(1) == "est";
            
            // Act
            var js = expr.CompileToJavascript(ScriptVersion.Es80);
            
            // Assert
            Assert.Equal("Name.padEnd(1)===\"est\"", js);
        }

        [Fact]
        public void Test__StringLastIndexOf()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.LastIndexOf("!", StringComparison.Ordinal) == 1;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("Name.lastIndexOf(\"!\")===1", js);
        }

        [Fact]
        public void Test__StringIndexOf()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Name.IndexOf("!", StringComparison.Ordinal) == 1;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("Name.indexOf(\"!\")===1", js);
        }

        [Fact]
        public void Test__StringIndexer1()
        {
            // Arrange
            Expression<Func<string, char>> expr = s => s[0];
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("s[0]", js);
        }

        [Fact]
        public void Test__StringIndexer2()
        {
            // Arrange
            Expression<Func<string, char>> expr = s => "MASB"[0];
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal("(\"MASB\")[0]", js);
        }

        [Fact]
        public void Test__NumLiteralToString1()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString();
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toString()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringD()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("D");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toString()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringE()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("E");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toExponential()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringE4()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("E4");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toExponential(4)", js);
        }

        [Fact]
        public void Test__NumLiteralToStringF()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("F");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toFixed()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringF4()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("F4");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toFixed(4)", js);
        }

        [Fact]
        public void Test__NumLiteralToStringG()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("G");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toFixed()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringG4()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("G4");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toFixed(4)", js);
        }

        [Fact]
        public void Test__NumLiteralToStringN()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("N");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toLocaleString()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringN4()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("N4");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toLocaleString(undefined,{minimumFractionDigits:4})", js);
        }

        [Fact]
        public void Test__NumLiteralToStringX()
        {
            // Arrange
            Expression<Func<string>> expr = () => 1.ToString("X");
            
            // Act
            var js = expr.Body.CompileToJavascript();
            
            // Assert
            Assert.Equal("(1).toString(16)", js);
        }

        [Fact]
        public void Test__StringAdd1()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => o.Name + ":" + 10;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"Name+"":""+10", js);
        }

        [Fact]
        public void Test__StringAdd2()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => o.Name + ":" + (o.Age + 10);
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"Name+"":""+(Age+10)", js);
        }

        [Fact]
        public void Test__StringAdd3()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => 1.5 + o.Name + ":" + (o.Age + 10);
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"1.5+Name+"":""+(Age+10)", js);
        }

        [Fact]
        public void Test__StringAdd4()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => 1.5 + o.Age + ":" + o.Name;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"(1.5+Age)+"":""+Name", js);
        }

/*        [Fact]
        public void CorrectUsageOfScopeParameterFlag()
        {
            Expression<Func<int>> expr = () => 1;
            Exception getEx = null;
            try
            {
                var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.ScopeParameter));
            }
            catch (Exception ex)
            {
                getEx = ex;
            }

            Assert.IsInstanceOfType(getEx, typeof(InvalidOperationException));
        }*/

/*        [Fact]
        public void CannotSerializeUnknownConstant()
        {
            Expression<Func<Phone>> expr = Expression.Lambda<Func<Phone>>(Expression.Constant(new Phone()));
            Exception getEx = null;
            try
            {
                var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            }
            catch (Exception ex)
            {
                getEx = ex;
            }

            Assert.IsInstanceOfType(getEx, typeof(NotSupportedException));
        }*/

        [Fact]
        public void Test__UseEnclosedValues()
        {
            // Arrange
            var value = 0;
            Expression<Func<int>> expr = () => value;
            
            // Act
            value = 1;
            var js1 = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            value = 2;
            var js2 = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            
            // Assert
            Assert.Equal(@"1", js1);
            Assert.Equal(@"2", js2);
        }

        [Fact]
        public void Test__ConditionalCheck1()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => o.Phones.Length > 0 && o.Age < 50 ? o.Name + " " + o.Phones.Length + " has phones and is " + o.Age + "yo" : "ignore";
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"Phones.length>0&&Age<50?Name+"" ""+Phones.length+"" has phones and is ""+Age+""yo"":""ignore""", js);
        }

        [Fact]
        public void Test__ConditionalCheck2()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => o.Phones.Length > 0 ? o.Age < 50 ? "a" : "b" : "c";
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"Phones.length>0?(Age<50?""a"":""b""):""c""", js);
        }

        [Fact]
        public void Test__ConditionalCheck3()
        {
            // Arrange
            Expression<Func<MyClass, int>> expr = o => o.Phones.Length > 0 ? 1 : o.Age < 50 ? 2 : 3;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"Phones.length>0?1:Age<50?2:3", js);
        }

        [Fact]
        public void Test__ConditionalCheck4()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => o.Phones.Length > 0 ? "a" : o.Age < 50 ? "b" : "c";
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"Phones.length>0?""a"":Age<50?""b"":""c""", js);
        }

        [Fact]
        public void Test__ConditionalCheck5()
        {
            // Arrange
            Expression<Func<MyClass, int>> expr = o => (o.Phones.Length > 0 ? 1 : 2) + 10;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"(Phones.length>0?1:2)+10", js);
        }

        [Fact]
        public void Test__ConditionalCheck6()
        {
            // Arrange
            Expression<Func<MyClass, string>> expr = o => (o.Phones.Length > 0 ? 1 : 2).ToString();
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"(Phones.length>0?1:2).toString()", js);
        }

        [Fact]
        public void Test__ConditionalCheck7()
        {
            // Arrange
            Expression<Func<MyClass, int>> expr = o => (o.Phones.Length == 0 ? 1 : o.Phones.Length == 1 ? 2 : 3) * 5;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"(Phones.length===0?1:Phones.length===1?2:3)*5", js);
        }

        [Fact]
        public void Test__ConditionalCheck8()
        {
            // Arrange
            const int x = 10;
            Expression<Func<MyClass, int>> expr = o => o.Phones.Length == 0 ? 1 + x : 2 + x;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            // TODO: this case could be optimized
            Assert.Equal(@"Phones.length===0?1+10:2+10", js);
        }

        [Fact]
        public void Test__CanCompareToNullable()
        {
            // Arrange
            Expression<Func<MyClass, bool>> expr = o => o.Count == 1;
            
            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"Count===1", js);
        }

        [Fact]
        public void Test__BooleanConstants()
        {
            // Arrange
            Expression<Func<object, bool?>> expr1 = x => true;
            Expression<Func<object, bool?>> expr2 = x => false;
            
            // Act
            var js1 = expr1.CompileToJavascript();
            var js2 = expr2.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"true", js1);
            Assert.Equal(@"false", js2);
        }

        [Fact]
        public void Test__BooleanNullableConstants()
        {
            // Arrange
            Expression<Func<string, bool?>> expr =
                x =>
                    x == "true" ? (bool?)true :
                    x == "false" ? (bool?)false :
                                   (bool?)null;

            // Act
            var js = expr.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"x===""true""?true:x===""false""?false:null", js);
        }

        [Fact]
        public void Test__CharConstant_a()
        {
            // Arrange
            Expression<Func<object, char>> expr1 = x => 'a';
            
            // Act
            var js1 = expr1.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"""a""", js1);
        }

        [Fact]
        public void Test__CharConstant_ch0()
        {
            // Arrange
            Expression<Func<object, char>> expr1 = x => '\0';
            
            // Act
            var js1 = expr1.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"""\0""", js1);
        }

        [Fact]
        public void Test__CharConstantInt_a()
        {
            // Arrange
            Expression<Func<object, int>> expr1 = x => 'a';
            
            // Act
            var js1 = expr1.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"97", js1);
        }

        [Fact]
        public void Test__CharConstantInt_ch0()
        {
            // Arrange
            Expression<Func<object, int>> expr1 = x => '\0';
            
            // Act
            var js1 = expr1.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"0", js1);
        }

        [Fact]
        public void Test__CharConstant_Sum()
        {
            // Arrange
            Expression<Func<char, int>> expr1 = x => x + 'a';
            
            // Act
            var js1 = expr1.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"x+97", js1);
        }

        [Fact]
        public void CharConstant_Concat()
        {
            // Arrange
            Expression<Func<string, string>> expr1 = x => x + 'a';
            
            // Act
            var js1 = expr1.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"x+""a""", js1);
        }

        [Fact]
        public void Char2Number()
        {
            // Arrange
            Expression<Func<char, short>> expr1 = x => (short)x;
            
            // Act
            var js1 = expr1.CompileToJavascript();
            
            // Assert
            Assert.Equal(@"x", js1);
        }
    }

    internal class MyClass
    {
        public Phone[] Phones { get; set; }
        public Dictionary<string, Phone> PhonesByName { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int? Count { get; set; }
    }

    internal class Phone
    {
        public int Ddd { get; set; }
    }
}
