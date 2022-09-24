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
        where TType : class
    {
        var options = new ObjectMembersCollectorOptions();

        configure?.Invoke(options);

        var context = new CollectorContext(options);

        Collect(context, instance.GetType(), instance);

        return context.Results;
    }

    private static void Collect<TType>(CollectorContext context, Type instanceType, TType instance)
        where TType : class
    {
        if (instance is not string &&
            instance is IEnumerable enumerable)
        {
            ProcessCollection(context, instanceType, enumerable);
        }
        else
        {
            ProcessProperty(context, instanceType, instance);
        }
    }

    private static void ProcessCollection(CollectorContext context, Type instanceType, IEnumerable enumerable)
    {
        string? scopeName = null;
        int idx = 0;

        foreach (var value in enumerable)
        {
            var type = value.GetType();
            scopeName ??= type.Module.ScopeName ?? string.Empty;

            if (!ClrScope.Contains(scopeName))
            {
                context.SetIndexedPath(idx);
                ProcessProperty(context, type, value);
            }
            else
            {
                context.AddIndexed(instanceType, type, idx, value);
            }

            idx++;
        }
    }

    private static void ProcessProperty<TType>(CollectorContext context, Type instanceType, TType instance)
         where TType : class
    {
        var fields = GetFields(instance, instanceType, context.Options);
        var properties = GetProperties(instance, instanceType, context.Options);

        var possiblePropertiesWithBackingField = properties.Where(p => fields.Any(f => f.Name.EndsWith(p.Name, StringComparison.Ordinal)));

        foreach (var pi in fields.Concat(properties.Except(possiblePropertiesWithBackingField)))
        {
            var value = pi.Value;

            if (pi.Type.IsClass && !ClrScope.Contains(pi.Type.Module.ScopeName))
            {
                if (value != null)
                {
                    context.SetRootPath(pi.Name);
                    Collect(context, pi.Type, value);
                }
                else if (!context.Options.IgnoreNullValues)
                {
                    context.AddNull(instanceType, pi.Type, pi.Name);
                }
            }
            else if (value is not string && value is IEnumerable enumerable)
            {
                context.SetRootPath(pi.Name);
                ProcessCollection(context, instanceType, enumerable);
            }
            else
            {
                if (value == null)
                {
                    if (!context.Options.IgnoreNullValues)
                    {
                        context.AddNull(instanceType, pi.Type, pi.Name);
                    }
                }
                else
                {
                    context.Add(instanceType, pi.Type, pi.Name, value);
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

    internal class CollectorContext
    {
        private readonly List<CollectedPropertyValue> results = new();
        private string currentPath = string.Empty;
        private string rootPath = string.Empty;

        internal IReadOnlyList<CollectedPropertyValue> Results => results;

        internal ObjectMembersCollectorOptions Options { get; }

        public CollectorContext(ObjectMembersCollectorOptions options)
        {
            Options = options;
        }

        internal void SetRootPath(string path) => currentPath = rootPath = CreatePath(currentPath, path, Options);

        internal void SetIndexedPath(int index) => currentPath = GetIndexedPath(rootPath, index);

        internal void Add(Type instanceType, Type type, string path, object? value) =>
            results.Add(new CollectedPropertyValue(instanceType, type, CreatePath(currentPath, path, Options), GetStringValue(value, Options)));

        internal void AddNull(Type instanceType, Type type, string path) =>
            results.Add(new CollectedPropertyValue(instanceType, type, CreatePath(currentPath, path, Options), Options.ValueFormats.NullValue));

        internal void AddIndexed(Type instanceType, Type type, int index, object? value) =>
            results.Add(new CollectedPropertyValue(instanceType, type, GetIndexedPath(rootPath, index), GetStringValue(value, Options)));
    }
}