using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleObjectComparerDotNet.Tests.Support;

// Case from Fhir hl7 .Net.
// https://github.com/FirelyTeam/firely-net-common/blob/develop/src/Hl7.Fhir.Support.Poco/Model/Base.cs
internal class EnumerableClass : IReadOnlyDictionary<string, object>
{
    public string Test1 { get; set; }
    public string Test2 { get; set; }

    protected virtual IEnumerable<KeyValuePair<string, object>> GetElementPairs()
    {
        if (Test1 is not null) yield return new KeyValuePair<string, object>("test1", Test1);
        if (Test2 is not null) yield return new KeyValuePair<string, object>("test2", Test2);
    }

    // IReadOnlyDictionary
    IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => GetElementPairs().Select(kvp => kvp.Key);

    IEnumerable<object> IReadOnlyDictionary<string, object>.Values => GetElementPairs().Select(kvp => kvp.Value);

    int IReadOnlyCollection<KeyValuePair<string, object>>.Count => GetElementPairs().Count();

    object IReadOnlyDictionary<string, object>.this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

    bool IReadOnlyDictionary<string, object>.ContainsKey(string key) => TryGetValue(key, out _);

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => GetElementPairs().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetElementPairs().GetEnumerator();

    bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value) => TryGetValue(key, out value);

    protected virtual bool TryGetValue(string key, out object value)
    {
        value = default;
        return false;
    }
}