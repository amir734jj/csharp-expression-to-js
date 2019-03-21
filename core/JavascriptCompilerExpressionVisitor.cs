using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

#pragma warning disable 1591
namespace core
{
    /// <summary>
    /// Expression visitor that converts each node to JavaScript code.
    /// </summary>
    internal sealed class JavascriptCompilerExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression contextParameter;
        private readonly IEnumerable<JavascriptConversionExtension> extensions;
        private readonly JavascriptWriter resultWriter = new JavascriptWriter();
        private List<string> usedScopeMembers;

        public JavascriptCompilerExpressionVisitor(
            ParameterExpression contextParameter,
            IEnumerable<JavascriptConversionExtension> extensions,
            [NotNull] JavascriptCompilationOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            this.contextParameter = contextParameter;
            this.extensions = extensions;
        }

        /// <summary>
        /// Gets the user options.
        /// </summary>
        [NotNull]
        public JavascriptCompilationOptions Options { get; private set; }

        /// <summary>
        /// Gets the resulting JavaScript code.
        /// </summary>
        public string Result => resultWriter.ToString();

        /// <summary>
        /// Gets the scope names that were used from the scope parameter.
        /// </summary>
        [CanBeNull]
        public string[] UsedScopeMembers => usedScopeMembers?.ToArray();

        public override Expression Visit(Expression node)
        {
            var node2 = PreprocessNode(node);
            JavascriptConversionContext context = null;
            foreach (var each in extensions)
            {
                context = context ?? new JavascriptConversionContext(node2, this, resultWriter, Options);

                each.ConvertToJavascript(context);

                if (context.preventDefault)
                {
                    // canceling any further action with the current node
                    return node2;
                }
            }

            // nothing happened, continue to the default conversion behavior
            return base.Visit(node2);
        }

        private Expression PreprocessNode(Expression node)
        {
            if (node.NodeType == ExpressionType.Equal
                || node.NodeType == ExpressionType.Or
                || node.NodeType == ExpressionType.And
                || node.NodeType == ExpressionType.ExclusiveOr
                || node.NodeType == ExpressionType.OrAssign
                || node.NodeType == ExpressionType.AndAssign
                || node.NodeType == ExpressionType.ExclusiveOrAssign
                || node.NodeType == ExpressionType.NotEqual)
            {
                var binary = (BinaryExpression)node;
                var leftVal = binary.Left is UnaryExpression left && (left.NodeType == ExpressionType.Convert || left.NodeType == ExpressionType.ConvertChecked) ? left.Operand : binary.Left;
                var rightVal = binary.Right is UnaryExpression right && (right.NodeType == ExpressionType.Convert || right.NodeType == ExpressionType.ConvertChecked) ? right.Operand : binary.Right;
                if (rightVal.Type != leftVal.Type)
                {
                    if (leftVal.Type.IsEnum && TypeHelpers.IsNumericType(rightVal.Type) && rightVal.NodeType == ExpressionType.Constant)
                    {
                        rightVal = Expression.Convert(
                            Expression.Constant(Enum.ToObject(leftVal.Type, ((ConstantExpression)rightVal).Value)),
                            rightVal.Type);
                        leftVal = binary.Left;
                    }
                    else if (rightVal.Type.IsEnum && TypeHelpers.IsNumericType(leftVal.Type) && leftVal.NodeType == ExpressionType.Constant)
                    {
                        leftVal = Expression.Convert(
                            Expression.Constant(Enum.ToObject(rightVal.Type, ((ConstantExpression)leftVal).Value)),
                            leftVal.Type);
                        rightVal = binary.Right;
                    }
                    else
                    {
                        return node;
                    }

                    return Expression.MakeBinary(node.NodeType, leftVal, rightVal);
                }
            }

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                {
                    Visit(node.Left);
                    resultWriter.Write('[');
                    using (resultWriter.Operation(0))
                        Visit(node.Right);
                    resultWriter.Write(']');
                    return node;
                }
            }

            using (resultWriter.Operation(node))
            {
                Visit(node.Left);
                resultWriter.WriteOperator(node.NodeType, node.Type);
                Visit(node.Right);
            }

            return node;
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            return node;
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            using (resultWriter.Operation(JavascriptOperationTypes.TernaryOp))
            {
                using (resultWriter.Operation(JavascriptOperationTypes.TernaryTest))
                    Visit(node.Test);

                resultWriter.Write('?');

                using (resultWriter.Operation(JavascriptOperationTypes.TernaryTrueValue))
                    Visit(node.IfTrue);

                resultWriter.Write(':');

                using (resultWriter.Operation(JavascriptOperationTypes.TernaryFalseValue))
                    Visit(node.IfFalse);

                return node;
            }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (TypeHelpers.IsNumericType(node.Type))
            {
                using (resultWriter.Operation(JavascriptOperationTypes.Literal))
                    resultWriter.Write(Convert.ToString(node.Value, CultureInfo.InvariantCulture));
            }
            else if (node.Type == typeof(bool))
            {
                using (resultWriter.Operation(JavascriptOperationTypes.Literal))
                    resultWriter.Write((bool)node.Value ? "true" : "false");
            }
            else if (node.Type == typeof(string))
            {
                using (resultWriter.Operation(JavascriptOperationTypes.Literal))
                    WriteStringLiteral((string)node.Value);
            }
            else if (node.Type == typeof(char))
            {
                using (resultWriter.Operation(JavascriptOperationTypes.Literal))
                    WriteStringLiteral(node.Value.ToString());
            }
            else if (node.Value == null)
            {
                resultWriter.Write("null");
            }
            else if (node.Type.IsEnum)
            {
                using (resultWriter.Operation(JavascriptOperationTypes.Literal))
                {
                    var underlyingType = Enum.GetUnderlyingType(node.Type);
                    resultWriter.Write(Convert.ChangeType(node.Value, underlyingType, CultureInfo.InvariantCulture));
                }
            }
            else if (node.Type == typeof(Regex))
            {
                using (resultWriter.Operation(JavascriptOperationTypes.Literal))
                {
                    resultWriter.Write('/');
                    resultWriter.Write(node.Value);
                    resultWriter.Write("/g");
                }
            }
            else if (node.Type.IsClosureRootType())
            {
                // do nothing, this is a reference to the closure root object
            }
            else
                throw new NotSupportedException("The used constant value is not supported: `" + node + "` (" + node.Type.Name + ")");

            return node;
        }

        private void WriteStringLiteral(string str)
        {
            resultWriter.Write('"');
            resultWriter.Write(
                str
                    .Replace("\\", "\\\\")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n")
                    .Replace("\t", "\\t")
                    .Replace("\0", "\\0")
                    .Replace("\"", "\\\""));

            resultWriter.Write('"');
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            return node;
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            return node;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            return node;
        }

        protected override Expression VisitExtension(Expression node)
        {
            return node;
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            return node;
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            return node;
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return node;
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            return node;
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            // Ecma script 6+: rendering arrow function syntax
            // Other: rendering inline annonimous function
            if (Options.ScriptVersion.Supports(JavascriptSyntaxFeature.ArrowFunction))
            {
                // Arrow function syntax and precedence works mostly like an assignment.
                using (resultWriter.Operation(JavascriptOperationTypes.AssignRhs))
                {
                    var pars = node.Parameters;
                    if (pars.Count != 1)
                        resultWriter.Write("(");

                    var posStart = resultWriter.Length;
                    foreach (var param in node.Parameters)
                    {
                        if (param.IsByRef)
                            throw new NotSupportedException("Cannot pass by ref in javascript.");

                        if (resultWriter.Length > posStart)
                            resultWriter.Write(',');

                        resultWriter.Write(param.Name);
                    }

                    if (pars.Count != 1)
                        resultWriter.Write(")");

                    resultWriter.Write("=>");

                    using (resultWriter.Operation(JavascriptOperationTypes.ParamIsolatedLhs))
                    {
                        Visit(node.Body);
                    }
                }
            }
            else
            {
                using (resultWriter.Operation(node))
                {
                    resultWriter.Write("function(");

                    var posStart = resultWriter.Length;
                    foreach (var param in node.Parameters)
                    {
                        if (param.IsByRef)
                            throw new NotSupportedException("Cannot pass by ref in javascript.");

                        if (resultWriter.Length > posStart)
                            resultWriter.Write(',');

                        resultWriter.Write(param.Name);
                    }

                    resultWriter.Write("){");
                    if (node.ReturnType != typeof(void))
                        using (resultWriter.Operation(0))
                        {
                            resultWriter.Write("return ");
                            Visit(node.Body);
                        }
                    else
                        using (resultWriter.Operation(0))
                        {
                            Visit(node.Body);
                        }

                    resultWriter.Write(";}");
                }
            }
            return node;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            // Detecting a new dictionary
            if (TypeHelpers.IsDictionaryType(node.Type))
            {
                using (resultWriter.Operation(0))
                {
                    resultWriter.Write('{');

                    var posStart = resultWriter.Length;
                    foreach (var init in node.Initializers)
                    {
                        if (resultWriter.Length > posStart)
                            resultWriter.Write(',');

                        if (init.Arguments.Count != 2)
                            throw new NotSupportedException(
                                "Objects can only be initialized with methods that receive pairs: key -> name");

                        var nameArg = init.Arguments[0];
                        if (nameArg.NodeType != ExpressionType.Constant || nameArg.Type != typeof(string))
                            throw new NotSupportedException("The key of an object must be a constant string value");

                        var name = (string)((ConstantExpression)nameArg).Value;
                        if (Regex.IsMatch(name, @"^\w[\d\w]*$"))
                            resultWriter.Write(name);
                        else
                            WriteStringLiteral(name);

                        resultWriter.Write(':');
                        Visit(init.Arguments[1]);
                    }

                    resultWriter.Write('}');
                }

                return node;
            }

            // Detecting a new dictionary
            if (TypeHelpers.IsListType(node.Type))
            {
                using (resultWriter.Operation(0))
                {
                    resultWriter.Write('[');

                    var posStart = resultWriter.Length;
                    foreach (var init in node.Initializers)
                    {
                        if (resultWriter.Length > posStart)
                            resultWriter.Write(',');

                        if (init.Arguments.Count != 1)
                            throw new Exception(
                                "Arrays can only be initialized with methods that receive a single parameter for the value");

                        Visit(init.Arguments[0]);
                    }

                    resultWriter.Write(']');
                }

                return node;
            }

            return node;
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == null)
            {
                var decl = node.Member.DeclaringType;
                if (decl == typeof(string))
                {
                    if (node.Member.Name == "Empty")
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.Literal))
                            resultWriter.Write("\"\"");
                        return node;
                    }
                }
            }

            var isClosure = false;
            using (resultWriter.Operation(node))
            {
                var metadataProvider = Options.GetMetadataProvider();
                var pos = resultWriter.Length;
                if (node.Expression == null)
                {
                    var decl = node.Member.DeclaringType;
                    if (decl != null)
                    {
                        // TODO: there should be a way to customize the name of types through metadata
                        resultWriter.Write(decl.FullName);
                        resultWriter.Write('.');
                        resultWriter.Write(decl.Name);
                    }
                }
                else if (node.Expression.Type.IsClosureRootType())
                {
                    isClosure = true;
                }
                else if (node.Expression != contextParameter)
                    Visit(node.Expression);
                else
                {
                    usedScopeMembers = usedScopeMembers ?? new List<string>();
                    var meta = metadataProvider.GetMemberMetadata(node.Member);
                    Debug.Assert(!string.IsNullOrEmpty(meta?.MemberName), "!string.IsNullOrEmpty(meta?.MemberName)");
                    usedScopeMembers.Add(meta?.MemberName ?? node.Member.Name);
                }

                if (resultWriter.Length > pos)
                    resultWriter.Write('.');

                if (!isClosure)
                {
                    var propInfo = node.Member as PropertyInfo;
                    if (propInfo?.DeclaringType != null
                        && node.Type == typeof(int)
                        && node.Member.Name == "Count"
                        && TypeHelpers.IsListType(propInfo.DeclaringType))
                    {
                        resultWriter.Write("length");
                    }
                    else
                    {
                        var meta = metadataProvider.GetMemberMetadata(node.Member);
                        Debug.Assert(!string.IsNullOrEmpty(meta?.MemberName), "!string.IsNullOrEmpty(meta?.MemberName)");
                        resultWriter.Write(meta?.MemberName);
                    }
                }
            }

            if (isClosure)
            {
                var cte = ((ConstantExpression)node.Expression).Value;
                var value = ((FieldInfo)node.Member).GetValue(cte);
                Visit(Expression.Constant(value, node.Type));
            }

            return node;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            using (resultWriter.Operation(node))
            {
                var isPostOp = JsOperationHelper.IsPostfixOperator(node.NodeType);

                if (!isPostOp)
                    resultWriter.WriteOperator(node.NodeType, node.Type);
                Visit(node.Operand);
                if (isPostOp)
                    resultWriter.WriteOperator(node.NodeType, node.Type);

                return node;
            }
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return node;
        }

        protected override Expression VisitTry(TryExpression node)
        {
            return node;
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            return node;
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            return node;
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            resultWriter.Write(node.Name);
            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            using (resultWriter.Operation(0))
            {
                resultWriter.Write('[');

                var posStart = resultWriter.Length;
                foreach (var item in node.Expressions)
                {
                    if (resultWriter.Length > posStart)
                        resultWriter.Write(',');

                    Visit(item);
                }

                resultWriter.Write(']');
            }

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            // Detecting inlineable objects
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (node.Members != null && node.Members.Count > 0)
            {
                using (resultWriter.Operation(0))
                {
                    resultWriter.Write('{');

                    var posStart = resultWriter.Length;
                    for (var itMember = 0; itMember < node.Members.Count; itMember++)
                    {
                        var member = node.Members[itMember];
                        if (resultWriter.Length > posStart)
                            resultWriter.Write(',');

                        if (Regex.IsMatch(member.Name, @"^\w[\d\w]*$"))
                            resultWriter.Write(member.Name);
                        else
                            WriteStringLiteral(member.Name);

                        resultWriter.Write(':');
                        Visit(node.Arguments[itMember]);
                    }

                    resultWriter.Write('}');
                }
            }
            else if (node.Type != typeof(Regex))
            {
                using (resultWriter.Operation(0))
                {
                    resultWriter.Write("new");
                    resultWriter.Write(' ');
                    using (resultWriter.Operation(JavascriptOperationTypes.Call))
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                            resultWriter.Write(node.Type.FullName.Replace('+', '.'));

                        resultWriter.Write('(');

                        var posStart = resultWriter.Length;
                        foreach (var argExpr in node.Arguments)
                        {
                            if (resultWriter.Length > posStart)
                                resultWriter.Write(',');

                            Visit(argExpr);
                        }

                        resultWriter.Write(')');
                    }
                }
            }
            else
            {
                // To run the regex use this code:
                // var lambda = Expression.Lambda<Func<Regex>>(node);

                // if all parameters are constant
                if (node.Arguments.All(a => a.NodeType == ExpressionType.Constant))
                {
                    resultWriter.Write('/');

                    var pattern = (string)((ConstantExpression)node.Arguments[0]).Value;
                    resultWriter.Write(pattern);
                    var args = node.Arguments.Count;

                    resultWriter.Write('/');
                    resultWriter.Write('g');
                    RegexOptions options = 0;
                    if (args == 2)
                    {
                        options = (RegexOptions)((ConstantExpression)node.Arguments[1]).Value;

                        if ((options & RegexOptions.IgnoreCase) != 0)
                            resultWriter.Write('i');
                        if ((options & RegexOptions.Multiline) != 0)
                            resultWriter.Write('m');
                    }

                    // creating a Regex object with `ECMAScript` to make sure the pattern is valid in JavaScript.
                    // If it is not valid, then an exception is thrown.
                    // ReSharper disable once UnusedVariable
                    var ecmaRegex = new Regex(pattern, options | RegexOptions.ECMAScript);
                }
                else
                {
                    using (resultWriter.Operation(JavascriptOperationTypes.New))
                    {
                        resultWriter.Write("new RegExp(");

                        using (resultWriter.Operation(JavascriptOperationTypes.ParamIsolatedLhs))
                            Visit(node.Arguments[0]);

                        var args = node.Arguments.Count;

                        if (args == 2)
                        {
                            resultWriter.Write(',');

                            if (!(node.Arguments[1] is ConstantExpression optsConst))
                                throw new NotSupportedException("The options parameter of a Regex must be constant");

                            var options = (RegexOptions)optsConst.Value;

                            resultWriter.Write('\'');
                            resultWriter.Write('g');
                            if ((options & RegexOptions.IgnoreCase) != 0)
                                resultWriter.Write('i');
                            if ((options & RegexOptions.Multiline) != 0)
                                resultWriter.Write('m');
                            resultWriter.Write('\'');
                        }

                        resultWriter.Write(')');
                    }
                }
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsSpecialName)
            {
                var isIndexer = node.Method.Name == "get_Item" || node.Method.Name == "get_Chars";
                if (isIndexer)
                {
                    using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                    {
                        Visit(node.Object);
                        resultWriter.Write('[');

                        using (resultWriter.Operation(0))
                        {
                            var posStart0 = resultWriter.Length;
                            foreach (var arg in node.Arguments)
                            {
                                if (resultWriter.Length != posStart0)
                                    resultWriter.Write(',');

                                Visit(arg);
                            }
                        }

                        resultWriter.Write(']');
                        return node;
                    }
                }

                if (node.Method.Name == "set_Item")
                {
                    using (resultWriter.Operation(0))
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.AssignRhs))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                            {
                                Visit(node.Object);
                                resultWriter.Write('[');

                                using (resultWriter.Operation(0))
                                {
                                    var posStart0 = resultWriter.Length;
                                    foreach (var arg in node.Arguments)
                                    {
                                        if (resultWriter.Length != posStart0)
                                            resultWriter.Write(',');

                                        Visit(arg);
                                    }
                                }

                                resultWriter.Write(']');
                            }
                        }

                        resultWriter.Write('=');
                        Visit(node.Arguments.Single());
                    }

                    return node;
                }
            }
            else
            {
                if (node.Method.DeclaringType != null
                    && (node.Method.Name == "ContainsKey"
                        && TypeHelpers.IsDictionaryType(node.Method.DeclaringType)))
                {
                    using (resultWriter.Operation(JavascriptOperationTypes.Call))
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                            Visit(node.Object);
                        resultWriter.Write(".hasOwnProperty(");
                        using (resultWriter.Operation(0))
                            Visit(node.Arguments.Single());
                        resultWriter.Write(')');
                        return node;
                    }
                }
            }

            if (node.Method.DeclaringType == typeof(string))
            {
                if (node.Method.Name == "Contains")
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_includes))
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".includes(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');
                            return node;
                        }
                    }

                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_indexOf))
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.Comparison))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.Call))
                            {
                                using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                    Visit(node.Object);
                                resultWriter.Write(".indexOf(");
                                using (resultWriter.Operation(0))
                                {
                                    var posStart = resultWriter.Length;
                                    foreach (var arg in node.Arguments)
                                    {
                                        if (resultWriter.Length > posStart)
                                            resultWriter.Write(',');
                                        Visit(arg);
                                    }
                                }

                                resultWriter.Write(')');
                            }

                            resultWriter.Write(">=0");
                            return node;
                        }
                    }
                }
                else if (node.Method.Name == nameof(string.StartsWith))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_startsWith))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".startsWith(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');
                            return node;

                        }
                }
                else if (node.Method.Name == nameof(string.EndsWith))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_endsWith))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".endsWith(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');
                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.ToLower))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_toLowerCase))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".toLowerCase(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');

                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.ToUpper))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_toUpperCase))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".toUpperCase(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');

                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.Trim))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_trim))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".trim(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');

                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.TrimEnd))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_trimRight))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".trimRight(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                            }

                            resultWriter.Write(')');

                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.TrimStart))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_trimLeft))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".trimLeft(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                            }

                            resultWriter.Write(')');

                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.Substring))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_substring))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".substring(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');

                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.PadLeft))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_padStart))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".padStart(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');

                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.PadRight))
                {
                    if (Options.ScriptVersion.Supports(JavascriptApiFeature.String_prototype_padEnd))
                        using (resultWriter.Operation(JavascriptOperationTypes.Call))
                        {
                            using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                                Visit(node.Object);
                            resultWriter.Write(".padEnd(");
                            using (resultWriter.Operation(0))
                            {
                                var posStart = resultWriter.Length;
                                foreach (var arg in node.Arguments)
                                {
                                    if (resultWriter.Length > posStart)
                                        resultWriter.Write(',');
                                    Visit(arg);
                                }
                            }

                            resultWriter.Write(')');

                            return node;
                        }
                }
                else if (node.Method.Name == nameof(string.LastIndexOf))
                {
                    using (resultWriter.Operation(JavascriptOperationTypes.Call))
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                            Visit(node.Object);
                        resultWriter.Write(".lastIndexOf(");
                        using (resultWriter.Operation(0))
                        {
                            var posStart = resultWriter.Length;
                            foreach (var arg in node.Arguments)
                            {
                                if (resultWriter.Length > posStart)
                                    resultWriter.Write(',');
                                Visit(arg);
                            }
                        }

                        resultWriter.Write(')');

                        return node;
                    }
                }
                else if (node.Method.Name == nameof(string.IndexOf))
                {
                    using (resultWriter.Operation(JavascriptOperationTypes.Call))
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                            Visit(node.Object);
                        resultWriter.Write(".indexOf(");
                        using (resultWriter.Operation(0))
                        {
                            var posStart = resultWriter.Length;
                            foreach (var arg in node.Arguments)
                            {
                                if (resultWriter.Length > posStart)
                                    resultWriter.Write(',');
                                Visit(arg);
                            }
                        }

                        resultWriter.Write(')');

                        return node;
                    }
                }
                else if (node.Method.Name == nameof(string.Concat))
                {
                    using (resultWriter.Operation(JavascriptOperationTypes.Concat))
                    {
                        if (node.Arguments.Count == 0)
                            resultWriter.Write("''");
                        else
                        {
                            if (node.Arguments[0].Type != typeof(string))
                                resultWriter.Write("''+");

                            var posStart = resultWriter.Length;
                            foreach (var arg in node.Arguments)
                            {
                                if (resultWriter.Length > posStart)
                                    resultWriter.Write('+');
                                Visit(arg);
                            }
                        }

                        return node;
                    }
                }
            }

            if (node.Method.Name == "ToString" && node.Type == typeof(string) && node.Object != null)
            {
                string methodName = null;
                if (node.Arguments.Count == 0 || typeof(IFormatProvider).IsAssignableFrom(node.Arguments[0].Type))
                {
                    methodName = "toString()";
                }
                else if (TypeHelpers.IsNumericType(node.Object.Type)
                         && node.Arguments.Count >= 1
                         && node.Arguments[0].Type == typeof(string)
                         && node.Arguments[0].NodeType == ExpressionType.Constant)
                {
                    var str = (string)((ConstantExpression)node.Arguments[0]).Value;
                    var match = Regex.Match(str, @"^([DEFGNX])(\d*)$", RegexOptions.IgnoreCase);
                    var f = match.Groups[1].Value.ToUpper();
                    var n = match.Groups[2].Value;
                    switch (f)
                    {
                        case "D":
                            methodName = "toString()";
                            break;
                        case "E":
                            methodName = "toExponential(" + n + ")";
                            break;
                        case "F":
                        case "G":
                            methodName = "toFixed(" + n + ")";
                            break;
                        case "N":
                        {
                            var undefined = Options.UndefinedLiteral;
                            methodName = string.IsNullOrEmpty(n) ? "toLocaleString()" : $"toLocaleString({undefined},{{minimumFractionDigits:{n}}})";
                            break;
                        }
                        case "X":
                            methodName = "toString(16)";
                            break;
                    }
                }

                if (methodName != null)
                    using (resultWriter.Operation(JavascriptOperationTypes.Call))
                    {
                        using (resultWriter.Operation(JavascriptOperationTypes.IndexerProperty))
                            Visit(node.Object);
                        resultWriter.WriteFormat(".{0}", methodName);
                        return node;
                    }
            }

            if (!node.Method.IsStatic)
                throw new NotSupportedException(
                    $"By default, Lambda2Js cannot convert custom instance methods, only static ones. `{node.Method.Name}` is not static.");

            using (resultWriter.Operation(JavascriptOperationTypes.Call))
                if (node.Method.DeclaringType != null)
                {
                    resultWriter.Write(node.Method.DeclaringType.FullName);
                    resultWriter.Write('.');
                    resultWriter.Write(node.Method.Name);
                    resultWriter.Write('(');

                    var posStart = resultWriter.Length;
                    using (resultWriter.Operation(0))
                        foreach (var arg in node.Arguments)
                        {
                            if (resultWriter.Length != posStart)
                                resultWriter.Write(',');

                            Visit(arg);
                        }

                    resultWriter.Write(')');

                    return node;
                }

            return node;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return node;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            throw new NotSupportedException("MemberInitExpression is not supported. Converting it requires a custom JavascriptConversionExtension like MemberInitAsJson.");
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return node;
        }
    }
}