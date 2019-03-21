using System;
using System.Linq.Expressions;

namespace core
{
    public class JavascriptConversionContext
    {
        [NotNull]
        private readonly JavascriptWriter _result;

        [NotNull]
        private readonly Expression _node;

        internal bool preventDefault;

        public JavascriptConversionContext(
            [NotNull] Expression node,
            [NotNull] ExpressionVisitor visitor,
            [NotNull] JavascriptWriter result,
            [NotNull] JavascriptCompilationOptions options)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
            Visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _node = node ?? throw new ArgumentNullException(nameof(node));
        }

        /// <summary>
        /// Gets the node being converted.
        /// [Do not set this property, as the setter will be soon removed. Use either `WriteLambda` or `WriteExpression` method.]
        /// </summary>
        /// <remarks>
        /// The preferred way to process another node, instead of setting this property,
        /// is calling either `WriteLambda` or `WriteExpression` method.
        /// </remarks>
        [NotNull]
        public Expression Node => _node;

        public void PreventDefault()
        {
            preventDefault = true;
        }

        [NotNull]
        public ExpressionVisitor Visitor { get; private set; }

        [NotNull]
        public JavascriptCompilationOptions Options { get; private set; }

        /// <summary>
        /// Gets a JavaScript writer, to output JavaScript code as the result of a node conversion.
        /// When this method is used, it marks the context as being used already,
        /// so that the node is not compiled again by any other extension or default behavior of the converter.
        /// </summary>
        public JavascriptWriter GetWriter()
        {
            return _result;
        }

        public void WriteNode(Expression node)
        {
            Visitor.Visit(node);
        }

        public void WriteLambda<T>(Expression<T> expression)
        {
            Visitor.Visit(expression);
        }

        public void WriteExpression<T>(Expression<T> expression)
        {
            Visitor.Visit(expression.Body);
        }
    }
}
