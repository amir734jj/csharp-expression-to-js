using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace core
{
    internal static class TypeHelpers
    {
        private static readonly Type[] numTypes = {
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(short),
                typeof(byte),
                typeof(sbyte),
                typeof(float),
                typeof(double),
                typeof(decimal)
            };

        private static readonly Type[] intTypes = {
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(short),
                typeof(byte),
                typeof(sbyte)
            };

        public static bool IsNumericType(Type type)
        {
            return Array.IndexOf(numTypes, type) >= 0;
        }

        public static bool IsIntegerType(Type type)
        {
            return Array.IndexOf(intTypes, type) >= 0;
        }

        public static bool IsDictionaryType([NotNull] Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return true;
            }

            if (type.IsGenericType)
            {
                var generic = type.GetGenericTypeDefinition();
                if (typeof(IDictionary<,>).IsAssignableFrom(generic))
                    return true;
            }

            return false;
        }

        public static bool IsListType([NotNull] Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (typeof(ICollection).IsAssignableFrom(type))
            {
                return true;
            }

            if (type.IsGenericType)
            {
                var generic = type.GetGenericTypeDefinition();
                if (typeof(ICollection<>).IsAssignableFrom(generic))
                    return true;
            }

            return false;
        }

        public static bool IsEnumerableType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return true;
            }

            if (type.IsGenericType)
            {
                var generic = type.GetGenericTypeDefinition();
                if (typeof(IEnumerable<>).IsAssignableFrom(generic))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TestAttribute<T>(this Type type, [NotNull] Func<T, bool> predicate)
            where T : Attribute
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var attr = type.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault();
            return attr != null && predicate(attr);
        }

        public static bool IsClosureRootType(this Type type)
        {
            if (type.Name.StartsWith("<>"))
            {
                if (type.TestAttribute((CompilerGeneratedAttribute a) => true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}