using System;
using System.Collections.Generic;
using System.Linq;

namespace Laso.IntegrationMessages.AzureStorageQueue.Extensions
{
    internal static class TypeExtensions
    {
        public static bool Closes(this Type type, Type genericTypeDefinition)
        {
            return Closes(type, genericTypeDefinition, out _);
        }

        public static bool Closes(this Type type, Type genericTypeDefinition, out ICollection<Type[]> genericTypeArguments)
        {
            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Type must be a generic type definition", nameof(genericTypeDefinition));
            }

            var typesToConsider = type.GetHierarchy();

            if (genericTypeDefinition.IsInterface)
            {
                typesToConsider = typesToConsider.Concat(type.GetInterfaces());
            }

            genericTypeArguments = typesToConsider
                .Where(x =>
                    x.IsGenericType
                    && x.GetGenericTypeDefinition() == genericTypeDefinition
                    && x.GenericTypeArguments.Length == genericTypeDefinition.GetGenericArguments().Length
                    && !x.GenericTypeArguments.Any(y => y.IsGenericParameter))
                .Select(x => x.GenericTypeArguments)
                .Distinct(new TypeSequenceEqualityComparer())
                .ToList();

            return genericTypeArguments.Any();
        }

        public static IEnumerable<Type> GetHierarchy(this Type type)
        {
            return type.Concat(type.GetBaseTypes());
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            var baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        private class TypeSequenceEqualityComparer : IEqualityComparer<Type[]>
        {
            public bool Equals(Type[] x, Type[] y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                return x.SequenceEqual(y);
            }

            public int GetHashCode(Type[] obj)
            {
                return obj.Sum(x => x.GetHashCode());
            }
        }
    }
}
