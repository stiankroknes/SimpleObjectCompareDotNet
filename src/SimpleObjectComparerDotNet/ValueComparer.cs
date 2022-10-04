using System;

namespace SimpleObjectComparerDotNet;

/// <summary>
/// Compare logic to handle comparing two values.
/// </summary>
public abstract class ValueComparer
{
    public abstract bool IsEqual(object? value1, object? value2, ObjectCompareOptions options);
}

internal sealed class EqualsComparer : ValueComparer
{
    public override bool IsEqual(object? value1, object? value2, ObjectCompareOptions options) => Equals(value1, value2);
}