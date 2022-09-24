using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        Compare(type1, type2, expected, actual, context);

        return context.Results;
    }

    private static void Compare(Type? type1, Type? type2, object? instance1, object? instance2, CompareContext context)
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


    private class CompareContext
    {
        private readonly List<CompareResult> results = new();

        public IReadOnlyList<CompareResult> Results => results;
        public ObjectCompareOptions Options { get; }

        public CompareContext(ObjectCompareOptions options)
        {
            Options = options;
        }

        private string currentPath = string.Empty;
        private string path = string.Empty;
        private bool isIndexPath = false;

        //public void Pop()
        //{
        //    stack.Pop();
        //    isIndexPath = false;
        //}

        //public void Push(string value) => stack.Push(new MemberPath(value, false, Options.PropertyNameSeparator));

        //public string Path => string.Join(Options.PropertyNameSeparator, stack);

        private string GetPath() =>
             !string.IsNullOrEmpty(currentPath)
                ? string.Concat(currentPath, Options.PropertyNameSeparator, path)
                : path;


        //private bool isIndexPath;

        internal void PushIndexPath(int idx)
        {
            isIndexPath = true;
            path = string.Concat(path, '[', idx, ']');
        }

        internal void Add(bool equal, object? value1, object? value2)
        {
            results.Add(new CompareResult(equal, GetPath(), GetStringValue(value1, Options), GetStringValue(value2, Options)));
        }

        internal void AddIndex(bool equal, int index, object? value1, object? value2)
        {
            results.Add(new CompareResult(equal, GetPath(), GetStringValue(value1, Options), GetStringValue(value2, Options)));
        }

        internal void AddNullValue()
        {
            results.Add(new CompareResult(true, GetPath(), Options.ValueFormats.NullValue, Options.ValueFormats.NullValue));
        }

        internal void AddCountResults(List<object> list1, List<object> list2)
        {
            results.Add(new CompareResult(false, GetPath() + '.' + nameof(IList.Count), list1.Count.ToString(), list2.Count.ToString()));
            results.Add(new CompareResult(false, GetPath() + '.' + nameof(Array.Length), list1.Count.ToString(), list2.Count.ToString()));
            results.Add(new CompareResult(false, GetPath() + '.' + nameof(Array.LongLength), list1.Count.ToString(), list2.Count.ToString()));
        }

        internal void PopIndexPath()
        {
            isIndexPath = false;
            //throw new NotImplementedException();
        }

        internal void PushPath(string name)
        {
            path = isIndexPath
                ? string.Concat(path, Options.PropertyNameSeparator, name)
                : name;
        }

        internal void ClearPath()
        {
            path = currentPath;
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
            context.PushIndexPath(idx);

            // TODO: should we do this for each value ? 
            // TODO: handling of same instance ref ?

            scopeName ??= (type1?.Module.ScopeName ?? type2?.Module.ScopeName) ?? string.Empty;

            if (!ClrScope.Contains(scopeName))
            {
                Compare(type1, type2, val1, val2, context);
            }
            else
            {
                // Collection of primitives.
                bool equal = IsEqual((type1 ?? type2), val1, val2, context.Options);
                context.Add(equal, val1, val2);
            }

            context.PopIndexPath();
        }

        bool isListCountEqual = list1.Count == list2.Count;
        if (!isListCountEqual)
        {
            context.AddCountResults(list1, list2);
        }
    }

    private static void ProcessProperty(CompareContext results, Type? type1, Type? type2, object? instance1, object? instance2)
    {
        var fields = GetFields(type1, type2, instance1, instance2, results.Options);
        var properties = GetProperties(type1, type2, instance1, instance2, results.Options);

        var possiblePropertiesWithBackingField = properties.Where(p => fields.Any(field => field.Name.EndsWith(p.Name, StringComparison.Ordinal)));

        foreach (var pi in fields.Concat(properties.Except(possiblePropertiesWithBackingField)))
        {
            var value1 = pi.Value1;
            var value2 = pi.Value2;
            results.PushPath(pi.Name);


            if (pi.Type1.IsClass && !ClrScope.Contains(pi.Type1.Module.ScopeName))
            {
                if (value1 != null && value2 != null)
                {
                    Compare(type1, type2, value1, value2, /*CreatePath(currentPath, pi.Name, results.Options)*/ results);
                }
                else if (!IsEqual(pi.Type1, value1, value2, results.Options))
                {
                    //results.Add(new CompareResult(false, CreatePath(currentPath, pi.Name, results.Options), GetStringValue(value1, results.Options), GetStringValue(value2, results.Options)));
                    results.Add(false, value1, value2);
                }
                else if (value1 == null && value2 == null)
                {
                    //results.Add(new CompareResult(true, CreatePath(currentPath, pi.Name, options), options.ValueFormats.NullValue, options.ValueFormats.NullValue));
                    results.AddNullValue();
                }
            }
            else if (!IsEqual(pi.Type1, value1, value2, results.Options))
            {
                if (value1 is not string &&
                    value1 is IEnumerable enumerable11 && value2 is IEnumerable enumerable22)
                {
                    ProcessCollection(results, pi.Type1.GetActualPropertyType(), pi.Type2?.GetActualPropertyType(), enumerable11, enumerable22);
                }
                else
                {
                    //results.Add(new CompareResult(false, CreatePath(currentPath, pi.Name, results.Options), GetStringValue(value1, results.Options), GetStringValue(value2, results.Options)));
                    results.Add(false, value1, value2);
                }
            }
            else
            {
                //results.Add(new CompareResult(true, CreatePath(currentPath, pi.Name, results.Options), GetStringValue(value1, results.Options), GetStringValue(value2, results.Options)));
                results.Add(true, value1, value2);
            }

            results.ClearPath();
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
}