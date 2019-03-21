using System;
using System.Reflection;

namespace core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class JavascriptMetadataProvider
    {
        /// <summary>
        /// Gets or sets the default metadata provider.
        /// The default is the <see cref="AttributeJavascriptMetadataProvider"/> class, but it can be changed.
        /// </summary>
        [NotNull] private static JavascriptMetadataProvider _default = new AttributeJavascriptMetadataProvider();

        /// <summary>
        /// Gets or sets the default metadata provider.
        /// The default is the <see cref="AttributeJavascriptMetadataProvider"/> class, but it can be changed.
        /// </summary>
        public static JavascriptMetadataProvider Default
        {
            get => _default;
            set => _default = value ?? throw new ArgumentNullException(nameof(value), "Cannot set this property to null.");
        }

        /// <summary>
        /// Gets metadata about a property that is going to be used in JavaScript code.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        [NotNull]
        public abstract IJavascriptMemberMetadata GetMemberMetadata(MemberInfo memberInfo);
    }
}