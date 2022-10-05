namespace SimpleObjectComparerDotNet;

/// <summary>
/// Configuration options for the <see cref="ObjectMembersCollector" />.
/// </summary>
/// <value></value>
public class ObjectMembersCollectorOptions : SimpleObjectComparerOptions
{
    /// <summary>
    /// When configured all paths will start with this prefix.
    /// </summary>
    public string RootPathPrefix { get; set; } = default!;
}