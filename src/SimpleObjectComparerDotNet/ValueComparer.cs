using System;

namespace SimpleObjectComparerDotNet;

/// <summary>
/// Compare logic configured to handle a specific <see cref="Type"/>.
/// </summary>
public abstract class ValueComparer
{
    public abstract bool IsEqual(object? value1, object? value2, ObjectCompareOptions options);
}

internal class EqualsComparer : ValueComparer
{
    public override bool IsEqual(object? value1, object? value2, ObjectCompareOptions options) => Equals(value1, value2);
}