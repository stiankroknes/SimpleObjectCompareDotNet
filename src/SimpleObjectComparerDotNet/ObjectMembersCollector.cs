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
            instance is IEnumerable enumerable &&
            context.Options.EnumerableFilter(enumerable))
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
        var currentPath = context.CurrentPath;

        foreach (var value in enumerable)
        {
            var type = value.GetType();
            scopeName ??= type.Module.ScopeName ?? string.Empty;

            if (!ClrScope.Contains(scopeName))
            {
                using (context.AppendIndexedPath(currentPath, idx))
                {
                    ProcessProperty(context, type, value);
                }
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

        var possibleBackingFields = fields.Where(field => properties.Any(prop => field.Name.EndsWith(prop.Name, StringComparison.OrdinalIgnoreCase)));

        var currentPath = context.CurrentPath;

        foreach (var pi in properties.Concat(fields.Except(possibleBackingFields)))
        {
            var value = pi.Value;

            if (pi.Type.IsClass && !ClrScope.Contains(pi.Type.Module.ScopeName))
            {
                if (value != null)
                {
                    using (context.AppendPath(currentPath, pi.Name))
                    {
                        Collect(context, pi.Type, value);
                    }
                }
                else if (!context.Options.IgnoreNullValues)
                {
                    context.AddNull(instanceType, pi.Type, pi.Name);
                }
            }
            else if (value is not string &&
                value is IEnumerable enumerable &&
                context.Options.EnumerableFilter(enumerable))
            {
                using (context.AppendPath(currentPath, pi.Name))
                {
                    ProcessCollection(context, instanceType, enumerable);
                }
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

    private sealed record SimpleMemberInfo(string Name, Type Type, object? Value);

    internal sealed class CollectorContext
    {
        private readonly List<CollectedPropertyValue> results = new();
        private string actualPath = string.Empty;
        private string currentPath = string.Empty;
        private readonly PathDisposable pathDisposable;

        internal IReadOnlyList<CollectedPropertyValue> Results => results;

        internal ObjectMembersCollectorOptions Options { get; }

        public CollectorContext(ObjectMembersCollectorOptions options)
        {
            Options = options;

            if (!string.IsNullOrWhiteSpace(options.RootPathPrefix))
            {
                currentPath = actualPath = options.RootPathPrefix;
            }

            pathDisposable = new PathDisposable(this);
        }

        internal string CurrentPath => actualPath;


        internal IDisposable AppendPath(string startingPath, string path)
        {
            actualPath = currentPath = CreatePath(actualPath, path, Options);
            return pathDisposable.FromPath(startingPath);
        }

        internal IDisposable AppendIndexedPath(string startingPath, int index)
        {
            actualPath = GetIndexedPath(currentPath, index);
            return pathDisposable.FromPath(startingPath);
        }

        private void ResetPath(string startingPath) => actualPath = currentPath = CreatePath(string.Empty, startingPath, Options);

        internal void Add(Type instanceType, Type type, string path, object? value) =>
            results.Add(new CollectedPropertyValue(instanceType, type, CreatePath(actualPath, path, Options), GetStringValue(value, Options)));

        internal void AddNull(Type instanceType, Type type, string path) =>
            results.Add(new CollectedPropertyValue(instanceType, type, CreatePath(actualPath, path, Options), Options.ValueFormats.NullValue));

        internal void AddIndexed(Type instanceType, Type type, int index, object? value) =>
            results.Add(new CollectedPropertyValue(instanceType, type, GetIndexedPath(currentPath, index), GetStringValue(value, Options)));

        private sealed class PathDisposable : IDisposable
        {
            private readonly CollectorContext context;
            private readonly Stack<string> paths = new();

            public PathDisposable(CollectorContext context) => this.context = context;

            public IDisposable FromPath(string currentPath)
            {
                paths.Push(currentPath);
                return this;
            }

            // Note: It is intended to reuse this disposable.
            public void Dispose() => context.ResetPath(paths.Pop());
        }
    }
}