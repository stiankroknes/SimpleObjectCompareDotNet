using System;
using System.Reflection;

namespace SimpleObjectComparerDotNet;

public static class DefaultPropertyValueCallbacks
{
    public static Type ValueTypeResolver(object? value, MemberInfo memberInfo) =>
            value?.GetType() ?? memberInfo.GetUnderlyingType();

    public static string StringValueResolver(object? value, ValueFormatOptions valueFormatters) => value switch
    {
        DateTimeOffset dst => dst.ToString(valueFormatters.UseDateTimeFormatForDateTimeOffset ? valueFormatters.DateTimeValueFormat : valueFormatters.DateTimeOffsetValueFormat),
        DateTime d => d.ToString(valueFormatters.DateTimeValueFormat),
        _ => value?.ToString() ?? valueFormatters.NullValue
    };

    public static object? ValueResolver(object? instance, MemberInfo memberInfo)
    {
        // TODO: impl register Type to value handlers?
        if (instance is Uri uri)
        {
            return uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.OriginalString; //  uri.ToString();
        }

        if (instance == null)
        {
            return default;
        }

        return memberInfo.GetValue(instance);
    }
}