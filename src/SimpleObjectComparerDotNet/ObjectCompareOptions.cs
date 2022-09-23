using System;
using System.Collections.Generic;

namespace SimpleObjectComparerDotNet;

/// <summary>
/// Configuration options for the <see cref="ObjectCompare" />.
/// </summary>
/// <value></value>
public class ObjectCompareOptions : SimpleObjectComparerOptions
{
    private readonly Dictionary<Type, ValueComparer> customValueComparerMap = new();

    public ObjectCompareOptions AddCustomTypeComparer(Type type, ValueComparer comparer)
    {
        if (type == null) { throw new ArgumentNullException("type"); }
        if (comparer == null) { throw new ArgumentNullException(nameof(comparer)); }

        if (!customValueComparerMap.ContainsKey(type))
        {
            customValueComparerMap.Add(type, comparer);
        }

        return this;
    }

    public IReadOnlyDictionary<Type, ValueComparer> CustomValueComparers => customValueComparerMap;
}