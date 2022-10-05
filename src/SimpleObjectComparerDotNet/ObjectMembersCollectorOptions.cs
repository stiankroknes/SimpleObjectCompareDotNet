using System;
using System.Collections;

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

    /// <summary>
    /// Filter action to be able to control which IEnumerable collector enumerates, default is true. Action returns true to enumerate, otherwise false.
    /// </summary>
    public Func<IEnumerable, bool> EnumerableFilter { get; set; }

    public ObjectMembersCollectorOptions()
    {
        EnumerableFilter = e => true;
    }
}