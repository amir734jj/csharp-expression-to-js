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
            Expression<Func<MyClass, object>> expr = x => new { x.Age, x.Name, x.Phones };
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.ScopeParameter));
            Assert.Equal("function(Age,Name,Phones){return {Age:Age,Name:Name,Phones:Phones};}", js);
        }

        [Fact]
        public void Test__FuncWithScopeArgsWoBody()
        {
            Expression<Func<MyClass, object>> expr = x => new { x.Age, x.Name, x.Phones };
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.ScopeParameter | JsCompilationFlags.BodyOnly));
            Assert.Equal("{Age:Age,Name:Name,Phones:Phones}", js);
        }

        [Fact]
        public void Test__FuncWoScopeArgsWithBody()
        {
            Expression<Func<MyClass, object>> expr = x => new { x.Age, x.Name, x.Phones };
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(0));
            Assert.Equal("function(x){return {Age:x.Age,Name:x.Name,Phones:x.Phones};}", js);
        }

        [Fact]
        public void Test__Test__FuncWoScopeArgsWoBody()
        {
            Expression<Func<MyClass, object>> expr = x => new { x.Age, x.Name, x.Phones };
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            Assert.Equal("{Age:x.Age,Name:x.Name,Phones:x.Phones}", js);
        }

        [Fact]
        public void Test__Test__LinqWhere()
        {
            Expression<Func<MyClass, object>> expr = x => x.Phones.Where(p => p.Ddd == 21);
            var js = expr.CompileToJavascript();
            Assert.Equal("System.Linq.Enumerable.Where(Phones,function(p){return p.DDD===21;})", js);
        }

        [Fact]
        public void Test__LinqCount()
        {
            Expression<Func<MyClass, object>> expr = x => x.Phones.Count();
            var js = expr.CompileToJavascript();
            Assert.Equal("System.Linq.Enumerable.Count(Phones)", js);
        }

        [Fact]
        public void Test__LinqFirstOrDefault()
        {
            Expression<Func<MyClass, object>> expr = x => x.Phones.FirstOrDefault(p => p.Ddd > 10);
            var js = expr.CompileToJavascript();
            Assert.Equal("System.Linq.Enumerable.FirstOrDefault(Phones,function(p){return p.DDD>10;})", js);
        }

        [Fact]
        public void Test__ArrayLength()
        {
            Expression<Func<MyClass, object>> expr = x => x.Phones.Length;
            var js = expr.CompileToJavascript();
            Assert.Equal("Phones.length", js);
        }

        [Fact]
        public void Test__ArrayIndex()
        {
            Expression<Func<MyClass, object>> expr = x => x.Phones[10];
            var js = expr.CompileToJavascript();
            Assert.Equal("Phones[10]", js);
        }

        [Fact]
        public void Test__ListCount()
        {
            Expression<Func<MyClass, object>> expr = x => ((IList<Phone>)x.Phones).Count;
            var js = expr.CompileToJavascript();
            Assert.Equal("Phones.length", js);
        }

        [Fact]
        public void Test__DictionaryItem()
        {
            Expression<Func<MyClass, object>> expr = x => x.PhonesByName["Miguel"];
            var js = expr.CompileToJavascript();
            Assert.Equal("PhonesByName[\"Miguel\"]", js);
        }

        [Fact]
        public void Test__DictionaryContainsKey()
        {
            Expression<Func<MyClass, object>> expr = x => x.PhonesByName.ContainsKey("Miguel");
            var js = expr.CompileToJavascript();
            Assert.Equal("PhonesByName.hasOwnProperty(\"Miguel\")", js);
        }

        [Fact]
        public void Test__OrElseOperator()
        {
            Expression<Func<MyClass, object>> expr = x => x.PhonesByName["Miguel"].Ddd == 32 || x.Phones.Length != 1;
            var js = expr.CompileToJavascript();
            Assert.Equal("PhonesByName[\"Miguel\"].DDD===32||Phones.length!==1", js);
        }

        [Fact]
        public void Test__OrOperator()
        {
            Expression<Func<MyClass, object>> expr = x => x.PhonesByName["Miguel"].Ddd == 32 | x.Phones.Length != 1;
            var js = expr.CompileToJavascript();
            Assert.Equal("PhonesByName[\"Miguel\"].DDD===32|Phones.length!==1", js);
        }

        [Fact]
        public void Test__InlineNewDictionary1()
        {
            Expression<Func<MyClass, object>> expr = x => new Dictionary<string, string>
                {
                    { "name", "Miguel" },
                    { "age", "30" },
                    { "birth-date", "1984-05-04" }
                };
            var js = expr.CompileToJavascript();
            Assert.Equal("{name:\"Miguel\",age:\"30\",\"birth-date\":\"1984-05-04\"}", js);
        }

        [Fact]
        public void Test__InlineNewDictionary2()
        {
            Expression<Func<MyClass, object>> expr = x => new Hashtable
                {
                    { "name", "Miguel" },
                    { "age", 30 },
                    { "birth-date", "1984-05-04" }
                };
            var js = expr.CompileToJavascript();
            Assert.Equal("{name:\"Miguel\",age:30,\"birth-date\":\"1984-05-04\"}", js);
        }

        [Fact]
        public void Test__InlineNewObject()
        {
            Expression<Func<MyClass, object>> expr = x => new
            {
                name = "Miguel",
                age = 30,
                birthDate = "1984-05-04"
            };
            var js = expr.CompileToJavascript();
            Assert.Equal("{name:\"Miguel\",age:30,birthDate:\"1984-05-04\"}", js);
        }

        [Fact]
        public void Test__InlineNewArray1()
        {
            Expression<Func<MyClass, object>> expr = x => new[] { 1, 2, 3 };
            var js = expr.CompileToJavascript();
            Assert.Equal("[1,2,3]", js);
        }

        [Fact]
        public void Test__InlineNewArray2()
        {
            Expression<Func<MyClass, object>> expr = x => new object[] { 1, 2, 3, "Miguel" };
            var js = expr.CompileToJavascript();
            Assert.Equal("[1,2,3,\"Miguel\"]", js);
        }

        [Fact]
        public void Test__InlineNewArray3()
        {
            Expression<Func<MyClass, object>> expr = x => new object[] { 1, 2, 3, "Miguel", (Func<int>)(() => 20) };
            var js = expr.CompileToJavascript();
            Assert.Equal("[1,2,3,\"Miguel\",function(){return 20;}]", js);
        }

        [Fact]
        public void Test__InlineNewArray4()
        {
            Expression<Func<MyClass, object>> expr = x => new[]
                {
                    new[] { 1, 2 },
                    new[] { 3, 4 }
                };
            var js = expr.CompileToJavascript();
            Assert.Equal("[[1,2],[3,4]]", js);
        }

        [Fact]
        public void Test__InlineNewList1()
        {
            Expression<Func<MyClass, object>> expr = x => new List<int> { 1, 2, 3 };
            var js = expr.CompileToJavascript();
            Assert.Equal("[1,2,3]", js);
        }

        [Fact]
        public void Test__InlineNewList2()
        {
            Expression<Func<MyClass, object>> expr = x => new ArrayList { 1, 2, 3 };
            var js = expr.CompileToJavascript();
            Assert.Equal("[1,2,3]", js);
        }

        [Fact]
        public void Test__InlineNewMultipleThings()
        {
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
            var js = expr.CompileToJavascript();
            Assert.Equal(@"[{name:""Miguel"",age:30,func:function(y){return (y+10)*0.5;},list:[""a"",""b"",""c""]},{name:""André"",age:30,func:function(z){return z+5;},list:[10,20,30]}]", js);
        }

        [Fact]
        public void Test__ArrowFunctionOneArg()
        {
            Expression<Func<int, int>> expr = x => 1024 + x;
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(0, ScriptVersion.Es60));
            Assert.Equal(@"x=>1024+x", js);
        }

        [Fact]
        public void Test__ArrowFunctionManyArgs()
        {
            Expression<Func<int, int, int>> expr = (x, y) => y + x;
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(0, ScriptVersion.Es60));
            Assert.Equal(@"(x,y)=>y+x", js);
        }

        [Fact]
        public void Test__ArrowFunctionNoArgs()
        {
            Expression<Func<int>> expr = () => 1024;
            var js = expr.CompileToJavascript(new JavascriptCompilationOptions(0, ScriptVersion.Es60));
            Assert.Equal(@"()=>1024", js);
        }

        [Fact]
        public void Test__Regex1()
        {
            Expression<Func<Regex>> expr = () => new Regex(@"^\d{4}-\d\d-\d\d$", RegexOptions.IgnoreCase);
            var js = expr.Body.CompileToJavascript();
            Assert.Equal(@"/^\d{4}-\d\d-\d\d$/gi", js);
        }

        [Fact]
        public void Test__Regex1B()
        {
            Expression<Func<Regex>> expr = () => new Regex(@"^\d{4}-\d\d-\d\d$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var js = expr.Body.CompileToJavascript();
            Assert.Equal(@"/^\d{4}-\d\d-\d\d$/gim", js);
        }

        [Fact]
        public void Test__Regex2()
        {
            Expression<Func<Func<string, Regex>>> expr = () => (p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Multiline));
            var js = expr.Body.CompileToJavascript();
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
            Expression<Func<Func<string, string, int>>> expr = () => ((s, b) => string.Compare(s, b));
            var js = expr.Body.CompileToJavascript();
            Assert.Equal(@"function(s,b){return System.String.Compare(s,b);}", js);
        }

        [Fact]
        public void Test__StringContains()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.Contains("Miguel");
            var js = expr.CompileToJavascript();
            Assert.Equal("Name.indexOf(\"Miguel\")>=0", js);
            js = expr.CompileToJavascript(ScriptVersion.Es60);
            Assert.Equal("Name.includes(\"Miguel\")", js);
        }

        [Fact]
        public void Test__StringContains2()
        {
            Expression<Func<MyClass, bool>> expr = o => "Miguel Angelo Santos Bicudo".Contains(o.Name);
            var js = expr.CompileToJavascript();
            Assert.Equal("(\"Miguel Angelo Santos Bicudo\").indexOf(Name)>=0", js);
            js = expr.CompileToJavascript(ScriptVersion.Es60);
            Assert.Equal("(\"Miguel Angelo Santos Bicudo\").includes(Name)", js);
        }

        [Fact]
        public void Test__StringStartsWith()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.StartsWith("Test");
            var js = expr.CompileToJavascript(ScriptVersion.Es60);
            Assert.Equal("Name.startsWith(\"Test\")", js);
        }

        [Fact]
        public void Test__StringEndsWith()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.EndsWith("Test");
            var js = expr.CompileToJavascript(ScriptVersion.Es60);
            Assert.Equal("Name.endsWith(\"Test\")", js);
        }

        [Fact]
        public void Test__StringToLower()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.ToLower() == "test";
            var js = expr.CompileToJavascript();
            Assert.Equal("Name.toLowerCase()===\"test\"", js);
        }

        [Fact]
        public void Test__StringToUpper()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.ToUpper() == "TEST";
            var js = expr.CompileToJavascript();
            Assert.Equal("Name.toUpperCase()===\"TEST\"", js);
        }

        [Fact]
        public void Test__StringTrim()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.Trim() == "test";
            var js = expr.CompileToJavascript(ScriptVersion.Es51);
            Assert.Equal("Name.trim()===\"test\"", js);
        }

        [Fact]
        public void Test__StringTrimStart()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.TrimStart() == "test";
            var js = expr.CompileToJavascript(ScriptVersion.Es50.NonStandard());
            Assert.Equal("Name.trimLeft()===\"test\"", js);
        }

        [Fact]
        public void Test__StringTrimEnd()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.TrimEnd() == "test";
            var js = expr.CompileToJavascript(ScriptVersion.Es50.NonStandard());
            Assert.Equal("Name.trimRight()===\"test\"", js);
        }

        [Fact]
        public void Test__StringSubString()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.Substring(1) == "est";
            var js = expr.CompileToJavascript();
            Assert.Equal("Name.substring(1)===\"est\"", js);
        }

        [Fact]
        public void Test__StringPadLeft()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.PadLeft(1) == "est";
            var js = expr.CompileToJavascript(ScriptVersion.Es80);
            Assert.Equal("Name.padStart(1)===\"est\"", js);
        }

        [Fact]
        public void Test__StringPadRight()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.PadRight(1) == "est";
            var js = expr.CompileToJavascript(ScriptVersion.Es80);
            Assert.Equal("Name.padEnd(1)===\"est\"", js);
        }

        [Fact]
        public void Test__StringLastIndexOf()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.LastIndexOf("!") == 1;
            var js = expr.CompileToJavascript();
            Assert.Equal("Name.lastIndexOf(\"!\")===1", js);
        }

        [Fact]
        public void Test__StringIndexOf()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Name.IndexOf("!") == 1;
            var js = expr.CompileToJavascript();
            Assert.Equal("Name.indexOf(\"!\")===1", js);
        }

        [Fact]
        public void Test__StringIndexer1()
        {
            Expression<Func<string, char>> expr = s => s[0];
            var js = expr.CompileToJavascript();
            Assert.Equal("s[0]", js);
        }

        [Fact]
        public void Test__StringIndexer2()
        {
            Expression<Func<string, char>> expr = s => "MASB"[0];
            var js = expr.CompileToJavascript();
            Assert.Equal("(\"MASB\")[0]", js);
        }

        [Fact]
        public void Test__NumLiteralToString1()
        {
            Expression<Func<string>> expr = () => 1.ToString();
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toString()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringD()
        {
            Expression<Func<string>> expr = () => 1.ToString("D");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toString()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringE()
        {
            Expression<Func<string>> expr = () => 1.ToString("E");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toExponential()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringE4()
        {
            Expression<Func<string>> expr = () => 1.ToString("E4");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toExponential(4)", js);
        }

        [Fact]
        public void Test__NumLiteralToStringF()
        {
            Expression<Func<string>> expr = () => 1.ToString("F");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toFixed()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringF4()
        {
            Expression<Func<string>> expr = () => 1.ToString("F4");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toFixed(4)", js);
        }

        [Fact]
        public void Test__NumLiteralToStringG()
        {
            Expression<Func<string>> expr = () => 1.ToString("G");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toFixed()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringG4()
        {
            Expression<Func<string>> expr = () => 1.ToString("G4");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toFixed(4)", js);
        }

        [Fact]
        public void Test__NumLiteralToStringN()
        {
            Expression<Func<string>> expr = () => 1.ToString("N");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toLocaleString()", js);
        }

        [Fact]
        public void Test__NumLiteralToStringN4()
        {
            Expression<Func<string>> expr = () => 1.ToString("N4");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toLocaleString(undefined,{minimumFractionDigits:4})", js);
        }

        [Fact]
        public void Test__NumLiteralToStringX()
        {
            Expression<Func<string>> expr = () => 1.ToString("X");
            var js = expr.Body.CompileToJavascript();
            Assert.Equal("(1).toString(16)", js);
        }

        [Fact]
        public void Test__StringAdd1()
        {
            Expression<Func<MyClass, string>> expr = o => o.Name + ":" + 10;
            var js = expr.CompileToJavascript();
            Assert.Equal(@"Name+"":""+10", js);
        }

        [Fact]
        public void Test__StringAdd2()
        {
            Expression<Func<MyClass, string>> expr = o => o.Name + ":" + (o.Age + 10);
            var js = expr.CompileToJavascript();
            Assert.Equal(@"Name+"":""+(Age+10)", js);
        }

        [Fact]
        public void Test__StringAdd3()
        {
            Expression<Func<MyClass, string>> expr = o => 1.5 + o.Name + ":" + (o.Age + 10);
            var js = expr.CompileToJavascript();
            Assert.Equal(@"1.5+Name+"":""+(Age+10)", js);
        }

        [Fact]
        public void Test__StringAdd4()
        {
            Expression<Func<MyClass, string>> expr = o => 1.5 + o.Age + ":" + o.Name;
            var js = expr.CompileToJavascript();
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
            var value = 0;
            Expression<Func<int>> expr = () => value;
            value = 1;
            var js1 = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            value = 2;
            var js2 = expr.CompileToJavascript(new JavascriptCompilationOptions(JsCompilationFlags.BodyOnly));
            Assert.Equal(@"1", js1);
            Assert.Equal(@"2", js2);
        }

        [Fact]
        public void Test__ConditionalCheck1()
        {
            Expression<Func<MyClass, string>> expr = o => o.Phones.Length > 0 && o.Age < 50 ? o.Name + " " + o.Phones.Length + " has phones and is " + o.Age + "yo" : "ignore";
            var js = expr.CompileToJavascript();
            Assert.Equal(@"Phones.length>0&&Age<50?Name+"" ""+Phones.length+"" has phones and is ""+Age+""yo"":""ignore""", js);
        }

        [Fact]
        public void Test__ConditionalCheck2()
        {
            Expression<Func<MyClass, string>> expr = o => o.Phones.Length > 0 ? (o.Age < 50 ? "a" : "b") : "c";
            var js = expr.CompileToJavascript();
            Assert.Equal(@"Phones.length>0?(Age<50?""a"":""b""):""c""", js);
        }

        [Fact]
        public void Test__ConditionalCheck3()
        {
            Expression<Func<MyClass, int>> expr = o => o.Phones.Length > 0 ? 1 : (o.Age < 50 ? 2 : 3);
            var js = expr.CompileToJavascript();
            Assert.Equal(@"Phones.length>0?1:Age<50?2:3", js);
        }

        [Fact]
        public void Test__ConditionalCheck4()
        {
            Expression<Func<MyClass, string>> expr = o => o.Phones.Length > 0 ? "a" : (o.Age < 50 ? "b" : "c");
            var js = expr.CompileToJavascript();
            Assert.Equal(@"Phones.length>0?""a"":Age<50?""b"":""c""", js);
        }

        [Fact]
        public void Test__ConditionalCheck5()
        {
            Expression<Func<MyClass, int>> expr = o => (o.Phones.Length > 0 ? 1 : 2) + 10;
            var js = expr.CompileToJavascript();
            Assert.Equal(@"(Phones.length>0?1:2)+10", js);
        }

        [Fact]
        public void Test__ConditionalCheck6()
        {
            Expression<Func<MyClass, string>> expr = o => (o.Phones.Length > 0 ? 1 : 2).ToString();
            var js = expr.CompileToJavascript();
            Assert.Equal(@"(Phones.length>0?1:2).toString()", js);
        }

        [Fact]
        public void Test__ConditionalCheck7()
        {
            Expression<Func<MyClass, int>> expr = o => (o.Phones.Length == 0 ? 1 : (o.Phones.Length == 1 ? 2 : 3)) * 5;
            var js = expr.CompileToJavascript();
            Assert.Equal(@"(Phones.length===0?1:Phones.length===1?2:3)*5", js);
        }

        [Fact]
        public void Test__ConditionalCheck8()
        {
            const int x = 10;
            Expression<Func<MyClass, int>> expr = o => o.Phones.Length == 0 ? 1 + x : 2 + x;
            var js = expr.CompileToJavascript();
            // TODO: this case could be optimized
            Assert.Equal(@"Phones.length===0?1+10:2+10", js);
        }

        [Fact]
        public void Test__CanCompareToNullable()
        {
            Expression<Func<MyClass, bool>> expr = o => o.Count == 1;
            var js = expr.CompileToJavascript();
            Assert.Equal(@"Count===1", js);
        }

        [Fact]
        public void Test__BooleanConstants()
        {
            Expression<Func<object, bool?>> expr1 = x => true;
            Expression<Func<object, bool?>> expr2 = x => false;
            var js1 = expr1.CompileToJavascript();
            var js2 = expr2.CompileToJavascript();
            Assert.Equal(@"true", js1);
            Assert.Equal(@"false", js2);
        }

        [Fact]
        public void Test__BooleanNullableConstants()
        {
            Expression<Func<string, bool?>> expr =
                x =>
                    x == "true" ? (bool?)true :
                    x == "false" ? (bool?)false :
                                   (bool?)null;

            var js = expr.CompileToJavascript();
            Assert.Equal(@"x===""true""?true:x===""false""?false:null", js);
        }

        [Fact]
        public void Test__CharConstant_a()
        {
            Expression<Func<object, char>> expr1 = x => 'a';
            var js1 = expr1.CompileToJavascript();
            Assert.Equal(@"""a""", js1);
        }

        [Fact]
        public void Test__CharConstant_ch0()
        {
            Expression<Func<object, char>> expr1 = x => '\0';
            var js1 = expr1.CompileToJavascript();
            Assert.Equal(@"""\0""", js1);
        }

        [Fact]
        public void Test__CharConstantInt_a()
        {
            Expression<Func<object, int>> expr1 = x => 'a';
            var js1 = expr1.CompileToJavascript();
            Assert.Equal(@"97", js1);
        }

        [Fact]
        public void Test__CharConstantInt_ch0()
        {
            Expression<Func<object, int>> expr1 = x => '\0';
            var js1 = expr1.CompileToJavascript();
            Assert.Equal(@"0", js1);
        }

        [Fact]
        public void Test__CharConstant_Sum()
        {
            Expression<Func<char, int>> expr1 = x => x + 'a';
            var js1 = expr1.CompileToJavascript();
            Assert.Equal(@"x+97", js1);
        }

        [Fact]
        public void CharConstant_Concat()
        {
            Expression<Func<string, string>> expr1 = x => x + 'a';
            var js1 = expr1.CompileToJavascript();
            Assert.Equal(@"x+""a""", js1);
        }

        [Fact]
        public void Char2Number()
        {
            Expression<Func<char, short>> expr1 = x => (short)x;
            var js1 = expr1.CompileToJavascript();
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
