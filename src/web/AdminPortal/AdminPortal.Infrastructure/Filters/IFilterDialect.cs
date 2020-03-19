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
        string GetBoolean(bool value);
        string GetString(string value);
        string GetDateTime(DateTime value);
        string GetGuid(Guid value);
        string GetPrimitive(object value);
        string GetEnum(Enum value);
    }
}