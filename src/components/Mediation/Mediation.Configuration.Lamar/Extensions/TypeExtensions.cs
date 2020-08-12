using System;
using System.Collections.Generic;
using System.Linq;

namespace Laso.Mediation.Configuration.Lamar.Extensions
{
    internal static class TypeExtensions
    {
        public static bool Closes(this Type type, Type genericTypeDefinition)
        {
            return Closes(type, genericTypeDefinition, out _);
        }

        public static bool Closes(this Type type, Type genericTypeDefinition, out Type[] genericTypeArguments)
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

            genericTypeArguments = typesToConsider.FirstOrDefault(x =>
                x.IsGenericType
                && x.GetGenericTypeDefinition() == genericTypeDefinition
                && x.GenericTypeArguments.Length == genericTypeDefinition.GetGenericArguments().Length
                && !x.GenericTypeArguments.Any(y => y.IsGenericParameter))?.GenericTypeArguments;

            return genericTypeArguments != null;
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
    }
}
