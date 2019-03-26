# csharp-expression-to-js

This is an ExpressionTree (lambda) to Javascript converter.

Examples
-------

Converting lambda with boolean and numeric operations:

```csharp
    Expression<Func<MyClass, object>> expr = x => x.PhonesByName["Amir"].DDD == 32 || x.Phones.Length != 1;
    var js = expr.CompileToJavascript();
    // js = PhonesByName["Amir"].DDD===32||Phones.length!==1
```

Converting lambda with LINQ expression, containing a inner lambda:

```csharp
    Expression<Func<MyClass, object>> expr = x => x.Phones.FirstOrDefault(p => p.DDD > 10);
    var js = expr.CompileToJavascript();
    // js = System.Linq.Enumerable.FirstOrDefault(Phones,function(p){return p.DDD>10;})
```

Converting lambda with Linq `Select` method:

```csharp
    Expression<Func<string[], IEnumerable<char>>> expr = array => array.Select(x => x[0]);
    var js = expr.CompileToJavascript(
        new JavascriptCompilationOptions(
            JsCompilationFlags.BodyOnly | JsCompilationFlags.ScopeParameter,
            new[] { new LinqMethods(), }));
    // js = array.map(function(x){return x[0];})
```

Clone using `ToArray` and targeting ES6:

```csharp
    Expression<Func<string[], IEnumerable<string>>> expr = array => array.ToArray();
    var js = expr.Body.CompileToJavascript(
        ScriptVersion.Es60,
        new JavascriptCompilationOptions(new LinqMethods()));
    // js = [...array]
```

Developing custom plugins
--------

# Plugins

This library supports plugins that allows customizing the conversion process.

We can tell the compiler that you want to use plugins when calling `CompileToJavascript` method.

Example
-------

Custom JavaScript output when calling a method in a custom class:

```csharp
    public class MyCustomClassMethods : JavascriptConversionExtension
    {
        public override void ConvertToJavascript(JavascriptConversionContext context)
        {
            var methodCall = context.Node as MethodCallExpression;
            if (methodCall != null)
                // This is needed to prevent default method translation
                context.PreventDefault ();
                if (methodCall.Method.DeclaringType == typeof(MyCustomClass))
                {
                    switch (methodCall.Method.Name)
                    {
                        case "GetValue":
                        {
                            using (context.Operation(JavascriptOperationTypes.Call))
                            {
                                using (context.Operation(JavascriptOperationTypes.IndexerProperty))
                                    context.Write("Xpto.GetValue");

                                context.WriteManyIsolated('(', ')', ',', methodCall.Arguments);
                            }

                            return;
                        }
                    }
                }
        }
    }
```
