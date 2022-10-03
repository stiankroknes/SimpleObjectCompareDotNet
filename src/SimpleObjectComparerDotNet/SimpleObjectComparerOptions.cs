using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleObjectComparerDotNet;

public abstract class SimpleObjectComparerOptions
{
    /// <summary>
    /// If null values should be ignored.
    /// </summary>
    public bool IgnoreNullValues { get; set; }

    /// <summary>
    /// List of member names to ignore.
    /// </summary>
    /// <value></value>
    public HashSet<string> ExcludeMembersWithName { get; }

    /// <summary>
    /// Only include members with the given attribute(s).
    /// </summary>
    /// <value></value>
    public HashSet<Type> IncludeMembersWithAttribute { get; }

    /// <summary>
    /// Indicates if we should filter by the <see cref="IncludeMembersWithAttribute" />.
    /// </summary>
    public bool FilterMembersByAttributes => IncludeMembersWithAttribute?.Any() ?? false;

    /// <summary>
    /// Exclude properties/members with the Ignore attribute. Default is true.
    /// </summary>
    public bool RespectIgnoreDataMember { get; set; }

    /// <summary>
    /// Separator used in property names. The default is dot.
    /// </summary>
    /// <value></value>
    public string PropertyNameSeparator { get; set; }

    /// <summary>
    /// Options for different value formats.
    /// </summary>
    public ValueFormatOptions ValueFormats { get; set; }

    /// <summary>
    /// Callback to resolve the <see cref="Type" /> of the given member. Default is to use <see cref="object.GetType()"/> if we have a non-null instance, otherwise <see cref="MemberInfo.MemberType"/> is used.
    /// </summary>
    public Func<object?, MemberInfo, Type> CustomValueTypeResolver { get; set; }

    /// <summary>
    /// Callback to create a string value for a given object/value. Default is to use <see cref="object.ToString" />. <see cref="DateTime"/>/<see cref="DateTimeOffset"/> is formatted using <see cref="ValueFormatOptions.DateTimeValueFormat" />.
    /// </summary>
    public Func<object?, ValueFormatOptions, string> CustomValueToStringResolver { get; set; }

    /// <summary>
    /// Callback to resolve the value of a given member. Default is to use GetValue from either <see cref="FieldInfo"/> or <see cref="PropertyInfo"/>.
    /// </summary>
    public Func<object?, MemberInfo, object?> CustomValueResolver { get; set; }

    /// <summary>
    /// Create options with default options.
    /// </summary>
    public SimpleObjectComparerOptions()
    {
        CustomValueResolver = DefaultPropertyValueCallbacks.ValueResolver;
        CustomValueToStringResolver = DefaultPropertyValueCallbacks.StringValueResolver;
        CustomValueTypeResolver = DefaultPropertyValueCallbacks.ValueTypeResolver;
        ValueFormats = new ValueFormatOptions();
        ExcludeMembersWithName = new HashSet<string>(StringComparer.Ordinal);
        IncludeMembersWithAttribute = new HashSet<Type>();
        IgnoreNullValues = false;
        RespectIgnoreDataMember = true;
        PropertyNameSeparator = ".";
    }
}

public class ValueFormatOptions
{
    /// <summary>
    /// Indicate if we should use <see cref="DateTimeValueFormat"/> for <see cref="DateTimeOffset"/> also. Default is false.
    /// </summary>
    public bool UseDateTimeFormatForDateTimeOffset { get; set; }

    /// <summary>
    /// Format string to use for <see cref="DateTime"/> and <see cref="DateTimeOffset"/>. Default is "yyyy-MM-ddTHH:mm:sszzz".
    /// </summary>
    public string DateTimeValueFormat { get; set; }

    /// <summary>
    /// Format string to use for <see cref="DateTimeOffset"/>. Default is "yyyy-MM-ddTHH:mm:ss".
    /// </summary>
    public string DateTimeOffsetValueFormat { get; set; }

    /// <summary>
    /// String value to use when value is null. Default is "null".
    /// </summary>
    /// <value></value>
    public string NullValue { get; set; }

    public ValueFormatOptions()
    {
        DateTimeValueFormat = "yyyy-MM-ddTHH:mm:ss";
        DateTimeOffsetValueFormat = "yyyy-MM-ddTHH:mm:sszzz";
        UseDateTimeFormatForDateTimeOffset = false;
        NullValue = "null";
    }
}

public static class SimpleObjectComparerOptionsExtensions
{
    public static TOptions IncludeMemberWithAttribute<TOptions>(this TOptions options, Type attributeType)
        where TOptions : SimpleObjectComparerOptions
    {
        if (attributeType == null) { throw new ArgumentNullException(nameof(attributeType)); }

        options.IncludeMembersWithAttribute.Add(attributeType);

        return options;
    }

    public static TOptions ExcludeMemberName<TOptions>(this TOptions options, string name)
        where TOptions : SimpleObjectComparerOptions
    {
        if (string.IsNullOrEmpty(name)) { throw new ArgumentException("Exclude name can not be null or empty.", nameof(name)); }

        options.ExcludeMembersWithName.Add(name);

        return options;
    }

    public static TOptions ExcludeMemberName<TOptions>(this TOptions options, IEnumerable<string> memberNames)
        where TOptions : SimpleObjectComparerOptions
    {
        if (memberNames == null) { throw new ArgumentNullException(nameof(memberNames)); }

        options.ExcludeMembersWithName.AddRange(memberNames);

        return options;
    }
}