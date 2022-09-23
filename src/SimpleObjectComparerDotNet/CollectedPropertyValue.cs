using System;
using System.Diagnostics;

namespace SimpleObjectComparerDotNet;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CollectedPropertyValue
{
    public CollectedPropertyValue(Type parentType, Type type, string name, string value)
    {
        ParentType = parentType;
        Type = type;
        Name = name;
        Value = value;
    }
    
    public Type ParentType { get; }
    public Type Type { get; }
    public string Name { get; }
    public string Value { get; }

    private string DebuggerDisplay => $"Name = {Name}, Value = {Value}, Type = {Type}, ParentType = {ParentType}";
}