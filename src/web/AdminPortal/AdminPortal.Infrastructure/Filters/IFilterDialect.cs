using System;

namespace Laso.AdminPortal.Infrastructure.Filters
{
    public interface IFilterDialect
    {
        string Equal { get;}
        string NotEqual { get; }
        string GreaterThan { get; }
        string LessThan { get; }
        string GreaterThanOrEqual { get; }
        string LessThanOrEqual { get; }
        string And { get; }
        string Or { get; }
        string GetStandaloneBoolean(bool value);
        string GetBooleanParameter(bool value);
        string GetStringParameter(string value);
        string GetDateTimeParameter(DateTime value);
        string GetGuidParameter(Guid value);
        string GetPrimitiveParameter(object value);
        string GetEnumParameter(Enum value);
    }
}