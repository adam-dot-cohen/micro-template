﻿using System;
using System.Collections.Generic;
using System.Linq;
using Laso.AdminPortal.Core.Extensions;

namespace Laso.AdminPortal.Infrastructure.Extensions
{
    public static class TypeExtensions
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

            var typesToConsider = type.GetConcreteHierarchy();

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

        public static IEnumerable<Type> GetConcreteHierarchy(this Type type)
        {
            return type.Concat(type.GetConcreteBaseTypes());
        }

        public static IEnumerable<Type> GetConcreteBaseTypes(this Type type)
        {
            var baseType = type.BaseType;

            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        public static Type GetNonNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static Type GetListType(this Type listType)
        {
            return listType.Closes(typeof(IEnumerable<>), out var genericTypeArguments)
                ? genericTypeArguments[0]
                : null;
        }
    }
}
