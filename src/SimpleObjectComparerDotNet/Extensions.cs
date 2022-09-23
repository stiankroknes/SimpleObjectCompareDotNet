using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleObjectComparerDotNet;

internal static class Extensions
{
    // Check of "CommonLanguageRuntimeLibrary" is needed because string is also a class
    internal static readonly IReadOnlyList<string> ClrScope = new string[] { "CommonLanguageRuntimeLibrary", "System.Private.CoreLib.dll" };

    internal static string CreatePath(string path, string name, SimpleObjectComparerOptions options) =>
        string.IsNullOrWhiteSpace(path)
            ? name
            : string.Concat(path, options.PropertyNameSeparator, name);

    internal static string GetIndexedPath(string currentPath, int index) => string.Concat(currentPath, '[', index, ']');

    internal static object? GetValue(object? instance, MemberInfo memberInfo, SimpleObjectComparerOptions options) => options.CustomValueResolver(instance, memberInfo);

    internal static string GetStringValue(object? value, SimpleObjectComparerOptions options) => options.CustomValueToStringResolver(value, options.ValueFormats);

    internal static Type GetValueType(object? value, MemberInfo memberInfo, SimpleObjectComparerOptions options) => options.CustomValueTypeResolver(value, memberInfo);


    internal static Type GetUnderlyingType(this MemberInfo member) => (object)member.MemberType switch
    {
        MemberTypes.Event => ((EventInfo)member).EventHandlerType,
        MemberTypes.Field => ((FieldInfo)member).FieldType,
        MemberTypes.Method => ((MethodInfo)member).ReturnType,
        MemberTypes.Property => ((PropertyInfo)member).PropertyType,
        _ => throw new ArgumentException($"{nameof(MemberInfo)} must be of type {nameof(EventInfo)}, {nameof(FieldInfo)}, {nameof(MethodInfo)}, or {nameof(PropertyInfo)}."),
    };

    internal static object? GetValue(this MemberInfo memberInfo, object instance) => memberInfo.MemberType switch
    {
        MemberTypes.Field => ((FieldInfo)memberInfo).GetValue(instance),
        MemberTypes.Property => ((PropertyInfo)memberInfo).GetValue(instance),
        _ => throw new ArgumentException($"{nameof(MemberInfo)} must be of type {nameof(FieldInfo)} or {nameof(PropertyInfo)}."),
    };

    internal static HashSet<T> AddRange<T>(this HashSet<T> set, IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            set.Add(value);
        }

        return set;
    }

    internal static Type GetActualPropertyType(this Type type)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return elementType ?? throw new ArgumentException("Could not resolve element type from array type.", nameof(type));
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        // the Type implements/extends IEnumerable<T>
        var enumType = type
            .GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(t => t.GenericTypeArguments[0])
            .FirstOrDefault();

        return enumType ?? throw new ArgumentException("Does not represent an enumerable type.", nameof(type));
    }
}