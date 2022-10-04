using System.Diagnostics;

namespace SimpleObjectComparerDotNet;

/// <summary>
/// Details about a member comparison.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class CompareResult
{
    public CompareResult(bool equal, string name, string? expectedValue, string? actualValue)
    {
        Equal = equal;
        Name = name;
        ExpectedValue = expectedValue;
        ActualValue = actualValue;
    }

    /// <summary>
    /// True if comparison is considered equal.
    /// </summary>
    public bool Equal { get; }

    /// <summary>
    /// The member path and name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The expected string value of the member.
    /// </summary>
    public string? ExpectedValue { get; }

    /// <summary>
    /// The actual string value of the member.
    /// </summary>
    public string? ActualValue { get; }

    private string DebuggerDisplay => $"Name = {Name}, ExpectedValue = {ExpectedValue}, ActualValue = {ActualValue}, Equal = {(Equal ? "Y" : "N")}";
}