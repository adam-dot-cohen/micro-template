﻿using System;
using System.Reflection;
using Laso.Identity.Infrastructure.Extensions;

namespace Laso.Identity.Infrastructure.Filters.FilterPropertyMappers
{
    public class EnumFilterPropertyMapper : IFilterPropertyMapper
    {
        public bool CanMap(PropertyInfo entityProperty)
        {
            return entityProperty.PropertyType.GetNonNullableType().IsEnum;
        }

        public string MapToQueryParameter(IFilterDialect dialect, PropertyInfo entityProperty, object value)
        {
            if (value is Enum @enum)
                return dialect.GetEnum(@enum);

            return dialect.GetPrimitive(value);
        }
    }
}