using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static SimpleObjectComparerDotNet.Extensions;

namespace SimpleObjectComparerDotNet;

public static class ObjectComparer
{
    private static readonly ValueComparer DefaultComparer = new EqualsComparer();

    /// <summary>
    /// Compare the public members of two instances of the same type.
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <param name="expected">The expected object state.</param>
    /// <param name="actual">The object to compare with.</param>
    /// <param name="configure">Callback to customize the comparison configuration.</param>
    /// <returns>List of <see cref="CompareResult"/> results.</returns>
    public static IReadOnlyList<CompareResult> ComparePublicMembers<TValueType>(TValueType expected, TValueType actual, Action<ObjectCompareOptions>? configure = null)
            where TValueType : class
    {
        var options = new ObjectCompareOptions();

        configure?.Invoke(options);

        var context = new CompareContext(options);

        var type1 = expected?.GetType() ?? typeof(TValueType);
        var type2 = actual?.GetType() ?? typeof(TValueType);

        Compare(context, type1, type2, expected, actual);

        return context.Results;
    }

    private static void Compare(CompareContext context, Type? type1, Type? type2, object? instance1, object? instance2)
    {
        if (instance1 is not string &&
            instance1 is IEnumerable enumerable1 && instance2 is IEnumerable enumerable2)
        {
            var actualType1 = type1?.GetActualPropertyType();
            var actualType2 = type2?.GetActualPropertyType();
            ProcessCollection(context, actualType1, actualType2, enumerable1, enumerable2);
        }
        else
        {
            ProcessProperty(context, type1, type2, instance1, instance2);
        }
    }

    private static void ProcessCollection(CompareContext context, Type? type1, Type? type2, IEnumerable enumerable1, IEnumerable enumerable2)
    {
        var list1 = enumerable1.OfType<object>().ToList();
        var list2 = enumerable2.OfType<object>().ToList();

        var max = Math.Max(list1.Count, list2.Count);

        string? scopeName = default;

        for (int idx = 0; idx < max; idx++)
        {
            var val1 = list1.Count > idx ? list1[idx] : default;
            var val2 = list2.Count > idx ? list2[idx] : default;

            // TODO: should we do this for each value ? 
            // TODO: handling of same instance ref ?

            scopeName ??= (type1?.Module.ScopeName ?? type2?.Module.ScopeName) ?? string.Empty;

            if (!ClrScope.Contains(scopeName))
            {
                context.SetIndexedPath(idx);
                Compare(context, type1, type2, val1, val2);
            }
            else
            {
                // Collection of primitives.
                bool equal = IsEqual((type1 ?? type2), val1, val2, context.Options);
                context.AddIndexedResult(equal, idx, val1, val2);
            }
        }

        bool isListCountEqual = list1.Count == list2.Count;
        if (!isListCountEqual)
        {
            context.AddCountResults(list1, list2);
        }
    }

    private static void ProcessProperty(CompareContext context, Type? type1, Type? type2, object? instance1, object? instance2)
    {
        var fields = GetFields(type1, type2, instance1, instance2, context.Options);
        var properties = GetProperties(type1, type2, instance1, instance2, context.Options);

        var possiblePropertiesWithBackingField = properties.Where(p => fields.Any(field => field.Name.EndsWith(p.Name, StringComparison.Ordinal)));

        foreach (var pi in fields.Concat(properties.Except(possiblePropertiesWithBackingField)))
        {
            var value1 = pi.Value1;
            var value2 = pi.Value2;

            if (pi.Type1.IsClass && !ClrScope.Contains(pi.Type1.Module.ScopeName))
            {
                if (value1 != null && value2 != null)
                {
                    context.SetRootPath(pi.Name);
                    Compare(context, type1, type2, value1, value2);
                }
                else if (!IsEqual(pi.Type1, value1, value2, context.Options))
                {
                    context.AddResult(false, pi.Name, value1, value2);
                }
                else if (value1 == null && value2 == null)
                {
                    context.AddNullValueResult(pi.Name);
                }
            }
            else if (!IsEqual(pi.Type1, value1, value2, context.Options))
            {
                if (value1 is not string &&
                    value1 is IEnumerable enumerable11 && value2 is IEnumerable enumerable22)
                {
                    context.SetRootPath(pi.Name);
                    ProcessCollection(context, pi.Type1.GetActualPropertyType(), pi.Type2?.GetActualPropertyType(), enumerable11, enumerable22);
                }
                else
                {
                    context.AddResult(false, pi.Name, value1, value2);
                }
            }
            else
            {
                context.AddResult(true, pi.Name, value1, value2);
            }
        }
    }

    private static bool IsEqual(Type? type, object? value1, object? value2, ObjectCompareOptions options)
    {
        if (type != null && options.CustomValueComparers.TryGetValue(type, out var compareLogic))
        {
            return compareLogic.IsEqual(value1, value2, options);
        }

        return DefaultComparer.IsEqual(value1, value2, options);
    }

    private static IReadOnlyList<SimpleMemberInfo> GetFields(Type? type1, Type? type2, object? instance1, object? instance2, ObjectCompareOptions options)
    {
        var actualType1 = instance1?.GetType() ?? type1 ?? typeof(object);
        var actualType2 = instance2?.GetType() ?? type2 ?? typeof(object);

        var type1Members = MemberInfoResolver.ResolveByFields(actualType1, options);
        var type2Members = MemberInfoResolver.ResolveByFields(actualType2, options);

        return CreateSimpleMemberInfos(instance1, instance2, type1Members, type2Members, options);
    }

    private static IReadOnlyList<SimpleMemberInfo> GetProperties(Type? type1, Type? type2, object? instance1, object? instance2, ObjectCompareOptions options)
    {
        var actualType1 = instance1?.GetType() ?? type1 ?? typeof(object);
        var actualType2 = instance2?.GetType() ?? type2 ?? typeof(object);

        var type1Members = MemberInfoResolver.ResolveByProperties(actualType1, options);
        var type2Members = MemberInfoResolver.ResolveByProperties(actualType2, options);

        return CreateSimpleMemberInfos(instance1, instance2, type1Members, type2Members, options);
    }

    private static IReadOnlyList<SimpleMemberInfo> CreateSimpleMemberInfos(object? instance1, object? instance2, IReadOnlyCollection<MemberInfo> type1Members, IReadOnlyCollection<MemberInfo> type2Members, ObjectCompareOptions options)
    {
        var simpleMemberInfos = new List<SimpleMemberInfo>();

        foreach (var type1Member in type1Members)
        {
            simpleMemberInfos.Add(CreateSimpleMemberInfo(instance1, instance2, type1Member, type2Members, options));
        }

        return simpleMemberInfos;

        static SimpleMemberInfo CreateSimpleMemberInfo(object? instance1, object? instance2, MemberInfo memberInfo, IReadOnlyCollection<MemberInfo> type2Members, ObjectCompareOptions options)
        {
            var value1 = GetValue(instance1, memberInfo, options);
            var valueType1 = GetValueType(value1, memberInfo, options);

            var type2MemberInfo = type2Members.SingleOrDefault(p2 => p2.Name == memberInfo.Name);
            if (type2MemberInfo != null)
            {
                var value2 = GetValue(instance2, type2MemberInfo, options);
                var valueType2 = GetValueType(value2, type2MemberInfo, options);
                return new SimpleMemberInfo(memberInfo.Name, valueType1, valueType2, value1, value2);
            }

            return new SimpleMemberInfo(memberInfo.Name, valueType1, typeof(object), value1, null);
        }
    }

    private record SimpleMemberInfo(string Name, Type Type1, Type Type2, object? Value1, object? Value2);

    internal class CompareContext
    {
        private readonly List<CompareResult> results = new();
        private string currentPath = string.Empty;
        private string rootPath = string.Empty;

        internal IReadOnlyList<CompareResult> Results => results;

        internal ObjectCompareOptions Options { get; }

        public CompareContext(ObjectCompareOptions options)
        {
            Options = options;
        }

        internal void SetRootPath(string path) => currentPath = rootPath = CreatePath(currentPath, path, Options);

        internal void SetIndexedPath(int index) => currentPath = GetIndexedPath(rootPath, index);

        internal void AddResult(bool equal, string path, object? value1, object? value2) =>
            results.Add(new CompareResult(equal, CreatePath(currentPath, path, Options), GetStringValue(value1, Options), GetStringValue(value2, Options)));
        internal void AddNullValueResult(string path) =>
            results.Add(new CompareResult(true, CreatePath(currentPath, path, Options), Options.ValueFormats.NullValue, Options.ValueFormats.NullValue));

        internal void AddIndexedResult(bool equal, int index, object? value1, object? value2) =>
            results.Add(new CompareResult(equal, GetIndexedPath(rootPath, index), GetStringValue(value1, Options), GetStringValue(value2, Options)));

        internal void AddCountResults(List<object> list1, List<object> list2)
        {
            results.Add(new CompareResult(false, CreatePath(rootPath, nameof(IList.Count), Options), list1.Count.ToString(), list2.Count.ToString()));
            results.Add(new CompareResult(false, CreatePath(rootPath, nameof(Array.Length), Options), list1.Count.ToString(), list2.Count.ToString()));
            results.Add(new CompareResult(false, CreatePath(rootPath, nameof(Array.LongLength), Options), list1.Count.ToString(), list2.Count.ToString()));
        }
    }
}