using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static SimpleObjectComparerDotNet.Extensions;

namespace SimpleObjectComparerDotNet;

public static class ObjectMembersCollector
{
    public static IReadOnlyList<CollectedPropertyValue> Collect<TType>(TType instance, Action<ObjectMembersCollectorOptions>? configure = null)
        where TType : class => Collect(instance, string.Empty, configure);

    public static IReadOnlyList<CollectedPropertyValue> Collect<TType>(TType instance, string currentPath, Action<ObjectMembersCollectorOptions>? configure = null)
        where TType : class
    {
        var collected = new List<CollectedPropertyValue>();

        var options = new ObjectMembersCollectorOptions();

        configure?.Invoke(options);

        Collect(instance.GetType(), instance, currentPath, collected, options);

        return collected;
    }

    private static void Collect<TType>(Type instanceType, TType instance, string currentPath, List<CollectedPropertyValue> results, ObjectMembersCollectorOptions options)
        where TType : class
    {
        if (instance is not string &&
            instance is IEnumerable enumerable)
        {
            ProcessCollection(instanceType, currentPath, results, enumerable, options);
        }
        else
        {
            ProcessProperty(instanceType, instance, currentPath, results, options);
        }
    }

    private static void ProcessCollection(Type instanceType, string path, List<CollectedPropertyValue> results, IEnumerable enumerable, ObjectMembersCollectorOptions options)
    {
        string? scopeName = null;
        var currentPath = path;
        int idx = 0;

        foreach (var value in enumerable)
        {
            var type = value.GetType();
            scopeName ??= type.Module.ScopeName ?? string.Empty;

            if (!ClrScope.Contains(scopeName))
            {
                Collect(type, value, GetIndexedPath(currentPath, idx), results, options);
            }
            else
            {
                results.Add(new CollectedPropertyValue(instanceType, type, GetIndexedPath(currentPath, idx), GetStringValue(value, options)));
            }

            idx++;
        }
    }

    private static void ProcessProperty<TType>(Type instanceType, TType instance, string currentPath, List<CollectedPropertyValue> results, ObjectMembersCollectorOptions options)
         where TType : class
    {
        var fields = GetFields(instance, instanceType, options);
        var properties = GetProperties(instance, instanceType, options);

        var possiblePropertiesWithBackingField = properties.Where(p => fields.Any(f => f.Name.EndsWith(p.Name, StringComparison.Ordinal)));

        foreach (var pi in fields.Concat(properties.Except(possiblePropertiesWithBackingField)))
        {
            var value = pi.Value;

            if (pi.Type.IsClass && !ClrScope.Contains(pi.Type.Module.ScopeName))
            {
                if (value != null)
                {
                    Collect(pi.Type, value, CreatePath(currentPath, pi.Name, options), results, options);
                }
                else if (!options.IgnoreNullValues)
                {
                    results.Add(new CollectedPropertyValue(instanceType, pi.Type, CreatePath(currentPath, pi.Name, options), options.ValueFormats.NullValue));
                }
            }
            else if (value is not string && value is IEnumerable enumerable11)
            {
                ProcessCollection(instanceType, CreatePath(currentPath, pi.Name, options), results, enumerable11, options);
            }
            else
            {
                if (value == null)
                {
                    if (!options.IgnoreNullValues)
                    {
                        results.Add(new CollectedPropertyValue(instanceType, pi.Type, CreatePath(currentPath, pi.Name, options), options.ValueFormats.NullValue));
                    }
                }
                else
                {
                    results.Add(new CollectedPropertyValue(instanceType, pi.Type, CreatePath(currentPath, pi.Name, options), GetStringValue(value, options)));
                }
            }
        }
    }

    private static IReadOnlyList<SimpleMemberInfo> GetFields<TType>(TType instance, Type type, ObjectMembersCollectorOptions options)
    {
        var memberInfos = MemberInfoResolver.ResolveByFields(type, options);

        return CreateSimpleMemberInfos(instance, memberInfos, options);
    }

    private static IReadOnlyList<SimpleMemberInfo> GetProperties<TType>(TType instance, Type type, ObjectMembersCollectorOptions options)
    {
        var memberInfos = MemberInfoResolver.ResolveByProperties(type, options);

        return CreateSimpleMemberInfos(instance, memberInfos, options);
    }

    private static IReadOnlyList<SimpleMemberInfo> CreateSimpleMemberInfos<TType>(TType instance, IReadOnlyCollection<MemberInfo> memberInfos, ObjectMembersCollectorOptions options)
    {
        if (memberInfos.Count == 0)
        {
            return Array.Empty<SimpleMemberInfo>();
        }

        var simpleInfos = new List<SimpleMemberInfo>();

        foreach (var memberInfo in memberInfos)
        {
            var value = GetValue(instance, memberInfo, options);
            var valueType = GetValueType(value, memberInfo, options);
            simpleInfos.Add(new SimpleMemberInfo(memberInfo.Name, valueType, value));
        }

        return simpleInfos;
    }

    private record SimpleMemberInfo(string Name, Type Type, object? Value);
}