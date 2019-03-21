using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using core.Compat;

namespace core
{
    /// <summary>
    /// Provides metadata about the objects that are going to be converted to JavaScript in some way.
    /// </summary>
    public class AttributeJavascriptMetadataProvider : JavascriptMetadataProvider
    {
        private readonly object _locker = new object();

        private IJavascriptMemberMetadata GetMemberMetadataNoCache([NotNull] MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            var attr0 = memberInfo
                .GetCustomAttributes(typeof(JavascriptMemberAttribute), true)
                .OfType<IJavascriptMemberMetadata>()
                .SingleOrDefault();

            if (attr0 != null)
                return attr0;

            var jsonAttr = memberInfo
                .GetCustomAttributes(true)
                .Where(a => a.GetType().Name == "JsonPropertyAttribute")
                .Select(ConvertJsonAttribute)
                .SingleOrDefault();

            if (jsonAttr != null)
                return jsonAttr;

            return new JavascriptMemberAttribute
            {
                MemberName = memberInfo.Name
            };
        }

        private Dictionary<MemberInfo, IJavascriptMemberMetadata> cache
            = new Dictionary<MemberInfo, IJavascriptMemberMetadata>();

        private IJavascriptMemberMetadata GetMemberMetadataWithCache([NotNull] MemberInfo memberInfo)
        {
            if (cache.TryGetValue(memberInfo, out var value))
                return value;

            lock (_locker)
            {
                if (cache.TryGetValue(memberInfo, out value))
                    return value;

                var meta = GetMemberMetadataNoCache(memberInfo);

                // Have to create a new instance here, because readers don't have any
                // syncronization with writers. I.e. when executing the line
                // `cache.TryGetValue` outside of the lock (above), if the same
                // instance was added to, then that method could get the wrong result.
                var newCache = new Dictionary<MemberInfo, IJavascriptMemberMetadata>(cache)
                    {
                        [memberInfo] = meta
                    };

                // This memory barrier is needed if this class is ever used in
                // a weaker memory model implementation, allowed by the CLR
                // specification.
                Interlocked.MemoryBarrier();

                cache = newCache;
                return meta;
            }
        }

        /// <summary>
        /// Gets metadata about a property that is going to be used in JavaScript code.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public override IJavascriptMemberMetadata GetMemberMetadata([NotNull] MemberInfo memberInfo)
        {
            return GetMemberMetadata(memberInfo, UseCache);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use cache by default.
        /// Applyes to `GetMemberMetadata` overload without `useCache` parameter.
        /// </summary>
        public bool UseCache { get; set; }

        /// <summary>
        /// Gets metadata about a property that is going to be used in JavaScript code.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public IJavascriptMemberMetadata GetMemberMetadata([NotNull] MemberInfo memberInfo, bool useCache)
        {
            if (useCache)
                return GetMemberMetadataWithCache(memberInfo);

            return GetMemberMetadataNoCache(memberInfo);
        }

        private IJavascriptMemberMetadata ConvertJsonAttribute(object attr)
        {
            var type = attr.GetType();
            var accessor = GetAccessors(type);

            var memberName = accessor.PropertyNameGetter?.Invoke(attr);

            // TODO: supporting JsonPropertyAttribute members would be nice
            //  e.g. NullValueHandling.Ignore - does not write variable to javascript object if the value is null
            if (string.IsNullOrEmpty(memberName))
            {
                return null;
            }

            return new JavascriptMemberAttribute
            {
                MemberName = memberName
            };
        }

        class Accessors
        {
            public Func<object, string> PropertyNameGetter { get; set; }
        }

        private Dictionary<Type, Accessors> _accessors = new Dictionary<Type, Accessors>();

        private Accessors GetAccessors(Type type)
        {
            Accessors value;
            if (_accessors.TryGetValue(type, out value))
                return value;

            lock (_locker)
            {
                if (_accessors.TryGetValue(type, out value))
                    return value;

                var accessor = new Accessors
                {
                    PropertyNameGetter = type.GetProperty("PropertyName")?.MakeGetterDelegate<string>()
                };

                // have to create a new instance here, because readers don't have any
                // synchronization with writers.
                var newAccessors = new Dictionary<Type, Accessors>(_accessors)
                    {
                        [type] = accessor
                    };

                // This memory barrier is needed if this class is ever used in
                // a weaker memory model implementation, allowed by the CLR
                // specification.
                Interlocked.MemoryBarrier();

                _accessors = newAccessors;
                return accessor;
            }
        }
    }
}