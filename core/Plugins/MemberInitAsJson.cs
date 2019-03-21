using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace core.Plugins
{
    public class MemberInitAsJson : JavascriptConversionExtension
    {
        [CanBeNull]
        public Predicate<Type> TypePredicate { get; }

        [CanBeNull]
        public Type[] NewObjectTypes { get; }

        public static readonly MemberInitAsJson ForAllTypes = new MemberInitAsJson();

        /// <summary>
        /// Initializes a new instance of <see cref="MemberInitAsJson"/>,
        /// so that member initializations of types in `newObjectTypes` are converted to JSON.
        /// </summary>
        public MemberInitAsJson([NotNull] params Type[] newObjectTypes)
        {
            if (newObjectTypes == null)
                throw new ArgumentNullException(nameof(newObjectTypes));
            if (newObjectTypes.Length == 0)
                throw new ArgumentException("Argument is empty collection. Maybe you are looking for `MemberInitAsJson.ForAllTypes`.", nameof(newObjectTypes));

            NewObjectTypes = newObjectTypes;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MemberInitAsJson"/>,
        /// so that member initializations of any types are converted to JSON.
        /// </summary>
        private MemberInitAsJson()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MemberInitAsJson"/>,
        /// so that member initializations of types that pass the `typePredicate` criteria are converted to JSON.
        /// </summary>
        public MemberInitAsJson([NotNull] Predicate<Type> typePredicate)
        {
            TypePredicate = typePredicate ?? throw new ArgumentNullException(nameof(typePredicate));
        }

        public override void ConvertToJavascript(JavascriptConversionContext context)
        {
            if (!(context.Node is MemberInitExpression initExpr))
                return;
            var typeOk1 = NewObjectTypes?.Contains(initExpr.Type) ?? false;
            var typeOk2 = TypePredicate?.Invoke(initExpr.Type) ?? false;
            var typeOk3 = NewObjectTypes == null && TypePredicate == null;
            if (!typeOk1 && !typeOk2 && !typeOk3)
                return;
            if (initExpr.NewExpression.Arguments.Count > 0)
                return;
            if (initExpr.Bindings.Any(mb => mb.BindingType != MemberBindingType.Assignment))
                return;

            context.PreventDefault();
            var writer = context.GetWriter();
            using (writer.Operation(0))
            {
                writer.Write('{');

                var posStart = writer.Length;
                foreach (var assignExpr in initExpr.Bindings.Cast<MemberAssignment>())
                {
                    if (writer.Length > posStart)
                        writer.Write(',');

                    var metadataProvider = context.Options.GetMetadataProvider();
                    var meta = metadataProvider.GetMemberMetadata(assignExpr.Member);
                    var memberName = meta?.MemberName;
                    Debug.Assert(!string.IsNullOrEmpty(memberName), "!string.IsNullOrEmpty(memberName)");
                    if (Regex.IsMatch(memberName, @"^\w[\d\w]*$"))
                        writer.Write(memberName);
                    else
                        writer.WriteLiteral(memberName);

                    writer.Write(':');
                    context.Visitor.Visit(assignExpr.Expression);
                }

                writer.Write('}');
            }
        }
    }
}