using System.Diagnostics;

namespace SimpleObjectComparerDotNet;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CompareResult
{
    public CompareResult(bool equal, string name, string? expectedValue, string? actualValue)
    {
        Equal = equal;
        Name = name;
        ExpectedValue = expectedValue;
        ActualValue = actualValue;
    }

    public bool Equal { get; }
    public string Name { get; }

    public string? ExpectedValue { get; }
    public string? ActualValue { get; }

    private string DebuggerDisplay => $"Name = {Name}, ExpectedValue = {ExpectedValue}, ActualValue = {ActualValue}, Equal = {(Equal ? "Y" : "N")}";
}