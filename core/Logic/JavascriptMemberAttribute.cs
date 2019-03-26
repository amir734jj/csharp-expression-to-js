using System;

namespace core.Logic
{
    /// <summary>
    /// Attribute containing metadata for JavaScript conversion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class JavascriptMemberAttribute : Attribute,
        IJavascriptMemberMetadata
    {
        /// <summary>
        /// Gets or sets the name of the property when converted to JavaScript.
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized - this is an attribute, the user is responsible for initializing this
        public string MemberName { get; set; }
    }
}