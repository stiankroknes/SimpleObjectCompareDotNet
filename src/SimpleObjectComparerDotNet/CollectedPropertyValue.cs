using System;
using System.Diagnostics;

namespace SimpleObjectComparerDotNet;

/// <summary>
/// Details about collected member.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class CollectedPropertyValue
{
    public CollectedPropertyValue(Type parentType, Type type, string name, string value)
    {
        ParentType = parentType;
        Type = type;
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Type of the parent object.
    /// </summary>
    public Type ParentType { get; }

    /// <summary>
    /// The type of the member.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The member name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The member string value.
    /// </summary>
    public string Value { get; }

    private string DebuggerDisplay => $"Name = {Name}, Value = {Value}, Type = {Type}, ParentType = {ParentType}";
}